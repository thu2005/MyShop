using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyShop.Tests.Mocks
{
    public class MockDiscountRepository : IDiscountRepository
    {
        public List<Discount> Discounts { get; set; } = new List<Discount>();

        public Task<Discount> AddAsync(Discount entity)
        {
            if (entity.Id == 0)
                entity.Id = Discounts.Any() ? Discounts.Max(d => d.Id) + 1 : 1;
            Discounts.Add(entity);
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(int id)
        {
            var discount = Discounts.FirstOrDefault(d => d.Id == id);
            if (discount != null) Discounts.Remove(discount);
            return Task.CompletedTask;
        }

        public Task<List<Discount>> GetAllAsync() => Task.FromResult(Discounts.ToList());

        public Task<Discount?> GetByIdAsync(int id) => Task.FromResult(Discounts.FirstOrDefault(d => d.Id == id));

        public Task UpdateAsync(Discount entity)
        {
            var existing = Discounts.FirstOrDefault(d => d.Id == entity.Id);
            if (existing != null)
            {
                Discounts.Remove(existing);
                Discounts.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task<int> CountAsync() => Task.FromResult(Discounts.Count);

        // --- IDiscountRepository Implementation ---

        public Task<List<Discount>> GetActiveDiscountsAsync()
        {
            var now = DateTime.UtcNow;
            return Task.FromResult(Discounts.Where(d =>
                d.IsActive &&
                (d.StartDate == null || d.StartDate <= now) &&
                (d.EndDate == null || d.EndDate >= now)
            ).ToList());
        }

        public Task<Discount?> GetByCodeAsync(string code)
        {
            return Task.FromResult(Discounts.FirstOrDefault(d =>
                d.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));
        }

        // Unimplemented methods
        public Task<int> CountAsync(Expression<Func<Discount, bool>> predicate) => throw new NotImplementedException();
        public Task<List<Discount>> FindAsync(Expression<Func<Discount, bool>> predicate) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Expression<Func<Discount, bool>> predicate) => throw new NotImplementedException();
    }
}