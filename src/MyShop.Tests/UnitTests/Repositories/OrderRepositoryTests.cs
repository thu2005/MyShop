using GraphQL;
using GraphQL.Client.Abstractions;
using Moq;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

// Static import allows us to use the DTOs (OrderResponse, etc.) directly
using static MyShop.Data.Repositories.GraphQLOrderRepository;

namespace MyShop.Tests.UnitTests.Repositories
{
    public class OrderRepositoryTests
    {
        private readonly Mock<IGraphQLClient> _mockClient;
        private readonly OrderMockGraphQLService _mockService;
        private readonly GraphQLOrderRepository _repository;

        public OrderRepositoryTests()
        {
            _mockClient = new Mock<IGraphQLClient>();
            // Wraps the mock client in our service helper
            _mockService = new OrderMockGraphQLService(_mockClient.Object);
            _repository = new GraphQLOrderRepository(_mockService);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnOrder_WhenFound()
        {
            // Arrange
            var expectedOrder = new Order { Id = 1, OrderNumber = "ORD-001", Total = 100 };
            var response = new GraphQLResponse<OrderResponse>
            {
                Data = new OrderResponse { Order = expectedOrder }
            };

            SetupQuery<OrderResponse>(response);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ORD-001", result.OrderNumber);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, OrderNumber = "ORD-1" },
                new Order { Id = 2, OrderNumber = "ORD-2" }
            };

            var response = new GraphQLResponse<OrdersResponse>
            {
                Data = new OrdersResponse
                {
                    Orders = new OrdersQueryResult { Orders = orders, Total = 2 }
                }
            };

            SetupQuery<OrdersResponse>(response);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("ORD-1", result[0].OrderNumber);
        }

        [Fact]
        public async Task GetByDateRangeAsync_ShouldReturnFilteredOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, CreatedAt = DateTime.Now.AddDays(-1) }
            };
            var response = new GraphQLResponse<OrdersResponse>
            {
                Data = new OrdersResponse
                {
                    Orders = new OrdersQueryResult { Orders = orders }
                }
            };

            SetupQuery<OrdersResponse>(response);

            // Act
            var result = await _repository.GetByDateRangeAsync(DateTime.Now.AddDays(-2), DateTime.Now);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetTotalRevenueAsync_ShouldSumCompletedOrders()
        {
            // Arrange
            // The repository implementation gets all orders (or date ranged) and filters in memory
            var orders = new List<Order>
            {
                new Order { Id = 1, Total = 100, Status = OrderStatus.COMPLETED },
                new Order { Id = 2, Total = 50, Status = OrderStatus.PENDING }, // Should be ignored
                new Order { Id = 3, Total = 200, Status = OrderStatus.COMPLETED }
            };

            var response = new GraphQLResponse<OrdersResponse>
            {
                Data = new OrdersResponse
                {
                    Orders = new OrdersQueryResult { Orders = orders }
                }
            };

            SetupQuery<OrdersResponse>(response);

            // Act
            var revenue = await _repository.GetTotalRevenueAsync(DateTime.Now.AddDays(-7), DateTime.Now);

            // Assert
            Assert.Equal(300, revenue); // 100 + 200
        }

        [Fact]
        public async Task AddAsync_ShouldReturnCreatedOrder()
        {
            // Arrange
            var newOrder = new Order { CustomerId = 1, Total = 150 };
            var createdOrder = new Order { Id = 10, OrderNumber = "NEW-001", Total = 150 };

            var response = new GraphQLResponse<CreateOrderResponse>
            {
                Data = new CreateOrderResponse { CreateOrder = createdOrder }
            };

            SetupMutation<CreateOrderResponse>(response);

            // Act
            var result = await _repository.AddAsync(newOrder);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("NEW-001", result.OrderNumber);
        }

        [Fact]
        public async Task UpdateAsync_ShouldExecuteMutation()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.CANCELLED };
            var response = new GraphQLResponse<dynamic>
            {
                Data = new { updateOrder = new { id = 1 } }
            };

            SetupMutation<dynamic>(response);

            // Act
            await _repository.UpdateAsync(order);

            // Assert
            // Verify that SendMutationAsync was called exactly once
            _mockClient.Verify(c => c.SendMutationAsync<dynamic>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldExecuteCancelMutation()
        {
            // Arrange
            var response = new GraphQLResponse<dynamic>
            {
                Data = new { cancelOrder = new { id = 1, status = "CANCELLED" } }
            };

            SetupMutation<dynamic>(response);

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            _mockClient.Verify(c => c.SendMutationAsync<dynamic>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()), Times.Once);
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

        internal class OrderMockGraphQLService : GraphQLService
        {
            private readonly IGraphQLClient _mockClient;

            public OrderMockGraphQLService(IGraphQLClient mockClient) : base("http://mock-url")
            {
                _mockClient = mockClient;
            }

            public override IGraphQLClient Client => _mockClient;
        }
    }
}