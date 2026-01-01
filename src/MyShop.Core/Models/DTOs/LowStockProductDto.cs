using System.Text.Json.Serialization;

namespace MyShop.Core.Models.DTOs;

/// <summary>
/// Low stock product DTO
/// </summary>
public class LowStockProductDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("mainImage")]
    public string? MainImage { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("stock")]
    public int Stock { get; set; }
}

/// <summary>
/// Response wrapper for lowStockProducts query
/// </summary>
public class LowStockResponse
{
    [JsonPropertyName("lowStockProducts")]
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
}
