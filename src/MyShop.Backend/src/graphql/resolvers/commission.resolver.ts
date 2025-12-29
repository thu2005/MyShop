import { Context } from '../../types/context';
import { Prisma } from '@prisma/client';

export interface CommissionFilterInput {
    userId?: number;
    isPaid?: boolean;
    dateRange?: {
        from: Date;
        to: Date;
    };
}

export interface PaginationInput {
    page?: number;
    pageSize?: number;
}

export const commissionResolvers = {
    Query: {
        // Get paginated commissions with filters
        getCommissions: async (
            _: any,
            {
                filter,
                pagination,
            }: { filter?: CommissionFilterInput; pagination?: PaginationInput },
            { prisma, user }: Context
        ) => {
            if (!user) throw new Error('Not authenticated');

            const page = pagination?.page || 1;
            const pageSize = pagination?.pageSize || 20;
            const skip = (page - 1) * pageSize;

            // Build where clause
            const where: Prisma.CommissionWhereInput = {};

            if (filter?.userId) {
                where.userId = filter.userId;
            }

            if (filter?.isPaid !== undefined) {
                where.isPaid = filter.isPaid;
            }

            if (filter?.dateRange) {
                where.createdAt = {
                    gte: filter.dateRange.from,
                    lte: filter.dateRange.to,
                };
            }

            // Execute queries
            const [items, total] = await Promise.all([
                prisma.commission.findMany({
                    where,
                    skip,
                    take: pageSize,
                    orderBy: { createdAt: 'desc' },
                }),
                prisma.commission.count({ where }),
            ]);

            return {
                items,
                total,
                page,
                pageSize,
                totalPages: Math.ceil(total / pageSize),
            };
        },

        // Get commissions for specific user
        getUserCommissions: async (
            _: any,
            { userId }: { userId: number },
            { prisma, user }: Context
        ) => {
            if (!user) throw new Error('Not authenticated');

            return prisma.commission.findMany({
                where: { userId },
                orderBy: { createdAt: 'desc' },
            });
        },

        // Get commission statistics
        getCommissionStats: async (
            _: any,
            {
                userId,
                dateRange,
            }: { userId?: number; dateRange?: { from: Date; to: Date } },
            { prisma, user }: Context
        ) => {
            if (!user) throw new Error('Not authenticated');

            const where: Prisma.CommissionWhereInput = {};

            if (userId) {
                where.userId = userId;
            }

            if (dateRange) {
                where.createdAt = {
                    gte: dateRange.from,
                    lte: dateRange.to,
                };
            }

            const commissions = await prisma.commission.findMany({ where });

            const totalCommission = commissions.reduce(
                (sum, c) => sum + Number(c.commissionAmount),
                0
            );
            const paidCommission = commissions
                .filter((c) => c.isPaid)
                .reduce((sum, c) => sum + Number(c.commissionAmount), 0);
            const unpaidCommission = commissions
                .filter((c) => !c.isPaid)
                .reduce((sum, c) => sum + Number(c.commissionAmount), 0);

            return {
                totalCommission,
                paidCommission,
                unpaidCommission,
                totalOrders: commissions.length,
            };
        },
    },

    Mutation: {
        // Calculate commission for an order
        calculateCommission: async (
            _: any,
            { orderId }: { orderId: number },
            { prisma, user }: Context
        ) => {
            if (!user) throw new Error('Not authenticated');

            // Get order details
            const order = await prisma.order.findUnique({
                where: { id: orderId },
                include: { orderItems: true },
            });

            if (!order) throw new Error('Order not found');

            // Check if commission already exists
            const existing = await prisma.commission.findUnique({
                where: { orderId },
            });

            if (existing) {
                throw new Error('Commission already calculated for this order');
            }

            // Calculate order total
            const orderTotal = order.orderItems.reduce(
                (sum, item) => sum + Number(item.unitPrice) * item.quantity,
                0
            );

            // Commission rate: 5% for orders >= $1000, 3% otherwise
            const commissionRate = orderTotal >= 1000 ? 0.05 : 0.03;
            const commissionAmount = orderTotal * commissionRate;

            // Create commission record
            const commission = await prisma.commission.create({
                data: {
                    userId: order.userId,
                    orderId: order.id,
                    orderTotal,
                    commissionRate,
                    commissionAmount,
                    isPaid: false,
                },
            });

            return commission;
        },

        // Mark commission as paid
        markCommissionPaid: async (
            _: any,
            { id }: { id: number },
            { prisma, user }: Context
        ) => {
            if (!user) throw new Error('Not authenticated');

            const commission = await prisma.commission.update({
                where: { id },
                data: { isPaid: true },
            });

            return commission;
        },
    },

    Commission: {
        user: async (parent: any, _: any, { prisma }: Context) => {
            return prisma.user.findUnique({
                where: { id: parent.userId },
            });
        },
        order: async (parent: any, _: any, { prisma }: Context) => {
            return prisma.order.findUnique({
                where: { id: parent.orderId },
            });
        },
    },
};
