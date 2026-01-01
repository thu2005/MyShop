using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new GraphQLNullableDateTimeConverter(),
                    new GraphQLDateTimeConverter()
                }
            };

            _client = new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(endpoint)
            }, new SystemTextJsonSerializer(jsonSerializerOptions));
        }

        public GraphQLHttpClient ConcreteClient => _client;

        public virtual IGraphQLClient Client => _client;

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
                throw new Exception($"GraphQL query error: {errorMessage}");
            }

            if (response.Data == null)
            {
                throw new Exception("GraphQL response data is null");
            }

            return response.Data;
        }

        public void SetAuthToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _client.HttpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                _client.HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }

    /// <summary>
    /// Custom DateTime converter for GraphQL that ensures dates are serialized in ISO 8601 format
    /// </summary>
    public class GraphQLDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                if (DateTime.TryParse(dateString, out var date))
                {
                    return date;
                }
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Write DateTime in ISO 8601 format which GraphQL expects (with milliseconds precision)
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }

    /// <summary>
    /// Custom nullable DateTime converter for GraphQL
    /// </summary>
    public class GraphQLNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                if (string.IsNullOrEmpty(dateString))
                {
                    return null;
                }
                if (DateTime.TryParse(dateString, out var date))
                {
                    return date;
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                // Write DateTime in ISO 8601 format which GraphQL expects (with milliseconds precision)
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}