using System.Text.Json.Serialization;

namespace MyShop.Core.Models.DTOs;

/// <summary>
/// Dashboard statistics DTO matching GraphQL dashboardStats query
/// </summary>
public class DashboardStatsDto
{
    [JsonPropertyName("totalProducts")]
    public int TotalProducts { get; set; }

    [JsonPropertyName("totalOrders")]
    public int TotalOrders { get; set; }

    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }

    [JsonPropertyName("totalCustomers")]
    public int TotalCustomers { get; set; }

    [JsonPropertyName("lowStockProducts")]
    public int LowStockProducts { get; set; }

    [JsonPropertyName("pendingOrders")]
    public int PendingOrders { get; set; }

    [JsonPropertyName("todayRevenue")]
    public decimal TodayRevenue { get; set; }

    [JsonPropertyName("todayOrders")]
    public int TodayOrders { get; set; }
}

/// <summary>
/// Response wrapper for dashboardStats query
/// </summary>
public class DashboardStatsResponse
{
    [JsonPropertyName("dashboardStats")]
    public DashboardStatsDto? DashboardStats { get; set; }
}
