using MyShop.Core.Models.DTOs;

namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Dashboard service interface for fetching dashboard data
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get sales report for date range
    /// </summary>
    /// <param name="from">Start date (optional)</param>
    /// <param name="to">End date (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<SalesReportDto> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent orders
    /// </summary>
    /// <param name="count">Number of orders to fetch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<OrderDto>> GetRecentOrdersAsync(int count = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low stock products
    /// </summary>
    /// <param name="threshold">Stock threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default);
}
