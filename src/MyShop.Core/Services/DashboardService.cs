using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models.DTOs;

namespace MyShop.Core.Services;

/// <summary>
/// Dashboard service implementation for fetching dashboard data from GraphQL API
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IGraphQLService _graphQLService;

    public DashboardService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
            query {
                dashboardStats {
                    totalProducts
                    totalOrders
                    totalRevenue
                    totalCustomers
                    lowStockProducts
                    pendingOrders
                    todayRevenue
                    todayOrders
                }
            }";

        var result = await _graphQLService.QueryAsync<DashboardStatsResponse>(query, null, cancellationToken);
        return result.DashboardStats ?? new DashboardStatsDto();
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        const string query = @"
            query SalesReport($dateRange: DateRangeInput!) {
                salesReport(dateRange: $dateRange) {
                    totalRevenue
                    totalOrders
                    averageOrderValue
                    topProducts {
                        product {
                            id
                            name
                            imageUrl
                            price
                        }
                        quantity
                        revenue
                    }
                    revenueByDate {
                        date
                        revenue
                        orders
                    }
                }
            }";

        var variables = new
        {
            dateRange = new
            {
                from = from?.ToString("yyyy-MM-ddT00:00:00.000Z"),  // ISO 8601 with UTC timezone
                to = to?.ToString("yyyy-MM-ddT23:59:59.999Z")
            }
        };

        var result = await _graphQLService.QueryAsync<SalesReportResponse>(query, variables, cancellationToken);
        return result.SalesReport ?? new SalesReportDto();
    }

    public async Task<List<OrderDto>> GetRecentOrdersAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        const string query = @"
            query RecentOrders($pagination: PaginationInput) {
                orders(pagination: $pagination) {
                    orders {
                        id
                        orderNumber
                        customer {
                            name
                        }
                        status
                        total
                        createdAt
                    }
                }
            }";

        var variables = new
        {
            pagination = new { page = 1, pageSize = count }
        };

        var result = await _graphQLService.QueryAsync<OrderListResponse>(query, variables, cancellationToken);

        if (result.Orders?.Orders == null)
        {
            return new List<OrderDto>();
        }

        // Map to OrderDto
        return result.Orders.Orders.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.Customer?.Name ?? "Guest",
            Status = Enum.Parse<OrderStatus>(o.Status),
            Total = o.Total,
            CreatedAt = o.CreatedAt,
            IsPaid = o.Status != "PENDING"
        }).ToList();
    }

    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        const string query = @"
            query LowStock($threshold: Int) {
                lowStockProducts(threshold: $threshold) {
                    id
                    name
                    imageUrl
                    price
                    stock
                }
            }";

        var variables = new { threshold };

        var result = await _graphQLService.QueryAsync<LowStockResponse>(query, variables, cancellationToken);
        
        return result.LowStockProducts ?? new List<LowStockProductDto>();
    }
}
