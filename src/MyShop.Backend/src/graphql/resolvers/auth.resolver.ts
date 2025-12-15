import { GraphQLError } from 'graphql';
import { Context } from '../../types/context';
import { AuthUtils } from '../../utils/auth';

export const authResolvers = {
  Query: {
    me: async (_: any, __: any, context: Context) => {
      if (!context.user) {
        throw new GraphQLError('Not authenticated', {
          extensions: { code: 'UNAUTHENTICATED' },
        });
      }

      const user = await context.prisma.user.findUnique({
        where: { id: context.user.id },
      });

      if (!user) {
        throw new GraphQLError('User not found', {
          extensions: { code: 'NOT_FOUND' },
        });
      }

      return user;
    },
  },

  Mutation: {
    login: async (_: any, { input }: any, context: Context) => {
      const { username, password } = input;

      // Find user
      const user = await context.prisma.user.findUnique({
        where: { username },
      });

      if (!user) {
        throw new GraphQLError('Invalid credentials', {
          extensions: { code: 'UNAUTHORIZED' },
        });
      }

      // Check if user is active
      if (!user.isActive) {
        throw new GraphQLError('Account is inactive', {
          extensions: { code: 'FORBIDDEN' },
        });
      }

      // Verify password
      const isValid = await AuthUtils.comparePassword(password, user.password);
      if (!isValid) {
        throw new GraphQLError('Invalid credentials', {
          extensions: { code: 'UNAUTHORIZED' },
        });
      }

      // Generate tokens
      const userPayload = {
        id: user.id,
        username: user.username,
        email: user.email,
        role: user.role,
      };

      const token = AuthUtils.generateToken(userPayload);
      const refreshToken = AuthUtils.generateRefreshToken(userPayload);

      return {
        token,
        refreshToken,
        user,
      };
    },

    register: async (_: any, { input }: any, context: Context) => {
      const { username, email, password, role } = input;

      // Check if username exists
      const existingUser = await context.prisma.user.findUnique({
        where: { username },
      });

      if (existingUser) {
        throw new GraphQLError('Username already exists', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      // Check if email exists
      const existingEmail = await context.prisma.user.findUnique({
        where: { email },
      });

      if (existingEmail) {
        throw new GraphQLError('Email already exists', {
          extensions: { code: 'BAD_USER_INPUT' },
        });
      }

      // Hash password
      const hashedPassword = await AuthUtils.hashPassword(password);

      // Create user
      const user = await context.prisma.user.create({
        data: {
          username,
          email,
          password: hashedPassword,
          role: role || 'STAFF',
        },
      });

      // Generate tokens
      const userPayload = {
        id: user.id,
        username: user.username,
        email: user.email,
        role: user.role,
      };

      const token = AuthUtils.generateToken(userPayload);
      const refreshToken = AuthUtils.generateRefreshToken(userPayload);

      return {
        token,
        refreshToken,
        user,
      };
    },

    refreshToken: async (_: any, { refreshToken }: any, context: Context) => {
      try {
        const decoded = AuthUtils.verifyToken(refreshToken);

        const user = await context.prisma.user.findUnique({
          where: { id: decoded.id },
        });

        if (!user || !user.isActive) {
          throw new GraphQLError('Invalid refresh token', {
            extensions: { code: 'UNAUTHORIZED' },
          });
        }

        const userPayload = {
          id: user.id,
          username: user.username,
          email: user.email,
          role: user.role,
        };

        const newToken = AuthUtils.generateToken(userPayload);
        const newRefreshToken = AuthUtils.generateRefreshToken(userPayload);

        return {
          token: newToken,
          refreshToken: newRefreshToken,
          user,
        };
      } catch (error) {
        throw new GraphQLError('Invalid or expired refresh token', {
          extensions: { code: 'UNAUTHORIZED' },
        });
      }
    },
  },
};
