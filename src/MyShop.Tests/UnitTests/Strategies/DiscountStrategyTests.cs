using MyShop.Core.Interfaces.Strategies;
using MyShop.Core.Models;
using MyShop.Core.Strategies;
using Xunit;

namespace MyShop.Tests.UnitTests.Strategies
{
    public class DiscountStrategyTests
    {
        [Fact]
        public void PercentageStrategy_ShouldCalculateCorrectAmount()
        {
            // Arrange
            var strategy = new PercentageDiscountStrategy();
            var order = new Order { Subtotal = 200.00m };
            var discount = new Discount { Value = 10 }; // 10%

            // Act
            var result = strategy.CalculateDiscount(order, discount);

            // Assert
            Assert.Equal(20.00m, result);
        }

        [Fact]
        public void PercentageStrategy_ShouldCapAtMaxDiscount()
        {
            // Arrange
            var strategy = new PercentageDiscountStrategy();
            var order = new Order { Subtotal = 1000.00m };
            var discount = new Discount
            {
                Value = 50, // 50% = 500
                MaxDiscount = 100.00m // Cap at 100
            };

            // Act
            var result = strategy.CalculateDiscount(order, discount);

            // Assert
            Assert.Equal(100.00m, result);
        }

        [Fact]
        public void FixedAmountStrategy_ShouldReturnExactValue()
        {
            // Arrange
            var strategy = new FixedAmountDiscountStrategy();
            var order = new Order { Subtotal = 200.00m };
            var discount = new Discount { Value = 50.00m };

            // Act
            var result = strategy.CalculateDiscount(order, discount);

            // Assert
            Assert.Equal(50.00m, result);
        }

        [Fact]
        public void FixedAmountStrategy_ShouldNotExceedSubtotal()
        {
            // Arrange
            var strategy = new FixedAmountDiscountStrategy();
            var order = new Order { Subtotal = 30.00m };
            var discount = new Discount { Value = 50.00m };

            // Act
            var result = strategy.CalculateDiscount(order, discount);

            // Assert
            Assert.Equal(30.00m, result); // Should be capped at subtotal
        }
    }
}