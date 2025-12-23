using System.Text.Json.Serialization;

namespace MyShop.Core.Models.DTOs;

/// <summary>
/// Generic GraphQL response wrapper for handling GraphQL-specific responses and errors
/// </summary>
/// <typeparam name="T">The type of data returned in the response</typeparam>
public class GraphQLResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<GraphQLError>? Errors { get; set; }

    public bool HasErrors => Errors != null && Errors.Any();
}

/// <summary>
/// Represents a GraphQL error
/// </summary>
public class GraphQLError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}
