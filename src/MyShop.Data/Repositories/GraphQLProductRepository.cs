using GraphQL;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
                            costPrice
                            stock
                            minStock
                            categoryId
                            isActive
                            createdAt
                            updatedAt
                            images {
                                id
                                imageUrl
                                displayOrder
                                isMain
                            }
                            mainImage
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
                        products(pagination: { pageSize: 1000 }) {
                            products {
                                id
                                name
                                description
                                sku
                                barcode
                                price
                                costPrice
                                stock
                                popularity
                                categoryId
                                category {
                                    id
                                    name
                                }
                                isActive
                                images {
                                    id
                                    imageUrl
                                    displayOrder
                                    isMain
                                }
                                mainImage
                            }
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<ProductsResponse>(request);
            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }
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
                                categoryId
                                mainImage
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
                                categoryId
                                mainImage
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
                            mainImage
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
                            costPrice
                            stock
                            categoryId
                            images {
                                id
                                imageUrl
                                displayOrder
                                isMain
                            }
                        }
                    }",
                Variables = new
                {
                    input = new
                    {
                        name = entity.Name,
                        sku = entity.Sku,
                        price = entity.Price,
                        costPrice = entity.CostPrice,
                        stock = entity.Stock,
                        categoryId = entity.CategoryId,
                        description = entity.Description,
                        barcode = entity.Barcode,
                        minStock = entity.MinStock,
                        imageUrls = entity.Images?.Select(i => i.ImageUrl).ToList()
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<CreateProductResponse>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"Failed to add product: {response.Errors[0].Message}");
            }

            if (response.Data?.CreateProduct == null)
            {
                throw new Exception("Server returned success but no data.");
            }

            return response.Data.CreateProduct;
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
                            images {
                                id
                                imageUrl
                                displayOrder
                                isMain
                            }
                        }
                    }",
                Variables = new
                {
                    id = entity.Id,
                    input = new
                    {
                        name = entity.Name,
                        description = entity.Description,
                        sku = entity.Sku,
                        price = entity.Price,
                        costPrice = entity.CostPrice,
                        stock = entity.Stock,
                        categoryId = entity.CategoryId,
                        imageUrls = entity.Images?.Select(i => i.ImageUrl).ToList(),
                        mainImageIndex = entity.Images?.FindIndex(i => i.IsMain)
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<dynamic>(request);
            if (response.Errors != null && response.Errors.Any())
                throw new Exception($"Update failed: {response.Errors[0].Message}");
        }

        public override async Task DeleteAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"mutation DeleteProduct($id: Int!) { deleteProduct(id: $id) }",
                Variables = new { id }
            };
            var response = await _graphQLService.Client.SendMutationAsync<dynamic>(request);
            if (response.Errors != null && response.Errors.Any())
                throw new Exception($"Delete failed: {response.Errors[0].Message}");
        }

        public override async Task<int> CountAsync()
        {
            var request = new GraphQLRequest { Query = @"query GetTotal { products { total } }" };
            var response = await _graphQLService.Client.SendQueryAsync<ProductsResponse>(request);
            return response.Data?.Products?.Total ?? 0;
        }

        public class ProductResponse { public Product? Product { get; set; } }
        public class ProductsResponse { public ProductsQueryResult? Products { get; set; } }
        public class ProductsQueryResult { public List<Product>? Products { get; set; } public int Total { get; set; } }
        public class LowStockResponse { public List<Product>? LowStockProducts { get; set; } }
        public class CreateProductResponse { public Product? CreateProduct { get; set; } }
    }
}