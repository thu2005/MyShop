using MyShop.App.Services;
using MyShop.Core.Models;
using MyShop.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyShop.Tests.UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly MockOrderRepository _mockRepository;
        private readonly OrderService _service;
        private readonly List<Order> _testOrders;

        public OrderServiceTests()
        {
            _mockRepository = new MockOrderRepository();
            _service = new OrderService(_mockRepository);

            // Seed Data
            _testOrders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    OrderNumber = "ORD-001",
                    Customer = new Customer { Id = 1, Name = "John Doe" },
                    Status = OrderStatus.COMPLETED,
                    Total = 150.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Order
                {
                    Id = 2,
                    OrderNumber = "ORD-002",
                    Customer = new Customer { Id = 2, Name = "Jane Smith" },
                    Status = OrderStatus.PENDING,
                    Total = 50.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Order
                {
                    Id = 3,
                    OrderNumber = "ORD-003",
                    Customer = new Customer { Id = 1, Name = "John Doe" },
                    Status = OrderStatus.CANCELLED,
                    Total = 200.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Order
                {
                    Id = 4,
                    OrderNumber = "ORD-004",
                    Customer = new Customer { Id = 3, Name = "Bob Wilson" },
                    Status = OrderStatus.PROCESSING,
                    Total = 75.50m,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Seed Repo
            _mockRepository.Orders = new List<Order>(_testOrders);
        }

        [Fact]
        public async Task LoadOrdersAsync_ShouldReturnAllOrders()
        {
            var result = await _service.GetAllOrdersAsync();
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnCorrectOrder()
        {
            var result = await _service.GetOrderByIdAsync(2);
            Assert.NotNull(result);
            Assert.Equal("ORD-002", result.OrderNumber);
        }

        [Fact]
        public void SearchOrders_ShouldFilterByOrderNumber()
        {
            var result = _service.SearchOrders(_testOrders, "ORD-003");
            Assert.Single(result);
            Assert.Equal("ORD-003", result.First().OrderNumber);
        }

        [Fact]
        public void FilterByStatus_ShouldShowOnlyMatchingItems()
        {
            var result = _service.FilterByStatus(_testOrders, OrderStatus.COMPLETED);
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
            Assert.Equal(OrderStatus.COMPLETED, result.First().Status);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldAddToRepo()
        {
            var newOrder = new Order
            {
                OrderNumber = "ORD-005",
                Status = OrderStatus.PENDING,
                Total = 100.00m
            };

            await _service.CreateOrderAsync(newOrder);

            Assert.Contains(_mockRepository.Orders, o => o.OrderNumber == "ORD-005");
        }
    }
}