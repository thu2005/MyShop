using MyShop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<List<Customer>> SearchAsync(string keyword);
        Task<(List<Customer> customers, int total)> GetCustomersAsync(
            int page = 1,
            int pageSize = 20,
            string? searchText = null,
            bool? isMember = null);
    }
}
