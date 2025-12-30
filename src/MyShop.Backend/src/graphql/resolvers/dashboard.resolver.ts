import { Prisma } from '@prisma/client';
import { Context } from '../../types/context';
import { requireAuth } from '../../middleware/auth.middleware';

export const dashboardResolvers = {
  Query: {
    dashboardStats: async (_: any, __: any, context: Context) => {
      requireAuth(context);

      const today = new Date();
      today.setHours(0, 0, 0, 0);

      const tomorrow = new Date(today);
      tomorrow.setDate(tomorrow.getDate() + 1);

      const [
        totalProducts,
        totalOrders,
        totalRevenueResult,
        totalCustomers,
        lowStockProducts,
        pendingOrders,
        todayRevenueResult,
        todayOrders,
      ] = await Promise.all([
        context.prisma.product.count({ where: { isActive: true } }),
        context.prisma.order.count(),
        context.prisma.order.aggregate({
          _sum: { total: true },
          where: { status: { in: ['COMPLETED', 'PROCESSING'] } },
        }),
        context.prisma.customer.count(),
        context.prisma.product.count({
          where: {
            stock: { lte: 10 },
            isActive: true,
          },
        }),
        context.prisma.order.count({ where: { status: 'PENDING' } }),
        context.prisma.order.aggregate({
          _sum: { total: true },
          where: {
            createdAt: { gte: today, lt: tomorrow },
            status: { in: ['COMPLETED', 'PROCESSING'] },
          },
        }),
        context.prisma.order.count({
          where: {
            createdAt: { gte: today, lt: tomorrow },
          },
        }),
      ]);

      return {
        totalProducts,
        totalOrders,
        totalRevenue: totalRevenueResult._sum.total || new Prisma.Decimal(0),
        totalCustomers,
        lowStockProducts,
        pendingOrders,
        todayRevenue: todayRevenueResult._sum.total || new Prisma.Decimal(0),
        todayOrders,
      };
    },

    salesReport: async (_: any, { dateRange }: any, context: Context) => {
      requireAuth(context);

      const fromDate = dateRange.from ? new Date(dateRange.from) : new Date(0);
      const toDate = dateRange.to ? new Date(dateRange.to) : new Date();

      // Get orders in date range
      const orders = await context.prisma.order.findMany({
        where: {
          createdAt: {
            gte: fromDate,
            lte: toDate,
          },
          status: 'COMPLETED',
        },
        include: {
          orderItems: {
            include: {
              product: {
                include: {
                  images: true,
                },
              },
            },
          },
        },
      });

      // Calculate total revenue
      const totalRevenue = orders.reduce(
        (sum, order) => sum.add(order.total),
        new Prisma.Decimal(0)
      );

      const totalOrders = orders.length;
      const averageOrderValue = totalOrders > 0
        ? totalRevenue.div(totalOrders)
        : new Prisma.Decimal(0);

      // Calculate top products
      const productSales = new Map<number, { quantity: number; revenue: Prisma.Decimal; product: any }>();

      for (const order of orders) {
        for (const item of order.orderItems) {
          const existing = productSales.get(item.productId);
          if (existing) {
            existing.quantity += item.quantity;
            existing.revenue = existing.revenue.add(item.total);
          } else {
            productSales.set(item.productId, {
              quantity: item.quantity,
              revenue: new Prisma.Decimal(item.total),
              product: item.product,
            });
          }
        }
      }

      const topProducts = Array.from(productSales.values())
        .sort((a, b) => b.revenue.comparedTo(a.revenue))
        .slice(0, 10)
        .map((item) => ({
          product: item.product,
          quantity: item.quantity,
          revenue: item.revenue,
        }));

      // Calculate revenue by date
      const revenueByDateMap = new Map<string, { revenue: Prisma.Decimal; orders: number }>();
      
      // Determine grouping based on date range
      const daysDiff = Math.ceil((toDate.getTime() - fromDate.getTime()) / (1000 * 60 * 60 * 24));
      const groupByMonth = daysDiff > 31; // If range > 1 month, group by month

      for (const order of orders) {
        let dateKey: string;
        const orderDate = new Date(order.createdAt);
        
        if (groupByMonth) {
          // Group by month (YYYY-MM format)
          dateKey = `${orderDate.getFullYear()}-${String(orderDate.getMonth() + 1).padStart(2, '0')}`;
        } else {
          // Group by day (YYYY-MM-DD format)
          dateKey = orderDate.toISOString().split('T')[0];
        }
        
        const existing = revenueByDateMap.get(dateKey);
        if (existing) {
          existing.revenue = existing.revenue.add(order.total);
          existing.orders += 1;
        } else {
          revenueByDateMap.set(dateKey, {
            revenue: new Prisma.Decimal(order.total),
            orders: 1,
          });
        }
      }

      const revenueByDate = Array.from(revenueByDateMap.entries())
        .sort((a, b) => a[0].localeCompare(b[0]))
        .map(([date, data]) => ({
          date,
          revenue: data.revenue,
          orders: data.orders,
        }));

      return {
        totalRevenue,
        totalOrders,
        averageOrderValue,
        topProducts,
        revenueByDate,
      };
    },
  },
};
