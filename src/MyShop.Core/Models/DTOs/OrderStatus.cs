using System.Text.Json.Serialization;

namespace MyShop.Core.Models.DTOs;

/// <summary>
/// Order status enum matching GraphQL backend
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    PENDING,
    PROCESSING,
    COMPLETED,
    CANCELLED
}
