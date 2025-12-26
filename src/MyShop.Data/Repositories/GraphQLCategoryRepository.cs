using GraphQL;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Data.Repositories
{
    public class GraphQLCategoryRepository : GraphQLRepositoryBase<Category>, ICategoryRepository
    {
        public GraphQLCategoryRepository(GraphQLService graphQLService)
            : base(graphQLService, "category")
        {
        }

        public override async Task<Category?> GetByIdAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetCategory($id: Int!) {
                        category(id: $id) {
                            id
                            name
                            description
                            isActive
                        }
                    }",
                Variables = new { id }
            };

            var response = await _graphQLService.Client.SendQueryAsync<CategoryResponse>(request);
            return response.Data?.Category;
        }

        public override async Task<List<Category>> GetAllAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetCategories {
                        categories(pagination: { pageSize: 100 }) {
                            categories {
                                id
                                name
                                description
                                isActive
                            }
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<CategoriesResponse>(request);
            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }

            return response.Data?.Categories?.Categories ?? new List<Category>();
        }

        public override async Task<Category> AddAsync(Category entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation CreateCategory($input: CreateCategoryInput!) {
                        createCategory(input: $input) {
                            id
                            name
                            description
                            isActive
                        }
                    }",
                Variables = new
                {
                    input = new
                    {
                        name = entity.Name,
                        description = entity.Description,
                        isActive = entity.IsActive
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<CreateCategoryResponse>(request);
            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }

            return response.Data?.CreateCategory ?? entity;
        }

        public override async Task UpdateAsync(Category entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation UpdateCategory($id: Int!, $input: UpdateCategoryInput!) {
                        updateCategory(id: $id, input: $input) {
                            id
                            name
                            description
                            isActive
                        }
                    }",
                Variables = new
                {
                    id = entity.Id,
                    input = new
                    {
                        name = entity.Name,
                        description = entity.Description,
                        isActive = entity.IsActive
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<UpdateCategoryResponse>(request);
            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }
        }

        public override async Task DeleteAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation DeleteCategory($id: Int!) {
                        deleteCategory(id: $id)
                    }",
                Variables = new { id }
            };

            var response = await _graphQLService.Client.SendMutationAsync<DeleteCategoryResponse>(request);
            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }
        }

        public override async Task<int> CountAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query CountCategories {
                        categories(pagination: { pageSize: 1 }) {
                            totalCount
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<CountResponse>(request);
            return response.Data?.Categories?.TotalCount ?? 0;
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetCategoryByName($name: String!) {
                        categories(pagination: { pageSize: 1 }, filter: { name: $name }) {
                            categories {
                                id
                                name
                                description
                                isActive
                            }
                        }
                    }",
                Variables = new { name }
            };

            var response = await _graphQLService.Client.SendQueryAsync<CategoriesResponse>(request);
            return response.Data?.Categories?.Categories?.FirstOrDefault();
        }

        // Response classes
        private class CategoryResponse
        {
            public Category? Category { get; set; }
        }

        private class CategoriesResponse
        {
            public CategoriesData? Categories { get; set; }
        }

        private class CategoriesData
        {
            public List<Category> Categories { get; set; } = new();
        }

        private class CreateCategoryResponse
        {
            public Category? CreateCategory { get; set; }
        }

        private class UpdateCategoryResponse
        {
            public Category? UpdateCategory { get; set; }
        }

        private class DeleteCategoryResponse
        {
            public bool DeleteCategory { get; set; }
        }

        private class CountResponse
        {
            public CountData? Categories { get; set; }
        }

        private class CountData
        {
            public int TotalCount { get; set; }
        }
    }
}
