import { GraphQLError } from 'graphql';
import { Prisma } from '@prisma/client';
import { Context } from '../../types/context';
import { requireAuth, requireRole } from '../../middleware/auth.middleware';

export const orderResolvers = {
  Query: {
    orders: async (_: any, { pagination, sort, filter }: any, context: Context) => {
      requireAuth(context);

      const page = pagination?.page || 1;
      const pageSize = Math.min(pagination?.pageSize || 20, 100);
      const skip = (page - 1) * pageSize;

      const where: any = {};

      if (filter) {
        if (filter.status) {
          where.status = filter.status;
        }
        if (filter.customerId) {
          where.customerId = filter.customerId;
        }
        if (filter.dateRange) {
          where.createdAt = {};
          if (filter.dateRange.from) {
            where.createdAt.gte = new Date(filter.dateRange.from);
          }
          if (filter.dateRange.to) {
            where.createdAt.lte = new Date(filter.dateRange.to);
          }
        }
      }

      const orderBy: any = sort
        ? { [sort.field]: sort.order.toLowerCase() }
        : { createdAt: 'desc' };

      const [orders, total] = await Promise.all([
        context.prisma.order.findMany({
          where,
          skip,
          take: pageSize,
          orderBy,
          include: {
            customer: true,
            createdBy: true,
            discount: true,
            orderItems: {
              include: {
                product: true,
              },
            },
          },
        }),
        context.prisma.order.count({ where }),
      ]);

      return {
        orders,
        total,
        page,
        pageSize,
        totalPages: Math.ceil(total / pageSize),
      };
    },

    order: async (_: any, { id }: any, context: Context) => {
      requireAuth(context);

      const order = await context.prisma.order.findUnique({
        where: { id },
        include: {
          customer: true,
          createdBy: true,
          discount: true,
          orderItems: {
            include: {
              product: true,
            },
          },
        },
      });

      if (!order) {
        throw new GraphQLError('Order not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      return order;
    },

    orderByNumber: async (_: any, { orderNumber }: any, context: Context) => {
      requireAuth(context);

      const order = await context.prisma.order.findUnique({
        where: { orderNumber },
        include: {
          customer: true,
          createdBy: true,
          discount: true,
          orderItems: {
            include: {
              product: true,
            },
          },
        },
      });

      if (!order) {
        throw new GraphQLError('Order not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      return order;
    },
  },

  Mutation: {
    createOrder: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);

      if (!context.user) {
        throw new GraphQLError('User not authenticated', {
          extensions: { code: 'UNAUTHENTICATED' },
        });
      }

      // Validate items
      if (!input.items || input.items.length === 0) {
        throw new GraphQLError('Order must have at least one item', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      // Generate order number
      const orderNumber = `ORD-${Date.now()}-${Math.floor(Math.random() * 1000)}`;

      // Calculate subtotal
      let subtotal = new Prisma.Decimal(0);
      const orderItemsData: any[] = [];

      for (const item of input.items) {
        // Check product exists and has stock
        const product = await context.prisma.product.findUnique({
          where: { id: item.productId },
        });

        if (!product) {
          throw new GraphQLError(`Product ${item.productId} not found`, {
            extensions: { code: 'NOT_FOUND' },
          });
        }

        if (product.stock < item.quantity) {
          throw new GraphQLError(`Insufficient stock for product ${product.name}`, {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }

        const itemSubtotal = new Prisma.Decimal(item.unitPrice).mul(item.quantity);
        subtotal = subtotal.add(itemSubtotal);

        orderItemsData.push({
          productId: item.productId,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
          subtotal: itemSubtotal,
          discountAmount: new Prisma.Decimal(0),
          total: itemSubtotal,
        });
      }

      // Calculate discount
      let discountAmount = new Prisma.Decimal(0);
      let discountId = null;

      if (input.discountId) {
        const discount = await context.prisma.discount.findUnique({
          where: { id: input.discountId },
        });

        if (!discount) {
          throw new GraphQLError('Discount not found', {
            extensions: { code: 'NOT_FOUND' },
          });
        }

        if (!discount.isActive) {
          throw new GraphQLError('Discount is not active', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }

        // Check usage limit
        if (discount.usageLimit && discount.usageCount >= discount.usageLimit) {
          throw new GraphQLError('Discount usage limit reached', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }

        // Check dates
        const now = new Date();
        if (discount.startDate && discount.startDate > now) {
          throw new GraphQLError('Discount not yet valid', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }
        if (discount.endDate && discount.endDate < now) {
          throw new GraphQLError('Discount has expired', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }

        // Check minimum purchase
        if (discount.minPurchase && subtotal.lt(discount.minPurchase)) {
          throw new GraphQLError(`Minimum purchase of ${discount.minPurchase} required`, {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }

        // Calculate discount amount
        if (discount.type === 'PERCENTAGE') {
          discountAmount = subtotal.mul(discount.value).div(100);
          if (discount.maxDiscount && discountAmount.gt(discount.maxDiscount)) {
            discountAmount = discount.maxDiscount;
          }
        } else if (discount.type === 'FIXED_AMOUNT') {
          discountAmount = discount.value;
        }

        discountId = discount.id;
      }

      // Calculate total
      const taxAmount = new Prisma.Decimal(input.taxAmount || 0);
      const total = subtotal.sub(discountAmount).add(taxAmount);

      // Create order in transaction
      const order = await context.prisma.$transaction(async (tx) => {
        // Create order
        const newOrder = await tx.order.create({
          data: {
            orderNumber,
            customerId: input.customerId || null,
            userId: context.user!.id,
            status: 'PENDING',
            subtotal,
            discountId,
            discountAmount,
            taxAmount,
            total,
            notes: input.notes || null,
            orderItems: {
              create: orderItemsData,
            },
          },
          include: {
            customer: true,
            createdBy: true,
            discount: true,
            orderItems: {
              include: {
                product: true,
              },
            },
          },
        });

        // Update product stock
        for (const item of input.items) {
          await tx.product.update({
            where: { id: item.productId },
            data: {
              stock: {
                decrement: item.quantity,
              },
              popularity: {
                increment: item.quantity,
              },
            },
          });
        }

        // Update discount usage count
        if (discountId) {
          await tx.discount.update({
            where: { id: discountId },
            data: {
              usageCount: {
                increment: 1,
              },
            },
          });
        }

        // Update customer total spent
        if (input.customerId) {
          await tx.customer.update({
            where: { id: input.customerId },
            data: {
              totalSpent: {
                increment: total,
              },
            },
          });
        }

        return newOrder;
      });

      return order;
    },

    updateOrder: async (_: any, { id, input }: any, context: Context) => {
      requireAuth(context);

      const existingOrder = await context.prisma.order.findUnique({
        where: { id },
      });

      if (!existingOrder) {
        throw new GraphQLError('Order not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      // Update completion date if status is COMPLETED
      const updateData: any = { ...input };
      if (input.status === 'COMPLETED' && existingOrder.status !== 'COMPLETED') {
        updateData.completedAt = new Date();
      }

      const order = await context.prisma.order.update({
        where: { id },
        data: updateData,
        include: {
          customer: true,
          createdBy: true,
          discount: true,
          orderItems: {
            include: {
              product: true,
            },
          },
        },
      });

      return order;
    },

    cancelOrder: async (_: any, { id }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN', 'MANAGER', 'STAFF']);

      const existingOrder = await context.prisma.order.findUnique({
        where: { id },
        include: {
          orderItems: true,
          customer: true,
        },
      });

      if (!existingOrder) {
        throw new GraphQLError('Order not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      if (existingOrder.status === 'CANCELLED') {
        throw new GraphQLError('Order is already cancelled', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      if (existingOrder.status === 'COMPLETED') {
        throw new GraphQLError('Cannot cancel completed order', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      // Cancel order in transaction
      const order = await context.prisma.$transaction(async (tx) => {
        // Update order status
        const cancelledOrder = await tx.order.update({
          where: { id },
          data: {
            status: 'CANCELLED',
          },
          include: {
            customer: true,
            createdBy: true,
            discount: true,
            orderItems: {
              include: {
                product: true,
              },
            },
          },
        });

        // Restore product stock
        for (const item of existingOrder.orderItems) {
          await tx.product.update({
            where: { id: item.productId },
            data: {
              stock: {
                increment: item.quantity,
              },
              popularity: {
                decrement: item.quantity,
              },
            },
          });
        }

        // Restore discount usage count
        if (existingOrder.discountId) {
          await tx.discount.update({
            where: { id: existingOrder.discountId },
            data: {
              usageCount: {
                decrement: 1,
              },
            },
          });
        }

        // Update customer total spent
        if (existingOrder.customerId) {
          await tx.customer.update({
            where: { id: existingOrder.customerId },
            data: {
              totalSpent: {
                decrement: existingOrder.total,
              },
            },
          });
        }

        return cancelledOrder;
      });

      return order;
    },
  },

  Order: {
    customer: async (parent: any, _: any, context: Context) => {
      if (!parent.customerId) return null;
      return context.prisma.customer.findUnique({
        where: { id: parent.customerId },
      });
    },

    createdBy: async (parent: any, _: any, context: Context) => {
      return context.prisma.user.findUnique({
        where: { id: parent.userId },
      });
    },

    discount: async (parent: any, _: any, context: Context) => {
      if (!parent.discountId) return null;
      return context.prisma.discount.findUnique({
        where: { id: parent.discountId },
      });
    },

    orderItems: async (parent: any, _: any, context: Context) => {
      return context.prisma.orderItem.findMany({
        where: { orderId: parent.id },
        include: {
          product: true,
        },
      });
    },
  },

  OrderItem: {
    product: async (parent: any, _: any, context: Context) => {
      return context.prisma.product.findUnique({
        where: { id: parent.productId },
      });
    },
  },
};
