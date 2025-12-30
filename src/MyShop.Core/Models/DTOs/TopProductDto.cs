using System.Text.Json.Serialization;

namespace MyShop.Core.Models.DTOs;

/// <summary>
/// Product DTO for GraphQL responses
/// </summary>
public class ProductDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("mainImage")]
    public string? MainImage { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

/// <summary>
/// Top product DTO with sales statistics
/// </summary>
public class TopProductDto
{
    [JsonPropertyName("product")]
    public ProductDto? Product { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }
}
