using MyShop.App.Services;
using MyShop.Core.Models;
using MyShop.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MyShop.Tests.UnitTests.Services
{
    public class DiscountServiceTests
    {
        private readonly MockDiscountRepository _mockRepository;
        private readonly DiscountService _service;

        public DiscountServiceTests()
        {
            _mockRepository = new MockDiscountRepository();
            _service = new DiscountService(_mockRepository);
        }

        [Fact]
        public async Task ApplyDiscount_Percentage_ShouldCalculateCorrectly()
        {
            // Arrange
            var discount = new Discount
            {
                Code = "SAVE10",
                Type = DiscountType.PERCENTAGE,
                Value = 10, // 10%
                IsActive = true
            };
            await _service.CreateDiscountAsync(discount);

            var order = new Order { Subtotal = 200.00m };

            // Act
            var result = await _service.ApplyDiscountAsync(order, "SAVE10");

            // Assert
            Assert.True(result);
            Assert.Equal(20.00m, order.DiscountAmount); // 10% of 200
            Assert.Equal(180.00m, order.Total);
        }

        [Fact]
        public async Task ApplyDiscount_FixedAmount_ShouldCalculateCorrectly()
        {
            // Arrange
            var discount = new Discount
            {
                Code = "MINUS50",
                Type = DiscountType.FIXED_AMOUNT,
                Value = 50.00m,
                IsActive = true
            };
            await _service.CreateDiscountAsync(discount);

            var order = new Order { Subtotal = 200.00m };

            // Act
            var result = await _service.ApplyDiscountAsync(order, "MINUS50");

            // Assert
            Assert.True(result);
            Assert.Equal(50.00m, order.DiscountAmount);
            Assert.Equal(150.00m, order.Total);
        }

        [Fact]
        public async Task ApplyDiscount_MinPurchaseNotMet_ShouldFail()
        {
            // Arrange
            var discount = new Discount
            {
                Code = "BULK100",
                Type = DiscountType.FIXED_AMOUNT,
                Value = 20,
                MinPurchase = 1000.00m,
                IsActive = true
            };
            await _service.CreateDiscountAsync(discount);

            var order = new Order { Subtotal = 500.00m }; // Less than 1000

            // Act
            var result = await _service.ApplyDiscountAsync(order, "BULK100");

            // Assert
            Assert.False(result);
            Assert.Equal(0, order.DiscountAmount);
        }

        [Fact]
        public async Task ApplyDiscount_Expired_ShouldFail()
        {
            // Arrange
            var discount = new Discount
            {
                Code = "EXPIRED",
                Type = DiscountType.PERCENTAGE,
                Value = 10,
                IsActive = true,
                EndDate = DateTime.UtcNow.AddDays(-1)
            };
            await _service.CreateDiscountAsync(discount);

            var order = new Order { Subtotal = 100.00m };

            // Act
            var result = await _service.ApplyDiscountAsync(order, "EXPIRED");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateDiscount_ShouldAddToRepo()
        {
            var discount = new Discount { Code = "NEWCODE", Name = "New Deal" };

            await _service.CreateDiscountAsync(discount);

            var savedDiscount = await _mockRepository.GetByCodeAsync("NEWCODE");
            Assert.NotNull(savedDiscount);
        }
    }
}