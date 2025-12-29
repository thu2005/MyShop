using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyShop.Tests.Mocks
{
    public class MockOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; set; } = new List<Order>();

        public Task<Order> AddAsync(Order entity)
        {
            if (entity.Id == 0)
                entity.Id = Orders.Any() ? Orders.Max(o => o.Id) + 1 : 1;

            if (entity.CreatedAt == default)
                entity.CreatedAt = DateTime.UtcNow;

            Orders.Add(entity);
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(int id)
        {
            var order = Orders.FirstOrDefault(o => o.Id == id);
            if (order != null) Orders.Remove(order);
            return Task.CompletedTask;
        }

        public Task<List<Order>> GetAllAsync() => Task.FromResult(Orders.ToList());

        public Task<Order?> GetByIdAsync(int id) => Task.FromResult(Orders.FirstOrDefault(o => o.Id == id));

        public Task UpdateAsync(Order entity)
        {
            var existing = Orders.FirstOrDefault(o => o.Id == entity.Id);
            if (existing != null)
            {
                Orders.Remove(existing);
                Orders.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task<int> CountAsync() => Task.FromResult(Orders.Count);

        // Specific IOrderRepository methods
        public Task<List<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToList());
        }

        public Task<List<Order>> GetByCustomerAsync(int customerId)
        {
            return Task.FromResult(Orders.Where(o => o.CustomerId == customerId).ToList());
        }

        public Task<List<Order>> GetByStatusAsync(OrderStatus status)
        {
            return Task.FromResult(Orders.Where(o => o.Status == status).ToList());
        }

        public Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = Orders.AsQueryable();
            if (startDate.HasValue) query = query.Where(o => o.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(o => o.CreatedAt <= endDate.Value);

            return Task.FromResult(query.Where(o => o.Status != OrderStatus.CANCELLED).Sum(o => o.Total));
        }

        public Task<int> GetTotalOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = Orders.AsQueryable();
            if (startDate.HasValue) query = query.Where(o => o.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(o => o.CreatedAt <= endDate.Value);

            return Task.FromResult(query.Count());
        }

        public Task<int> CountAsync(Expression<Func<Order, bool>> predicate) => throw new NotImplementedException();
        public Task<List<Order>> FindAsync(Expression<Func<Order, bool>> predicate) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Expression<Func<Order, bool>> predicate) => throw new NotImplementedException();
    }
}