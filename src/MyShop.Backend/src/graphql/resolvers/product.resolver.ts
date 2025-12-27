import { GraphQLError } from 'graphql';
import { Context } from '../../types/context';
import { requireAuth, requireRole } from '../../middleware/auth.middleware';

export const productResolvers = {
  Query: {
    products: async (_: any, { pagination, sort, filter }: any, context: Context) => {
      requireAuth(context);

      const page = pagination?.page || 1;
      const pageSize = Math.min(pagination?.pageSize || 20, 1000);
      const skip = (page - 1) * pageSize;

      const where: any = {};

      if (filter) {
        if (filter.name) {
          where.name = { contains: filter.name, mode: 'insensitive' };
        }
        if (filter.categoryId) {
          where.categoryId = filter.categoryId;
        }
        if (filter.minPrice !== undefined || filter.maxPrice !== undefined) {
          where.price = {};
          if (filter.minPrice !== undefined) where.price.gte = filter.minPrice;
          if (filter.maxPrice !== undefined) where.price.lte = filter.maxPrice;
        }
        if (filter.inStock !== undefined) {
          where.stock = filter.inStock ? { gt: 0 } : { lte: 0 };
        }
        if (filter.isActive !== undefined) {
          where.isActive = filter.isActive;
        }
      }

      const orderBy: any = sort
        ? { [sort.field]: sort.order.toLowerCase() }
        : { createdAt: 'desc' };

      const [products, total] = await Promise.all([
        context.prisma.product.findMany({
          where,
          skip,
          take: pageSize,
          orderBy,
          include: {
            category: true,
          },
        }),
        context.prisma.product.count({ where }),
      ]);

      return {
        products,
        total,
        page,
        pageSize,
        totalPages: Math.ceil(total / pageSize),
      };
    },

    product: async (_: any, { id }: any, context: Context) => {
      requireAuth(context);

      const product = await context.prisma.product.findUnique({
        where: { id },
        include: {
          category: true,
        },
      });

      if (!product) {
        throw new GraphQLError('Product not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      return product;
    },

    productBySku: async (_: any, { sku }: any, context: Context) => {
      requireAuth(context);

      const product = await context.prisma.product.findUnique({
        where: { sku },
        include: {
          category: true,
        },
      });

      if (!product) {
        throw new GraphQLError('Product not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      return product;
    },

    lowStockProducts: async (_: any, { threshold = 10 }: any, context: Context) => {
      requireAuth(context);

      const products = await context.prisma.product.findMany({
        where: {
          stock: { lte: threshold },
          isActive: true,
        },
        include: {
          category: true,
        },
        orderBy: {
          stock: 'asc',
        },
      });

      return products;
    },
  },

  Mutation: {
    createProduct: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN', 'MANAGER']);

      // Check if SKU exists
      const existingSku = await context.prisma.product.findUnique({
        where: { sku: input.sku },
      });

      if (existingSku) {
        throw new GraphQLError('SKU already exists', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      // Check if barcode exists (if provided)
      if (input.barcode) {
        const existingBarcode = await context.prisma.product.findUnique({
          where: { barcode: input.barcode },
        });

        if (existingBarcode) {
          throw new GraphQLError('Barcode already exists', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }
      }

      // Check if category exists
      const category = await context.prisma.category.findUnique({
        where: { id: input.categoryId },
      });

      if (!category) {
        throw new GraphQLError('Category not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      const product = await context.prisma.product.create({
        data: input,
        include: {
          category: true,
        },
      });

      return product;
    },

    updateProduct: async (_: any, { id, input }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN', 'MANAGER']);

      const existingProduct = await context.prisma.product.findUnique({
        where: { id },
      });

      if (!existingProduct) {
        throw new GraphQLError('Product not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      // Check if new SKU conflicts
      if (input.sku && input.sku !== existingProduct.sku) {
        const existingSku = await context.prisma.product.findUnique({
          where: { sku: input.sku },
        });

        if (existingSku) {
          throw new GraphQLError('SKU already exists', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }
      }

      // Check if new barcode conflicts
      if (input.barcode && input.barcode !== existingProduct.barcode) {
        const existingBarcode = await context.prisma.product.findUnique({
          where: { barcode: input.barcode },
        });

        if (existingBarcode) {
          throw new GraphQLError('Barcode already exists', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }
      }

      const product = await context.prisma.product.update({
        where: { id },
        data: input,
        include: {
          category: true,
        },
      });

      return product;
    },

    deleteProduct: async (_: any, { id }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN']);

      const existingProduct = await context.prisma.product.findUnique({
        where: { id },
      });

      if (!existingProduct) {
        throw new GraphQLError('Product not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      // Check if product is used in orders
      const ordersCount = await context.prisma.orderItem.count({
        where: { productId: id },
      });

      if (ordersCount > 0) {
        throw new GraphQLError('Cannot delete product that has been ordered', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      await context.prisma.product.delete({
        where: { id },
      });

      return true;
    },

    updateProductStock: async (_: any, { id, quantity }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN', 'MANAGER', 'STAFF']);

      const product = await context.prisma.product.findUnique({
        where: { id },
      });

      if (!product) {
        throw new GraphQLError('Product not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      const newStock = product.stock + quantity;

      if (newStock < 0) {
        throw new GraphQLError('Insufficient stock', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      const updatedProduct = await context.prisma.product.update({
        where: { id },
        data: { stock: newStock },
        include: {
          category: true,
        },
      });

      return updatedProduct;
    },
  },

  Product: {
    category: async (parent: any, _: any, context: Context) => {
      return context.prisma.category.findUnique({
        where: { id: parent.categoryId },
      });
    },
  },
};
