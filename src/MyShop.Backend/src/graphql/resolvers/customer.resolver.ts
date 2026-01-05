import { GraphQLError } from 'graphql';
import { Context } from '../../types/context';
import { requireAuth, requireRole } from '../../middleware/auth.middleware';

export const customerResolvers = {
  Query: {
    customers: async (_: any, { pagination, sort, filter }: any, context: Context) => {
      requireAuth(context);

      const page = pagination?.page || 1;
      const pageSize = Math.min(pagination?.pageSize || 20, 100);
      const skip = (page - 1) * pageSize;

      const where: any = {};

      if (filter) {
        if (filter.name) {
          where.name = { contains: filter.name, mode: 'insensitive' };
        }
        if (filter.email) {
          where.email = { contains: filter.email, mode: 'insensitive' };
        }
        if (filter.phone) {
          where.phone = { contains: filter.phone };
        }
        if (filter.isMember !== undefined) {
          where.isMember = filter.isMember;
        }
      }

      const orderBy: any = sort
        ? { [sort.field]: sort.order.toLowerCase() }
        : { createdAt: 'desc' };

      const [customers, total] = await Promise.all([
        context.prisma.customer.findMany({
          where,
          skip,
          take: pageSize,
          orderBy,
          include: {
            _count: {
              select: { orders: true },
            },
          },
        }),
        context.prisma.customer.count({ where }),
      ]);

      return {
        customers,
        total,
        page,
        pageSize,
        totalPages: Math.ceil(total / pageSize),
      };
    },

    customer: async (_: any, { id }: any, context: Context) => {
      requireAuth(context);

      const customer = await context.prisma.customer.findUnique({
        where: { id },
        include: {
          orders: {
            orderBy: { createdAt: 'desc' },
            take: 10,
          },
          _count: {
            select: { orders: true },
          },
        },
      });

      if (!customer) {
        throw new GraphQLError('Customer not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      return customer;
    },
  },

  Mutation: {
    createCustomer: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);

      // Check if email exists (if provided)
      if (input.email) {
        const existingEmail = await context.prisma.customer.findUnique({
          where: { email: input.email },
        });

        if (existingEmail) {
          throw new GraphQLError('Email already exists', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }
      }

      const customerData: any = {
        ...input,
      };

      // Handle properties that should be null if empty/falsy
      if (!customerData.email) delete customerData.email;
      if (!customerData.address) delete customerData.address;
      if (!customerData.notes) delete customerData.notes;

      if (input.isMember) {
        customerData.memberSince = new Date();
      }

      const customer = await context.prisma.customer.create({
        data: customerData,
        include: {
          _count: {
            select: { orders: true },
          },
        },
      });

      return customer;
    },

    updateCustomer: async (_: any, { id, input }: any, context: Context) => {
      requireAuth(context);

      const existingCustomer = await context.prisma.customer.findUnique({
        where: { id },
      });

      if (!existingCustomer) {
        throw new GraphQLError('Customer not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      // Check if new email conflicts
      if (input.email && input.email !== existingCustomer.email) {
        const emailConflict = await context.prisma.customer.findUnique({
          where: { email: input.email },
        });

        if (emailConflict) {
          throw new GraphQLError('Email already exists', {
            extensions: { code: 'BAD_USER_INPUT' },
          });
        }
      }

      const updateData: any = { ...input };

      // Handle properties that should be null if empty/falsy
      if (updateData.email === '') updateData.email = null;
      if (updateData.address === '') updateData.address = null;
      if (updateData.notes === '') updateData.notes = null;

      // Set memberSince if becoming a member
      if (input.isMember && !existingCustomer.isMember) {
        updateData.memberSince = new Date();
      }

      const customer = await context.prisma.customer.update({
        where: { id },
        data: updateData,
        include: {
          _count: {
            select: { orders: true },
          },
        },
      });

      return customer;
    },

    deleteCustomer: async (_: any, { id }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN', 'MANAGER']);

      const existingCustomer = await context.prisma.customer.findUnique({
        where: { id },
        include: {
          _count: {
            select: { orders: true },
          },
        },
      });

      if (!existingCustomer) {
        throw new GraphQLError('Customer not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      if (existingCustomer._count.orders > 0) {
        throw new GraphQLError('Cannot delete customer with orders', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      await context.prisma.customer.delete({
        where: { id },
      });

      return true;
    },
  },

  Customer: {
    orderCount: async (parent: any, _: any, context: Context) => {
      const count = await context.prisma.order.count({
        where: { customerId: parent.id },
      });
      return count;
    },

    orders: async (parent: any, _: any, context: Context) => {
      return context.prisma.order.findMany({
        where: { customerId: parent.id },
        orderBy: { createdAt: 'desc' },
      });
    },
  },
};
