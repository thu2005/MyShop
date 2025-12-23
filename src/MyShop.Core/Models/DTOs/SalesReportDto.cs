using System.Text.Json.Serialization;

namespace MyShop.Core.Models.DTOs;

/// <summary>
/// Sales report DTO containing aggregated sales data
/// </summary>
public class SalesReportDto
{
    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }

    [JsonPropertyName("totalOrders")]
    public int TotalOrders { get; set; }

    [JsonPropertyName("averageOrderValue")]
    public decimal AverageOrderValue { get; set; }

    [JsonPropertyName("topProducts")]
    public List<TopProductDto> TopProducts { get; set; } = new();

    [JsonPropertyName("revenueByDate")]
    public List<RevenueByDateDto> RevenueByDate { get; set; } = new();
}

/// <summary>
/// Response wrapper for salesReport query
/// </summary>
public class SalesReportResponse
{
    [JsonPropertyName("salesReport")]
    public SalesReportDto? SalesReport { get; set; }
}
