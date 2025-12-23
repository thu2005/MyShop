using System.Text.Json.Serialization;

namespace MyShop.Core.Models.DTOs;

/// <summary>
/// Order DTO for recent orders display
/// </summary>
public class OrderDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public OrderStatus Status { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("isPaid")]
    public bool IsPaid { get; set; }
}

/// <summary>
/// Internal GraphQL order response model
/// </summary>
public class OrderGraphQLDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("customer")]
    public CustomerInfo? Customer { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Customer info for order response
/// </summary>
public class CustomerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Order list wrapper
/// </summary>
public class OrderList
{
    [JsonPropertyName("orders")]
    public List<OrderGraphQLDto> Orders { get; set; } = new();
}

/// <summary>
/// Response wrapper for orders query
/// </summary>
public class OrderListResponse
{
    [JsonPropertyName("orders")]
    public OrderList? Orders { get; set; }
}
