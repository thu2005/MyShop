using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyShop.Tests.Mocks
{
    public class MockProductRepository : IProductRepository
    {
        public List<Product> Products { get; set; } = new List<Product>();

        public Task<Product> AddAsync(Product entity)
        {
            if (entity.Id == 0)
            {
                entity.Id = Products.Any() ? Products.Max(p => p.Id) + 1 : 1;
            }
            Products.Add(entity);
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                Products.Remove(product);
            }
            return Task.CompletedTask;
        }

        public Task<List<Product>> GetAllAsync()
        {
            return Task.FromResult(Products.ToList());
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            return Task.FromResult(Products.FirstOrDefault(p => p.Id == id));
        }

        public Task UpdateAsync(Product entity)
        {
            var existing = Products.FirstOrDefault(p => p.Id == entity.Id);
            if (existing != null)
            {
                Products.Remove(existing);
                Products.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task<List<Product>> SearchByNameAsync(string keyword)
        {
            var lowerKeyword = keyword.ToLower();
            return Task.FromResult(Products
                .Where(p => p.Name.ToLower().Contains(lowerKeyword))
                .ToList());
        }

        public Task<List<Product>> GetByCategoryAsync(int categoryId)
        {
            return Task.FromResult(Products.Where(p => p.CategoryId == categoryId).ToList());
        }

        public Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return Task.FromResult(Products.Where(p => p.Stock <= threshold).ToList());
        }

        // Implement generic IRepository methods not strictly used by ViewModel but required by interface
        public Task<int> CountAsync() => Task.FromResult(Products.Count);

        public Task<int> CountAsync(Expression<Func<Product, bool>> predicate) => throw new NotImplementedException();

        public Task<List<Product>> FindAsync(Expression<Func<Product, bool>> predicate) => throw new NotImplementedException();

        public Task<bool> ExistsAsync(Expression<Func<Product, bool>> predicate) => throw new NotImplementedException();
    }
}
