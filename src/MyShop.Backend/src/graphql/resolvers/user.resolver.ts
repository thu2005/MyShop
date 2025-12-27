import { GraphQLError } from 'graphql';
import { Context } from '../../types/context';
import { AuthUtils } from '../../utils/auth';
import { requireAuth, requireRole } from '../../middleware/auth.middleware';

export const userResolvers = {
  Query: {
    users: async (_: any, { pagination, sort }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN', 'MANAGER']);

      const page = pagination?.page || 1;
      const pageSize = Math.min(pagination?.pageSize || 20, 100);
      const skip = (page - 1) * pageSize;

      const orderBy: any = sort
        ? { [sort.field]: sort.order.toLowerCase() }
        : { createdAt: 'desc' };

      const [users, total] = await Promise.all([
        context.prisma.user.findMany({
          skip,
          take: pageSize,
          orderBy,
        }),
        context.prisma.user.count(),
      ]);

      return {
        users,
        total,
        page,
        pageSize,
        totalPages: Math.ceil(total / pageSize),
      };
    },

    user: async (_: any, { id }: any, context: Context) => {
      requireAuth(context);

      const user = await context.prisma.user.findUnique({
        where: { id },
      });

      if (!user) {
        throw new GraphQLError('User not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      return user;
    },

    userByUsername: async (_: any, { username }: any, context: Context) => {
      requireAuth(context);

      const user = await context.prisma.user.findFirst({
        where: {
          username: {
            equals: username,
            mode: 'insensitive'
          }
        },
      });

      return user;
    },
  },

  Mutation: {
    createUser: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN']);

      // Check if username exists (case-insensitive)
      const existingUser = await context.prisma.user.findFirst({
        where: {
          username: {
            equals: input.username,
            mode: 'insensitive'
          }
        },
      });

      if (existingUser) {
        throw new GraphQLError('Username already exists', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      // Check if email exists
      const existingEmail = await context.prisma.user.findUnique({
        where: { email: input.email },
      });

      if (existingEmail) {
        throw new GraphQLError('Email already exists', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      // Hash password
      const hashedPassword = await AuthUtils.hashPassword(input.password);

      const user = await context.prisma.user.create({
        data: {
          ...input,
          password: hashedPassword,
        },
      });

      return user;
    },

    updateUser: async (_: any, { id, input }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN']);

      const existingUser = await context.prisma.user.findUnique({
        where: { id },
      });

      if (!existingUser) {
        throw new GraphQLError('User not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      const updateData: any = { ...input };

      // Hash password if provided
      if (input.password) {
        updateData.password = await AuthUtils.hashPassword(input.password);
      }

      const user = await context.prisma.user.update({
        where: { id },
        data: updateData,
      });

      return user;
    },

    deleteUser: async (_: any, { id }: any, context: Context) => {
      requireAuth(context);
      requireRole(context, ['ADMIN']);

      // Don't allow deleting yourself
      if (context.user?.id === id) {
        throw new GraphQLError('Cannot delete your own account', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      const existingUser = await context.prisma.user.findUnique({
        where: { id },
      });

      if (!existingUser) {
        throw new GraphQLError('User not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      await context.prisma.user.delete({
        where: { id },
      });

      return true;
    },
  },
};
