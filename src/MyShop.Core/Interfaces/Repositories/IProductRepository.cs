using MyShop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> SearchByNameAsync(string keyword);
        Task<List<Product>> GetByCategoryAsync(int categoryId);
        Task<List<Product>> GetLowStockProductsAsync(int threshold = 10);
    }
}