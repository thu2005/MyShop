using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Order>> GetByCustomerAsync(int customerId);
        Task<List<Order>> GetByStatusAsync(OrderStatus status);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalOrdersAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
