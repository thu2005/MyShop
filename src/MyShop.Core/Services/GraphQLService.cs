
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MyShop.Core.Interfaces.Services;

namespace MyShop.Core.Services
{
    public class GraphQLService : IGraphQLService
    {
        private readonly GraphQLHttpClient _client;

        public GraphQLService(string endpoint)
        {
            _client = new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(endpoint)
            }, new SystemTextJsonSerializer());
        }

        public GraphQLHttpClient Client => _client;

        public async Task<T> QueryAsync<T>(string query, object? variables = null, CancellationToken cancellationToken = default)
        {
            var request = new GraphQL.GraphQLRequest
            {
                Query = query,
                Variables = variables
            };

            var response = await _client.SendQueryAsync<T>(request, cancellationToken);

            if (response.Errors != null && response.Errors.Length > 0)
            {
                var errorMessage = string.Join(", ", response.Errors.Select(e => e.Message));
                throw new GraphQLException($"GraphQL query error: {errorMessage}");
            }

            if (response.Data == null)
            {
                throw new GraphQLException("GraphQL response data is null");
            }

            return response.Data;
        }

        public void SetAuthToken(string token)
        {
            _client.HttpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
