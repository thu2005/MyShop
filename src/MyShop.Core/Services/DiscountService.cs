using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.App.Services
{
    public class DiscountService
    {
        private readonly IDiscountRepository _discountRepository;

        public DiscountService(IDiscountRepository discountRepository)
        {
            _discountRepository = discountRepository;
        }

        public async Task<List<Discount>> GetAllDiscountsAsync()
        {
            return await _discountRepository.GetAllAsync();
        }

        public async Task CreateDiscountAsync(Discount discount)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(discount.Code))
                throw new ArgumentException("Discount code is required");

            await _discountRepository.AddAsync(discount);
        }

        public async Task<bool> ApplyDiscountAsync(Order order, string code)
        {
            var discount = await _discountRepository.GetByCodeAsync(code);

            if (discount == null) return false;
            if (!ValidateDiscount(discount, order)) return false;

            decimal discountAmount = 0;

            switch (discount.Type)
            {
                case DiscountType.PERCENTAGE:
                    discountAmount = order.Subtotal * (discount.Value / 100);
                    if (discount.MaxDiscount.HasValue && discountAmount > discount.MaxDiscount.Value)
                        discountAmount = discount.MaxDiscount.Value;
                    break;

                case DiscountType.FIXED_AMOUNT:
                    discountAmount = discount.Value;
                    break;

                // Simplified for other types
                default:
                    discountAmount = 0;
                    break;
            }

            // Ensure discount doesn't exceed subtotal
            if (discountAmount > order.Subtotal) discountAmount = order.Subtotal;

            order.Discount = discount;
            order.DiscountId = discount.Id;
            order.DiscountAmount = discountAmount;
            order.Total = order.Subtotal + order.TaxAmount - discountAmount;

            return true;
        }

        public bool ValidateDiscount(Discount discount, Order order)
        {
            if (!discount.IsActive) return false;

            var now = DateTime.UtcNow;
            if (discount.StartDate.HasValue && now < discount.StartDate.Value) return false;
            if (discount.EndDate.HasValue && now > discount.EndDate.Value) return false;

            if (discount.UsageLimit.HasValue && discount.UsageCount >= discount.UsageLimit.Value) return false;

            if (discount.MinPurchase.HasValue && order.Subtotal < discount.MinPurchase.Value) return false;

            return true;
        }
    }
}