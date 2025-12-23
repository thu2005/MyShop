namespace MyShop.Core.Services;

/// <summary>
/// Custom exception for GraphQL errors
/// </summary>
public class GraphQLException : Exception
{
    public GraphQLException(string message) : base(message)
    {
    }

    public GraphQLException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
