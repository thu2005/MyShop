using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Tests.Mocks
{
    public class MockReportRepository : IReportRepository
    {
        // Mock Data to be returned
        public PeriodReport? MockPeriodReport { get; set; }
        public List<ProductSalesData> MockTopProducts { get; set; } = new();
        public List<CustomerSalesData> MockTopCustomers { get; set; } = new();
        public List<RevenueProfit> MockTimeline { get; set; } = new();
        public List<StaffPerformanceData> MockStaffPerformance { get; set; } = new();
        public CommissionStats? MockCommissionStats { get; set; }

        public MockReportRepository()
        {
            // Initialize with some default dummy data
            MockPeriodReport = new PeriodReport
            {
                TotalOrders = 100,
                TotalRevenue = 50000,
                TotalProfit = 15000,
                PeriodStart = DateTime.Now.AddDays(-7),
                PeriodEnd = DateTime.Now
            };

            MockTopProducts.Add(new ProductSalesData { ProductId = 1, ProductName = "Test Product", QuantitySold = 10, Revenue = 1000 });
            MockTopCustomers.Add(new CustomerSalesData { CustomerId = 1, CustomerName = "Test Customer", TotalOrders = 5, TotalSpent = 500 });
            MockStaffPerformance.Add(new StaffPerformanceData { StaffId = 1, Username = "Staff1", TotalOrders = 10, TotalRevenue = 1000 });

            MockCommissionStats = new CommissionStats
            {
                TotalCommission = 0,
                PaidCommission = 0,
                UnpaidCommission = 0,
                TotalOrderAmount = 0,
                TotalOrders = 0
            };
        }

        public Task<PeriodReport?> GetReportByPeriodAsync(PeriodType period, DateTime? startDate = null, DateTime? endDate = null)
        {
            return Task.FromResult(MockPeriodReport);
        }

        public Task<List<ProductSalesData>> GetTopProductsByQuantityAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(MockTopProducts);
        }

        public Task<List<CustomerSalesData>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int limit = 10)
        {
            return Task.FromResult(MockTopCustomers);
        }

        public Task<List<RevenueProfit>> GetRevenueAndProfitTimelineAsync(DateTime startDate, DateTime endDate, TimelineGrouping groupBy = TimelineGrouping.DAY)
        {
            return Task.FromResult(MockTimeline);
        }

        public Task<List<StaffPerformanceData>> GetAllStaffPerformanceAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(MockStaffPerformance);
        }

        public Task<CommissionStats?> GetCommissionStatsAsync(int? userId, DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(MockCommissionStats);
        }
    }
}