using System;
using System.Collections.Generic;
using MyShop.Core.Models;

namespace MyShop.App.Models
{
    public class OrderDraft
    {
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? DiscountId { get; set; }
        public string DiscountCode { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public List<OrderItemDraft> Items { get; set; } = new();
        public DateTime SavedAt { get; set; }
    }

    public class OrderItemDraft
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
