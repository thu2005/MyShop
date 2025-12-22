using GraphQL;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Data.Repositories
{
    public class GraphQLProductRepository : GraphQLRepositoryBase<Product>, IProductRepository
    {
        public GraphQLProductRepository(GraphQLService graphQLService) 
            : base(graphQLService, "product")
        {
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetProduct($id: Int!) {
                        product(id: $id) {
                            id
                            name
                            description
                            sku
                            barcode
                            price
                            stock
                            minStock
                            imageUrl
                            categoryId
                            isActive
                            createdAt
                            updatedAt
                        }
                    }",
                Variables = new { id }
            };

            var response = await _graphQLService.Client.SendQueryAsync<ProductResponse>(request);
            return response.Data?.Product;
        }

        public override async Task<List<Product>> GetAllAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetProducts {
                        products(pagination: { pageSize: 100 }) {
                            products {
                                id
                                name
                                sku
                                price
                                stock
                                categoryId
                                isActive
                            }
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<ProductsResponse>(request);
            return response.Data?.Products?.Products ?? new List<Product>();
        }

        public async Task<List<Product>> SearchByNameAsync(string keyword)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query SearchProducts($name: String!) {
                        products(filter: { name: $name }) {
                            products {
                                id
                                name
                                sku
                                price
                                stock
                            }
                        }
                    }",
                Variables = new { name = keyword }
            };

            var response = await _graphQLService.Client.SendQueryAsync<ProductsResponse>(request);
            return response.Data?.Products?.Products ?? new List<Product>();
        }

        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetProductsByCategory($categoryId: Int!) {
                        products(filter: { categoryId: $categoryId }) {
                            products {
                                id
                                name
                                sku
                                price
                                stock
                            }
                        }
                    }",
                Variables = new { categoryId }
            };

            var response = await _graphQLService.Client.SendQueryAsync<ProductsResponse>(request);
            return response.Data?.Products?.Products ?? new List<Product>();
        }

        public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetLowStock($threshold: Int) {
                        lowStockProducts(threshold: $threshold) {
                            id
                            name
                            sku
                            stock
                            minStock
                        }
                    }",
                Variables = new { threshold }
            };

            var response = await _graphQLService.Client.SendQueryAsync<LowStockResponse>(request);
            return response.Data?.LowStockProducts ?? new List<Product>();
        }

        public override async Task<Product> AddAsync(Product entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation CreateProduct($input: CreateProductInput!) {
                        createProduct(input: $input) {
                            id
                            name
                            sku
                            price
                            stock
                        }
                    }",
                Variables = new { 
                    input = new { 
                        name = entity.Name,
                        sku = entity.Sku,
                        price = entity.Price,
                        stock = entity.Stock,
                        categoryId = entity.CategoryId,
                        description = entity.Description,
                        barcode = entity.Barcode,
                        minStock = entity.MinStock,
                        imageUrl = entity.ImageUrl
                    } 
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<CreateProductResponse>(request);
            return response.Data?.CreateProduct ?? entity;
        }

        public override async Task UpdateAsync(Product entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation UpdateProduct($id: Int!, $input: UpdateProductInput!) {
                        updateProduct(id: $id, input: $input) {
                            id
                            name
                        }
                    }",
                Variables = new { 
                    id = entity.Id,
                    input = new { 
                        name = entity.Name,
                        sku = entity.Sku,
                        price = entity.Price,
                        stock = entity.Stock,
                        categoryId = entity.CategoryId,
                        isActive = entity.IsActive
                    } 
                }
            };

            await _graphQLService.Client.SendMutationAsync<dynamic>(request);
        }

        public override async Task DeleteAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation DeleteProduct($id: Int!) {
                        deleteProduct(id: $id)
                    }",
                Variables = new { id }
            };

            await _graphQLService.Client.SendMutationAsync<dynamic>(request);
        }

        public override async Task<int> CountAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetTotal {
                        products {
                            total
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<ProductsResponse>(request);
            return response.Data?.Products?.Total ?? 0;
        }

        private class ProductResponse { public Product? Product { get; set; } }
        private class ProductsResponse { public ProductsQueryResult? Products { get; set; } }
        private class ProductsQueryResult { public List<Product>? Products { get; set; } public int Total { get; set; } }
        private class LowStockResponse { public List<Product>? LowStockProducts { get; set; } }
        private class CreateProductResponse { public Product? CreateProduct { get; set; } }
    }
}
