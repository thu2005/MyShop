using MyShop.App.Services;
using MyShop.Core.Models;
using MyShop.Tests.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyShop.Tests.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly MockProductRepository _mockRepository;
        private readonly ProductService _service;
        private readonly List<Product> _testProducts;

        public ProductServiceTests()
        {
            _mockRepository = new MockProductRepository();
            _service = new ProductService(_mockRepository);

            // Seed Data for pure logic tests
            _testProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 1000, Stock = 5, CategoryId = 1 },
                new Product { Id = 2, Name = "Mouse", Price = 20, Stock = 50, CategoryId = 2 },
                new Product { Id = 3, Name = "Keyboard", Price = 50, Stock = 30, CategoryId = 2 },
                new Product { Id = 4, Name = "Monitor", Price = 300, Stock = 10, CategoryId = 1 }
            };

            // Seed Repo for Async/DB tests
            _mockRepository.Products = new List<Product>(_testProducts);
        }

        [Fact]
        public async Task LoadProductsAsync_ShouldReturnAllProducts()
        {
            var result = await _service.LoadProductsAsync();
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SearchProducts_ShouldFilterByName()
        {
            var result = _service.SearchProducts(_testProducts, "Mouse");
            Assert.Single(result);
            Assert.Equal("Mouse", result.First().Name);
        }

        [Fact]
        public void FilterByCategory_ShouldShowOnlyMatchingItems()
        {
            var result = _service.FilterByCategory(_testProducts, 1);
            Assert.Equal(2, result.Count); // Laptop & Monitor
            Assert.All(result, p => Assert.Equal(1, p.CategoryId));
        }

        [Fact]
        public void FilterByPriceRange_ShouldFilterCorrectly()
        {
            var result = _service.FilterByPriceRange(_testProducts, 100, 500);
            Assert.Single(result); // Monitor (300)
            Assert.Equal("Monitor", result.First().Name);
        }

        [Fact]
        public void SortProducts_PriceAsc_ShouldSortCorrectly()
        {
            var result = _service.SortProducts(_testProducts, "PriceAsc");
            Assert.Equal("Mouse", result.First().Name); // 20
            Assert.Equal("Laptop", result.Last().Name); // 1000
        }

        [Fact]
        public async Task AddProductAsync_ShouldAddToRepo()
        {
            var newProduct = new Product { Name = "Headphones", Price = 80, Stock = 15, CategoryId = 2 };
            await _service.AddProductAsync(newProduct);

            Assert.Contains(_mockRepository.Products, p => p.Name == "Headphones");
        }
    }
}
