using GraphQL;
using GraphQL.Client.Http;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyShop.Data.Repositories.Base
{
    public abstract class GraphQLRepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly GraphQLService _graphQLService;
        protected readonly string _typeName;

        protected GraphQLRepositoryBase(GraphQLService graphQLService, string typeName)
        {
            _graphQLService = graphQLService;
            _typeName = typeName;
        }

        public abstract Task<T?> GetByIdAsync(int id);
        public abstract Task<List<T>> GetAllAsync();
        
        public virtual Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotSupportedException("Generic FindAsync with Expressions is not supported in GraphQL repository. Please use specific search methods.");
        }

        public abstract Task<T> AddAsync(T entity);
        public abstract Task UpdateAsync(T entity);
        public abstract Task DeleteAsync(int id);
        public abstract Task<int> CountAsync();

        public virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotSupportedException("Generic CountAsync with Expressions is not supported in GraphQL repository.");
        }

        public virtual Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotSupportedException("Generic ExistsAsync with Expressions is not supported in GraphQL repository.");
        }

        protected async Task<TResult?> SendQueryAsync<TResult>(GraphQLRequest request, string queryName)
        {
            var response = await _graphQLService.Client.SendQueryAsync<dynamic>(request);
            if (response.Errors != null && response.Errors.Length > 0)
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }

            var data = response.Data;
            if (data == null) return default;

            try
            {
                var resultData = data[queryName];
                if (resultData == null) return default;
                
                // Use System.Text.Json or Newtonsoft via GraphQL.Client's built-in conversion
                return ((object)resultData).ToString() is string json 
                    ? System.Text.Json.JsonSerializer.Deserialize<TResult>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    : default;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing GraphQL response for query '{queryName}': {ex.Message}", ex);
            }
        }
    }
}
