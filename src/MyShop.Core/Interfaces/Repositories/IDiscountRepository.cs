using MyShop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories
{
    public interface IDiscountRepository : IRepository<Discount>
    {
        Task<List<Discount>> GetActiveDiscountsAsync();
        Task<Discount?> GetByCodeAsync(string code);
    }
}
