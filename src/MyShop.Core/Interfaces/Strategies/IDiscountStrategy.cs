using MyShop.Core.Models;

namespace MyShop.Core.Interfaces.Strategies
{
    public interface IDiscountStrategy
    {
        /// <summary>
        /// Calculates the discount amount based on the order and discount rules.
        /// </summary>
        decimal CalculateDiscount(Order order, Discount discount);
    }
}