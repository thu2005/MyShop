using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShop.Core.Models;

namespace MyShop.App.ViewModels
{
    public partial class OrderViewModel
    {
        public async Task ExportOrdersAsync()
        {
            try
            {
                IsBusy = true;

                // Use the already filtered orders list
                var ordersToExport = _allOrders.ToList();

                if (!ordersToExport.Any())
                {
                    ErrorMessage = "No orders to export.";
                    return;
                }

                // Build filter description
                var filterInfo = BuildFilterDescription();

                // Generate HTML report
                var html = GenerateOrdersHtmlReport(ordersToExport, filterInfo);

                // Save to temp file
                var tempPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(), 
                    $"OrdersReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                );
                await System.IO.File.WriteAllTextAsync(tempPath, html);

                // Open in default browser
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);

                System.Diagnostics.Debug.WriteLine($"Orders report opened: {tempPath}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to export orders: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error exporting orders: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string BuildFilterDescription()
        {
            var filters = new List<string>();

            if (SelectedStatus.HasValue)
            {
                filters.Add($"Status: {GetStatusDisplayText(SelectedStatus.Value)}");
            }

            if (SelectedPriceFilter != PriceFilter.All)
            {
                var priceText = SelectedPriceFilter switch
                {
                    PriceFilter.Low => "Under $100",
                    PriceFilter.Medium => "$100 - $500",
                    PriceFilter.High => "Above $500",
                    _ => SelectedPriceFilter.ToString()
                };
                filters.Add($"Price: {priceText}");
            }

            if (StartDate.HasValue)
            {
                filters.Add($"From: {StartDate.Value:MMM dd, yyyy}");
            }

            if (EndDate.HasValue)
            {
                filters.Add($"To: {EndDate.Value:MMM dd, yyyy}");
            }

            return filters.Any() ? string.Join(" | ", filters) : "All Orders";
        }

        private string GenerateOrdersHtmlReport(List<Order> orders, string filterInfo)
        {
            var sb = new StringBuilder();

            // Calculate summary statistics
            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.Total);
            var completedOrders = orders.Count(o => o.Status == OrderStatus.COMPLETED);
            var pendingOrders = orders.Count(o => o.Status == OrderStatus.PENDING);
            var cancelledOrders = orders.Count(o => o.Status == OrderStatus.CANCELLED);
            var processingOrders = orders.Count(o => o.Status == OrderStatus.PROCESSING);

            // Date range for title
            var minDate = orders.Any() ? orders.Min(o => o.CreatedAt) : DateTime.Now;
            var maxDate = orders.Any() ? orders.Max(o => o.CreatedAt) : DateTime.Now;

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<title>Orders Report</title>");

            // CSS Styles (matching Reports page style)
            sb.AppendLine("<style>");
            sb.AppendLine("@media print { @page { margin: 1cm; } .no-break { break-inside: avoid; } }");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; }");
            sb.AppendLine(".container { max-width: 1200px; margin: 0 auto; background: white; padding: 40px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("h1 { color: #003F62; margin: 0 0 10px 0; font-size: 32px; }");
            sb.AppendLine(".subtitle { color: #666; margin-bottom: 30px; font-size: 14px; }");
            sb.AppendLine(".filter-info { background: #e3f2fd; color: #003F62; padding: 10px 15px; border-radius: 6px; margin-bottom: 20px; font-size: 14px; }");
            sb.AppendLine("h2 { color: #003F62; margin: 40px 0 20px 0; padding-bottom: 10px; border-bottom: 2px solid #003F62; font-size: 20px; }");
            sb.AppendLine(".summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 20px; margin: 30px 0; }");
            sb.AppendLine(".card { background: linear-gradient(135deg, #003F62 0%, #005580 100%); color: white; padding: 25px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }");
            sb.AppendLine(".card.green { background: linear-gradient(135deg, #059669 0%, #10b981 100%); }");
            sb.AppendLine(".card.blue { background: linear-gradient(135deg, #0078d4 0%, #2196f3 100%); }");
            sb.AppendLine(".card.orange { background: linear-gradient(135deg, #d97706 0%, #f59e0b 100%); }");
            sb.AppendLine(".card.red { background: linear-gradient(135deg, #dc2626 0%, #ef4444 100%); }");
            sb.AppendLine(".card h3 { margin: 0 0 10px 0; font-size: 14px; opacity: 0.9; font-weight: normal; }");
            sb.AppendLine(".card .value { font-size: 32px; font-weight: bold; margin: 0; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            sb.AppendLine("thead { background: #003F62; color: white; }");
            sb.AppendLine("th { padding: 15px; text-align: left; font-weight: 600; }");
            sb.AppendLine("td { padding: 12px 15px; border-bottom: 1px solid #e0e0e0; }");
            sb.AppendLine("tbody tr:hover { background: #f9f9f9; }");
            sb.AppendLine("tbody tr:nth-child(even) { background: #fafafa; }");
            sb.AppendLine(".text-right { text-align: right; }");
            sb.AppendLine(".text-center { text-align: center; }");
            sb.AppendLine(".status { padding: 4px 12px; border-radius: 4px; font-size: 12px; font-weight: 600; display: inline-block; }");
            sb.AppendLine(".status-pending { background: rgba(255,159,0,0.2); color: #d97706; }");
            sb.AppendLine(".status-processing { background: rgba(0,120,212,0.2); color: #0078d4; }");
            sb.AppendLine(".status-completed { background: rgba(5,150,105,0.2); color: #059669; }");
            sb.AppendLine(".status-cancelled { background: rgba(220,53,69,0.2); color: #dc3545; }");
            sb.AppendLine("@media print { body { background: white; } .container { box-shadow: none; padding: 0; } }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine("<div class='container'>");

            // Header
            sb.AppendLine("<h1>Orders Report</h1>");
            sb.AppendLine("<div class='subtitle'>");
            sb.AppendLine($"<strong>Period:</strong> {minDate:MMM dd, yyyy} - {maxDate:MMM dd, yyyy}<br>");
            sb.AppendLine($"<strong>Generated:</strong> {DateTime.Now:MMM dd, yyyy HH:mm}");
            sb.AppendLine("</div>");

            // Filter info
            sb.AppendLine($"<div class='filter-info'><strong>Filter:</strong> {filterInfo}</div>");

            // Summary Cards
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"<div class='card'><h3>Total Orders</h3><p class='value'>{totalOrders}</p></div>");
            sb.AppendLine($"<div class='card'><h3>Total Revenue</h3><p class='value'>${totalRevenue:N2}</p></div>");
            sb.AppendLine($"<div class='card green'><h3>Completed</h3><p class='value'>{completedOrders}</p></div>");
            sb.AppendLine($"<div class='card blue'><h3>Processing</h3><p class='value'>{processingOrders}</p></div>");
            sb.AppendLine($"<div class='card orange'><h3>Pending</h3><p class='value'>{pendingOrders}</p></div>");
            sb.AppendLine($"<div class='card red'><h3>Cancelled</h3><p class='value'>{cancelledOrders}</p></div>");
            sb.AppendLine("</div>");

            // Orders Table
            sb.AppendLine("<h2>Orders List</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th>Order #</th>");
            sb.AppendLine("<th>Date</th>");
            sb.AppendLine("<th>Customer</th>");
            sb.AppendLine("<th class='text-center'>Status</th>");
            sb.AppendLine("<th class='text-right'>Subtotal</th>");
            sb.AppendLine("<th class='text-right'>Discount</th>");
            sb.AppendLine("<th class='text-right'>Total</th>");
            sb.AppendLine("</tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var order in orders.OrderByDescending(o => o.CreatedAt))
            {
                var statusClass = order.Status switch
                {
                    OrderStatus.PENDING => "status-pending",
                    OrderStatus.PROCESSING => "status-processing",
                    OrderStatus.COMPLETED => "status-completed",
                    OrderStatus.CANCELLED => "status-cancelled",
                    _ => ""
                };

                var statusText = order.Status switch
                {
                    OrderStatus.PENDING => "Pending",
                    OrderStatus.PROCESSING => "Processing",
                    OrderStatus.COMPLETED => "Completed",
                    OrderStatus.CANCELLED => "Cancelled",
                    _ => order.Status.ToString()
                };

                var customerName = order.Customer?.Name ?? "Guest";

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td><strong>{order.OrderNumber}</strong></td>");
                sb.AppendLine($"<td>{order.CreatedAt:MMM dd, yyyy}</td>");
                sb.AppendLine($"<td>{customerName}</td>");
                sb.AppendLine($"<td class='text-center'><span class='status {statusClass}'>{statusText}</span></td>");
                sb.AppendLine($"<td class='text-right'>${order.Subtotal:N2}</td>");
                sb.AppendLine($"<td class='text-right'>-${order.DiscountAmount:N2}</td>");
                sb.AppendLine($"<td class='text-right'><strong>${order.Total:N2}</strong></td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            
            // Footer row with totals
            sb.AppendLine("<tfoot style='background: #f0f0f0; font-weight: bold;'>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td colspan='4'>Total ({totalOrders} orders)</td>");
            sb.AppendLine($"<td class='text-right'>${orders.Sum(o => o.Subtotal):N2}</td>");
            sb.AppendLine($"<td class='text-right'>-${orders.Sum(o => o.DiscountAmount):N2}</td>");
            sb.AppendLine($"<td class='text-right'>${totalRevenue:N2}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</tfoot>");
            sb.AppendLine("</table>");

            sb.AppendLine("</div>");

            // Auto-print when page loads
            sb.AppendLine("<script>");
            sb.AppendLine("window.onload = function() { window.print(); };");
            sb.AppendLine("</script>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }
    }
}
