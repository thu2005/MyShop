using MyShop.Core.Interfaces.Strategies;
using MyShop.Core.Models;
using System;

namespace MyShop.Core.Strategies
{
    public class PercentageDiscountStrategy : IDiscountStrategy
    {
        public decimal CalculateDiscount(Order order, Discount discount)
        {
            if (discount.Value <= 0) return 0;

            var amount = order.Subtotal * (discount.Value / 100);

            // Apply Max Discount Cap if exists
            if (discount.MaxDiscount.HasValue && amount > discount.MaxDiscount.Value)
            {
                return discount.MaxDiscount.Value;
            }

            return amount;
        }
    }

    public class FixedAmountDiscountStrategy : IDiscountStrategy
    {
        public decimal CalculateDiscount(Order order, Discount discount)
        {
            if (discount.Value <= 0) return 0;

            // Discount cannot exceed the subtotal
            return Math.Min(discount.Value, order.Subtotal);
        }
    }

    // Example of a slightly more complex strategy (if needed)
    public class MemberDiscountStrategy : IDiscountStrategy
    {
        public decimal CalculateDiscount(Order order, Discount discount)
        {
            // Only applies if user is a member (logic assumption for example)
            // In a real scenario, you might check order.Customer.IsMember
            if (order.Customer == null) return 0;

            return order.Subtotal * (discount.Value / 100);
        }
    }
}