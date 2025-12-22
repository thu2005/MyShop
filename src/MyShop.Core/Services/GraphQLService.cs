using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System;

namespace MyShop.Core.Services
{
    public class GraphQLService
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
    }
}
