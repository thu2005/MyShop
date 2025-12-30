import { Prisma } from '@prisma/client';
import { Context } from '../../types/context';
import { requireAuth } from '../../middleware/auth.middleware';

export const reportResolvers = {
  Query: {
    // 1. Period Report with Comparison
    reportByPeriod: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);

      const { period, startDate, endDate } = input;
      let currentStart: Date, currentEnd: Date, previousStart: Date, previousEnd: Date;
      const now = new Date();

      // Calculate date ranges based on period
      switch (period) {
        case 'WEEKLY': {
          // Current week (Monday to Sunday)
          const dayOfWeek = now.getDay();
          const diffToMonday = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;
          currentStart = new Date(now);
          currentStart.setDate(now.getDate() + diffToMonday);
          currentStart.setHours(0, 0, 0, 0);
          
          currentEnd = new Date(currentStart);
          currentEnd.setDate(currentStart.getDate() + 6);
          currentEnd.setHours(23, 59, 59, 999);

          // Previous week
          previousStart = new Date(currentStart);
          previousStart.setDate(currentStart.getDate() - 7);
          previousEnd = new Date(currentEnd);
          previousEnd.setDate(currentEnd.getDate() - 7);
          break;
        }
        case 'MONTHLY': {
          // Current month
          currentStart = new Date(now.getFullYear(), now.getMonth(), 1);
          currentEnd = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59, 999);

          // Previous month
          previousStart = new Date(now.getFullYear(), now.getMonth() - 1, 1);
          previousEnd = new Date(now.getFullYear(), now.getMonth(), 0, 23, 59, 59, 999);
          break;
        }
        case 'YEARLY': {
          // Current year
          currentStart = new Date(now.getFullYear(), 0, 1);
          currentEnd = new Date(now.getFullYear(), 11, 31, 23, 59, 59, 999);

          // Previous year
          previousStart = new Date(now.getFullYear() - 1, 0, 1);
          previousEnd = new Date(now.getFullYear() - 1, 11, 31, 23, 59, 59, 999);
          break;
        }
        case 'CUSTOM': {
          currentStart = new Date(startDate);
          currentEnd = new Date(endDate);
          // No previous period for custom
          previousStart = new Date(0);
          previousEnd = new Date(0);
          break;
        }
        default:
          throw new Error('Invalid period type');
      }

      // Helper function to get stats for a period
      const getStats = async (start: Date, end: Date) => {
        const orders = await context.prisma.order.findMany({
          where: {
            createdAt: { gte: start, lte: end },
            status: 'COMPLETED',
          },
          include: {
            orderItems: {
              include: { product: true },
            },
          },
        });

        let totalProductsSold = 0;
        let totalRevenue = new Prisma.Decimal(0);
        let totalCost = new Prisma.Decimal(0);

        for (const order of orders) {
          totalRevenue = totalRevenue.add(order.total);
          for (const item of order.orderItems) {
            totalProductsSold += item.quantity;
            const cost = item.product.costPrice || new Prisma.Decimal(0);
            totalCost = totalCost.add(cost.mul(item.quantity));
          }
        }

        const totalProfit = totalRevenue.sub(totalCost);

        return {
          totalProductsSold,
          totalOrders: orders.length,
          totalRevenue,
          totalProfit,
        };
      };

      // Get current and previous period stats
      const currentStats = await getStats(currentStart, currentEnd);
      
      let previousStats = null;
      let productsChange = null;
      let ordersChange = null;
      let revenueChange = null;
      let profitChange = null;

      if (period !== 'CUSTOM') {
        previousStats = await getStats(previousStart, previousEnd);

        // Calculate percentage changes
        productsChange = previousStats.totalProductsSold > 0
          ? ((currentStats.totalProductsSold - previousStats.totalProductsSold) / previousStats.totalProductsSold) * 100
          : 0;

        ordersChange = previousStats.totalOrders > 0
          ? ((currentStats.totalOrders - previousStats.totalOrders) / previousStats.totalOrders) * 100
          : 0;

        revenueChange = previousStats.totalRevenue.toNumber() > 0
          ? ((currentStats.totalRevenue.toNumber() - previousStats.totalRevenue.toNumber()) / previousStats.totalRevenue.toNumber()) * 100
          : 0;

        profitChange = previousStats.totalProfit.toNumber() > 0
          ? ((currentStats.totalProfit.toNumber() - previousStats.totalProfit.toNumber()) / previousStats.totalProfit.toNumber()) * 100
          : 0;
      }

      return {
        totalProductsSold: currentStats.totalProductsSold,
        totalOrders: currentStats.totalOrders,
        totalRevenue: currentStats.totalRevenue,
        totalProfit: currentStats.totalProfit,
        previousTotalProductsSold: previousStats?.totalProductsSold,
        previousTotalOrders: previousStats?.totalOrders,
        previousTotalRevenue: previousStats?.totalRevenue,
        previousTotalProfit: previousStats?.totalProfit,
        productsChange,
        ordersChange,
        revenueChange,
        profitChange,
        periodStart: currentStart,
        periodEnd: currentEnd,
      };
    },

    // 2. All Products by Quantity (for chart - shows all products sold)
    topProductsByQuantity: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);

      const { startDate, endDate } = input;

      const orders = await context.prisma.order.findMany({
        where: {
          createdAt: { gte: new Date(startDate), lte: new Date(endDate) },
          status: 'COMPLETED',
        },
        include: {
          orderItems: {
            include: { product: true },
          },
        },
      });

      const productSales = new Map<number, { name: string; quantity: number; revenue: Prisma.Decimal }>();

      for (const order of orders) {
        for (const item of order.orderItems) {
          const existing = productSales.get(item.productId);
          if (existing) {
            existing.quantity += item.quantity;
            existing.revenue = existing.revenue.add(item.total);
          } else {
            productSales.set(item.productId, {
              name: item.product.name,
              quantity: item.quantity,
              revenue: new Prisma.Decimal(item.total),
            });
          }
        }
      }

      // Return ALL products sold, sorted by quantity
      return Array.from(productSales.entries())
        .map(([id, data]) => ({
          productId: id,
          productName: data.name,
          quantitySold: data.quantity,
          revenue: data.revenue,
        }))
        .sort((a, b) => b.quantitySold - a.quantitySold);
    },

    // 3. Top Customers
    topCustomers: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);

      const { startDate, endDate, limit = 10 } = input;

      const orders = await context.prisma.order.findMany({
        where: {
          createdAt: { gte: new Date(startDate), lte: new Date(endDate) },
          status: 'COMPLETED',
          customerId: { not: null },
        },
        include: {
          customer: true,
        },
      });

      const customerSales = new Map<number, { name: string; orders: number; spent: Prisma.Decimal }>();

      for (const order of orders) {
        const customerId = order.customerId!; // Safe to use ! since we filtered out nulls
        const customerName = order.customer!.name;

        const existing = customerSales.get(customerId);
        if (existing) {
          existing.orders += 1;
          existing.spent = existing.spent.add(order.total);
        } else {
          customerSales.set(customerId, {
            name: customerName,
            orders: 1,
            spent: new Prisma.Decimal(order.total),
          });
        }
      }

      return Array.from(customerSales.entries())
        .sort((a, b) => b[1].spent.comparedTo(a[1].spent))
        .slice(0, limit)
        .map(([customerId, data]) => ({
          customerId: customerId,
          customerName: data.name,
          totalOrders: data.orders,
          totalSpent: data.spent,
        }));
    },

    // 4. Revenue and Profit Timeline
    revenueAndProfitTimeline: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);

      const { startDate, endDate, groupBy = 'DAY' } = input;

      const orders = await context.prisma.order.findMany({
        where: {
          createdAt: { gte: new Date(startDate), lte: new Date(endDate) },
          status: 'COMPLETED',
        },
        include: {
          orderItems: {
            include: { product: true },
          },
        },
      });

      const timeline = new Map<string, { revenue: Prisma.Decimal; cost: Prisma.Decimal; orders: number }>();

      for (const order of orders) {
        let dateKey: string;
        const orderDate = new Date(order.createdAt);

        switch (groupBy) {
          case 'DAY':
            dateKey = orderDate.toISOString().split('T')[0];
            break;
          case 'WEEK': {
            const weekStart = new Date(orderDate);
            const dayOfWeek = weekStart.getDay();
            const diffToMonday = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;
            weekStart.setDate(weekStart.getDate() + diffToMonday);
            dateKey = `Week of ${weekStart.toISOString().split('T')[0]}`;
            break;
          }
          case 'MONTH':
            dateKey = `${orderDate.getFullYear()}-${String(orderDate.getMonth() + 1).padStart(2, '0')}`;
            break;
          default:
            dateKey = orderDate.toISOString().split('T')[0];
        }

        let orderCost = new Prisma.Decimal(0);
        for (const item of order.orderItems) {
          const cost = item.product.costPrice || new Prisma.Decimal(0);
          orderCost = orderCost.add(cost.mul(item.quantity));
        }

        const existing = timeline.get(dateKey);
        if (existing) {
          existing.revenue = existing.revenue.add(order.total);
          existing.cost = existing.cost.add(orderCost);
          existing.orders += 1;
        } else {
          timeline.set(dateKey, {
            revenue: new Prisma.Decimal(order.total),
            cost: orderCost,
            orders: 1,
          });
        }
      }

      return Array.from(timeline.entries())
        .sort((a, b) => a[0].localeCompare(b[0]))
        .map(([date, data]) => ({
          date,
          revenue: data.revenue,
          profit: data.revenue.sub(data.cost),
          orders: data.orders,
        }));
    },

    // 5. All Staff Performance
    allStaffPerformance: async (_: any, { input }: any, context: Context) => {
      requireAuth(context);

      const { startDate, endDate } = input;

      console.log('[Staff Performance] Query params:', { startDate, endDate });

      // Get all completed orders in date range with staff info
      const orders = await context.prisma.order.findMany({
        where: {
          createdAt: { gte: new Date(startDate), lte: new Date(endDate) }, // Changed from completedAt to createdAt
          status: 'COMPLETED',
        },
        include: {
          createdBy: true, // User who created the order (staff)
          orderItems: {
            include: { product: true },
          },
        },
      });

      console.log('[Staff Performance] Found orders:', orders.length);

      // Group by staff (only STAFF role)
      const staffSales = new Map<number, { username: string; email: string | null; orders: number; revenue: Prisma.Decimal; cost: Prisma.Decimal }>();

      for (const order of orders) {
        const staffId = order.userId;
        const staff = order.createdBy;

        if (!staff) {
          console.log('[Staff Performance] Skipping order - no staff info');
          continue;
        }

        // Only include STAFF role users
        if (staff.role !== 'STAFF') {
          console.log(`[Staff Performance] Skipping user ${staff.username} - role: ${staff.role}`);
          continue;
        }

        let orderCost = new Prisma.Decimal(0);
        for (const item of order.orderItems) {
          const cost = item.product.costPrice || new Prisma.Decimal(0);
          orderCost = orderCost.add(cost.mul(item.quantity));
        }

        const existing = staffSales.get(staffId);
        if (existing) {
          existing.orders += 1;
          existing.revenue = existing.revenue.add(order.total);
          existing.cost = existing.cost.add(orderCost);
        } else {
          staffSales.set(staffId, {
            username: staff.username,
            email: staff.email,
            orders: 1,
            revenue: new Prisma.Decimal(order.total),
            cost: orderCost,
          });
        }
      }

      console.log('[Staff Performance] Staff count:', staffSales.size);

      // Get PAID commissions for all staff in the date range
      const commissions = await context.prisma.commission.findMany({
        where: {
          createdAt: { gte: new Date(startDate), lte: new Date(endDate) },
          isPaid: true,
        },
      });

      // Map commissions by userId
      const commissionByUser = new Map<number, Prisma.Decimal>();
      for (const commission of commissions) {
        const existing = commissionByUser.get(commission.userId);
        if (existing) {
          commissionByUser.set(commission.userId, existing.add(commission.commissionAmount));
        } else {
          commissionByUser.set(commission.userId, new Prisma.Decimal(commission.commissionAmount));
        }
      }

      // Return ALL staff sorted by revenue
      const result = Array.from(staffSales.entries())
        .sort((a, b) => b[1].revenue.comparedTo(a[1].revenue))
        .map(([staffId, data]) => ({
          staffId,
          username: data.username,
          email: data.email,
          totalOrders: data.orders,
          totalRevenue: data.revenue,
          totalProfit: data.revenue.sub(data.cost),
          totalCommission: commissionByUser.get(staffId) || new Prisma.Decimal(0),
        }));

      console.log('[Staff Performance] Returning staff:', result.length);
      return result;
    },
  },
};
