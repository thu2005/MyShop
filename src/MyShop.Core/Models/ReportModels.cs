using System;

namespace MyShop.Core.Models
{
    public enum PeriodType
    {
        WEEKLY,
        MONTHLY,
        YEARLY,
        CUSTOM
    }

    public enum TimelineGrouping
    {
        DAY,
        WEEK,
        MONTH
    }

    public class PeriodReport
    {
        public int TotalProductsSold { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        
        public int? PreviousTotalProductsSold { get; set; }
        public int? PreviousTotalOrders { get; set; }
        public decimal? PreviousTotalRevenue { get; set; }
        public decimal? PreviousTotalProfit { get; set; }
        
        public double? ProductsChange { get; set; }
        public double? OrdersChange { get; set; }
        public double? RevenueChange { get; set; }
        public double? ProfitChange { get; set; }
        
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class ProductSalesData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CustomerSalesData
    {
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class RevenueProfit
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public int Orders { get; set; }
    }

    public class StaffPerformanceData
    {
        public int StaffId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
