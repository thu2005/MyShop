using GraphQL;
using GraphQL.Client.Abstractions;
using Moq;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using static MyShop.Data.Repositories.GraphQLProductRepository;

namespace MyShop.Tests.UnitTests.Repositories
{
    public class ProductRepositoryTests
    {
        private readonly Mock<IGraphQLClient> _mockClient;
        private readonly MockGraphQLService _mockService;
        private readonly GraphQLProductRepository _repository;

        public ProductRepositoryTests()
        {
            _mockClient = new Mock<IGraphQLClient>();
            _mockService = new MockGraphQLService(_mockClient.Object);
            _repository = new GraphQLProductRepository(_mockService);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenFound()
        {
            var expectedProduct = new Product { Id = 1, Name = "Test Product", Price = 100 };

            // Uses the Repository's Public DTO class
            var response = new GraphQLResponse<ProductResponse>
            {
                Data = new ProductResponse { Product = expectedProduct }
            };

            SetupQuery<ProductResponse>(response);

            var result = await _repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Product", result.Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnList_WhenSuccessful()
        {
            var expectedProducts = new List<Product>
            {
                new Product { Id = 1, Name = "P1" },
                new Product { Id = 2, Name = "P2" }
            };

            var response = new GraphQLResponse<ProductsResponse>
            {
                Data = new ProductsResponse
                {
                    Products = new ProductsQueryResult { Products = expectedProducts, Total = 2 }
                }
            };

            SetupQuery<ProductsResponse>(response);

            var result = await _repository.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("P1", result[0].Name);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnCreatedProduct()
        {
            var newProduct = new Product { Name = "New Product", Price = 50 };
            var createdProduct = new Product { Id = 10, Name = "New Product", Price = 50 };

            var response = new GraphQLResponse<CreateProductResponse>
            {
                Data = new CreateProductResponse { CreateProduct = createdProduct }
            };

            SetupMutation<CreateProductResponse>(response);

            var result = await _repository.AddAsync(newProduct);

            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("New Product", result.Name);
        }

        [Fact]
        public async Task SearchByNameAsync_ShouldReturnMatches()
        {
            var matches = new List<Product> { new Product { Id = 1, Name = "Apple" } };
            var response = new GraphQLResponse<ProductsResponse>
            {
                Data = new ProductsResponse
                {
                    Products = new ProductsQueryResult { Products = matches }
                }
            };

            SetupQuery<ProductsResponse>(response);

            var result = await _repository.SearchByNameAsync("Apple");

            Assert.Single(result);
            Assert.Equal("Apple", result[0].Name);
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnList()
        {
            var lowStock = new List<Product> { new Product { Id = 5, Stock = 1 } };
            var response = new GraphQLResponse<LowStockResponse>
            {
                Data = new LowStockResponse { LowStockProducts = lowStock }
            };

            SetupQuery<LowStockResponse>(response);

            var result = await _repository.GetLowStockProductsAsync(5);

            Assert.Single(result);
            Assert.Equal(1, result[0].Stock);
        }

        // --- Helper Methods ---

        private void SetupQuery<T>(GraphQLResponse<T> response)
        {
            _mockClient
                .Setup(c => c.SendQueryAsync<T>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
        }

        private void SetupMutation<T>(GraphQLResponse<T> response)
        {
            _mockClient
                .Setup(c => c.SendMutationAsync<T>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
        }
    }

    public class MockGraphQLService : GraphQLService
    {
        private readonly IGraphQLClient _mockClient;

        public MockGraphQLService(IGraphQLClient mockClient) : base("http://mock-url")
        {
            _mockClient = mockClient;
        }

        public override IGraphQLClient Client => _mockClient;
    }
}