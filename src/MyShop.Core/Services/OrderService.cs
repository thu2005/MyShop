using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.App.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<Order>> GetAllOrdersAsync() => await _orderRepository.GetAllAsync();

        public async Task<Order?> GetOrderByIdAsync(int id) => await _orderRepository.GetByIdAsync(id);

        public async Task CreateOrderAsync(Order order) => await _orderRepository.AddAsync(order);

        public async Task UpdateOrderAsync(Order order) => await _orderRepository.UpdateAsync(order);

        public async Task DeleteOrderAsync(int id) => await _orderRepository.DeleteAsync(id);

        public List<Order> SearchOrders(IEnumerable<Order> orders, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return orders.ToList();
            var lowerKeyword = keyword.ToLower();
            return orders.Where(o =>
                (o.OrderNumber?.ToLower().Contains(lowerKeyword) ?? false) ||
                (o.Customer?.Name?.ToLower().Contains(lowerKeyword) ?? false)
            ).ToList();
        }

        public List<Order> FilterByStatus(IEnumerable<Order> orders, OrderStatus? status)
        {
            if (!status.HasValue) return orders.ToList();
            return orders.Where(o => o.Status == status.Value).ToList();
        }

        public List<Order> FilterByDateRange(IEnumerable<Order> orders, DateTime? startDate, DateTime? endDate)
        {
            var query = orders;
            if (startDate.HasValue) query = query.Where(o => o.CreatedAt.Date >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(o => o.CreatedAt.Date <= endDate.Value.Date);
            return query.ToList();
        }
    }
}