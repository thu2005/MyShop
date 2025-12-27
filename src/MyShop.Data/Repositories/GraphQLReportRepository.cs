using GraphQL;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Data.Repositories
{
    public class GraphQLReportRepository : GraphQLRepositoryBase<object>, IReportRepository
    {
        public GraphQLReportRepository(GraphQLService graphQLService) 
            : base(graphQLService, "report") // base model doesn't matter much here as we use custom queries
        {
        }

        public async Task<PeriodReport?> GetReportByPeriodAsync(PeriodType period, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ReportRepository] Calling reportByPeriod with period={period}, startDate={startDate}, endDate={endDate}");
                
                var request = new GraphQLRequest
                {
                    Query = @"
                    query ReportByPeriod($input: ReportPeriodInput!) {
                        reportByPeriod(input: $input) {
                            totalProductsSold
                            totalOrders
                            totalRevenue
                            totalProfit
                            previousTotalProductsSold
                            previousTotalOrders
                            previousTotalRevenue
                            previousTotalProfit
                            productsChange
                            ordersChange
                            revenueChange
                            profitChange
                            periodStart
                            periodEnd
                        }
                    }",
                    Variables = new { 
                        input = new { 
                            period = period.ToString(),
                            startDate = startDate?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            endDate = endDate?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        } 
                    }
                };

                var response = await _graphQLService.Client.SendQueryAsync<ReportResponse>(request);
                
                if (response.Errors != null && response.Errors.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[ReportRepository] GraphQL Errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
                }
                
                System.Diagnostics.Debug.WriteLine($"[ReportRepository] Response received: {response.Data?.ReportByPeriod?.TotalOrders ?? 0} orders");
                
                return response.Data?.ReportByPeriod;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReportRepository] Exception: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<ProductSalesData>> GetTopProductsByQuantityAsync(DateTime startDate, DateTime endDate)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query TopProducts($input: TopProductsInput!) {
                        topProductsByQuantity(input: $input) {
                            productId
                            productName
                            quantitySold
                            revenue
                        }
                    }",
                Variables = new { 
                    input = new { 
                        startDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        endDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    } 
                }
            };

            var response = await _graphQLService.Client.SendQueryAsync<TopProductsResponse>(request);
            return response.Data?.TopProductsByQuantity ?? new List<ProductSalesData>();
        }

        public async Task<List<CustomerSalesData>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int limit = 10)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query TopCustomers($input: TopCustomersInput!) {
                        topCustomers(input: $input) {
                            customerId
                            customerName
                            totalOrders
                            totalSpent
                        }
                    }",
                Variables = new { 
                    input = new { 
                        startDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        endDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        limit
                    } 
                }
            };

            var response = await _graphQLService.Client.SendQueryAsync<TopCustomersResponse>(request);
            return response.Data?.TopCustomers ?? new List<CustomerSalesData>();
        }

        public async Task<List<RevenueProfit>> GetRevenueAndProfitTimelineAsync(DateTime startDate, DateTime endDate, TimelineGrouping groupBy = TimelineGrouping.DAY)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query Timeline($input: TimelineInput!) {
                        revenueAndProfitTimeline(input: $input) {
                            date
                            revenue
                            profit
                            orders
                        }
                    }",
                Variables = new { 
                    input = new { 
                        startDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        endDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        groupBy = groupBy.ToString()
                    } 
                }
            };

            var response = await _graphQLService.Client.SendQueryAsync<TimelineResponse>(request);
            return response.Data?.RevenueAndProfitTimeline ?? new List<RevenueProfit>();
        }

        public async Task<List<StaffPerformanceData>> GetAllStaffPerformanceAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GraphQLReportRepository] Calling allStaffPerformance with startDate={startDate}, endDate={endDate}");
                
                var request = new GraphQLRequest
                {
                    Query = @"
                    query AllStaffPerformance($input: StaffPerformanceInput!) {
                        allStaffPerformance(input: $input) {
                            staffId
                            username
                            email
                            totalOrders
                            totalRevenue
                            totalProfit
                        }
                    }",
                    Variables = new { 
                        input = new { 
                            startDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            endDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        } 
                    }
                };

                var response = await _graphQLService.Client.SendQueryAsync<StaffPerformanceResponse>(request);
                
                if (response.Errors != null && response.Errors.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[GraphQLReportRepository] GraphQL Errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
                }
                
                var result = response.Data?.AllStaffPerformance ?? new List<StaffPerformanceData>();
                System.Diagnostics.Debug.WriteLine($"[GraphQLReportRepository] Response received: {result.Count} staff members");
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GraphQLReportRepository] Exception: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        // Response wrappers
        private class ReportResponse { public PeriodReport? ReportByPeriod { get; set; } }
        private class TopProductsResponse { public List<ProductSalesData>? TopProductsByQuantity { get; set; } }
        private class TopCustomersResponse { public List<CustomerSalesData>? TopCustomers { get; set; } }
        private class TimelineResponse { public List<RevenueProfit>? RevenueAndProfitTimeline { get; set; } }
        private class StaffPerformanceResponse { public List<StaffPerformanceData>? AllStaffPerformance { get; set; } }

        // Override unused base methods
        public override Task<object?> GetByIdAsync(int id) => throw new NotImplementedException();
        public override Task<List<object>> GetAllAsync() => throw new NotImplementedException();
        public override Task<object> AddAsync(object entity) => throw new NotImplementedException();
        public override Task UpdateAsync(object entity) => throw new NotImplementedException();
        public override Task DeleteAsync(int id) => throw new NotImplementedException();
        public override Task<int> CountAsync() => throw new NotImplementedException();
    }
}
