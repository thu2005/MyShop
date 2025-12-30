using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories
{
    public interface IReportRepository
    {
        Task<PeriodReport?> GetReportByPeriodAsync(PeriodType period, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ProductSalesData>> GetTopProductsByQuantityAsync(DateTime startDate, DateTime endDate);
        Task<List<CustomerSalesData>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int limit = 10);
        Task<List<RevenueProfit>> GetRevenueAndProfitTimelineAsync(DateTime startDate, DateTime endDate, TimelineGrouping groupBy = TimelineGrouping.DAY);
        Task<List<StaffPerformanceData>> GetAllStaffPerformanceAsync(DateTime startDate, DateTime endDate);
        Task<CommissionStats?> GetCommissionStatsAsync(int? userId, DateTime startDate, DateTime endDate);
    }
}
