using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.App.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<Product>> LoadProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            await _productRepository.AddAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }

        public List<Product> SearchProducts(IEnumerable<Product> products, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return products.ToList();

            var lowerKeyword = keyword.ToLower();
            return products.Where(p =>
                (p.Name?.ToLower().Contains(lowerKeyword) ?? false) ||
                (p.Sku?.ToLower().Contains(lowerKeyword) ?? false) ||
                (p.Description?.ToLower().Contains(lowerKeyword) ?? false)
            ).ToList();
        }

        public List<Product> FilterByCategory(IEnumerable<Product> products, int categoryId)
        {
            if (categoryId == 0) return products.ToList();
            return products.Where(p => p.CategoryId == categoryId).ToList();
        }

        public List<Product> FilterByPriceRange(IEnumerable<Product> products, decimal? minPrice, decimal? maxPrice)
        {
            var query = products;
            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            return query.ToList();
        }

        public List<Product> SortProducts(IEnumerable<Product> products, string sortOption)
        {
            return sortOption switch
            {
                "PriceAsc" => products.OrderBy(p => p.Price).ToList(),
                "PriceDesc" => products.OrderByDescending(p => p.Price).ToList(),
                "NameAsc" => products.OrderBy(p => p.Name).ToList(),
                "NameDesc" => products.OrderByDescending(p => p.Name).ToList(),
                "StockAsc" => products.OrderBy(p => p.Stock).ToList(),
                "StockDesc" => products.OrderByDescending(p => p.Stock).ToList(),
                _ => products.ToList(),
            };
        }
    }
}