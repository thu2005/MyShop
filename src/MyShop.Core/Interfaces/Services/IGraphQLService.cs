namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Base interface for GraphQL operations with authentication and cancellation support
/// </summary>
public interface IGraphQLService
{
    /// <summary>
    /// Execute a GraphQL query
    /// </summary>
    /// <typeparam name="T">The expected response type</typeparam>
    /// <param name="query">The GraphQL query string</param>
    /// <param name="variables">Optional query variables</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deserialized response data</returns>
    Task<T> QueryAsync<T>(string query, object? variables = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the authentication token for subsequent requests
    /// </summary>
    /// <param name="token">JWT authentication token</param>
    void SetAuthToken(string token);
}
