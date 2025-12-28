using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.App.ViewModels
{
    public partial class ReportsViewModel
    {
        // Export functionality - ExportToPdfCommand is declared in main ReportsViewModel.cs

        private async Task ExportToPdfAsync()
        {
            try
            {
                IsBusy = true;

                // Get initial date range
                var startVal = StartDate ?? DateTimeOffset.Now.AddDays(-7);
                var endVal = EndDate ?? DateTimeOffset.Now;

                // Load summary report to get actual period range
                var report = await _reportRepository.GetReportByPeriodAsync(_selectedPeriod, startVal.DateTime, endVal.DateTime);
                
                // Calculate actual date range used by backend (same logic as LoadReportAsync)
                DateTime start = startVal.DateTime;
                DateTime end = endVal.DateTime;
                
                if (_selectedPeriod != PeriodType.CUSTOM && report != null)
                {
                    start = report.PeriodStart;
                    end = report.PeriodEnd;
                }

                // Determine timeline grouping based on selected period
                var timelineGrouping = TimelineGrouping.DAY;
                if (_selectedPeriod == PeriodType.YEARLY)
                {
                    timelineGrouping = TimelineGrouping.MONTH;
                }

                // Load fresh data for export using ACTUAL period range
                var products = await _reportRepository.GetTopProductsByQuantityAsync(start, end);
                var customers = await _reportRepository.GetTopCustomersAsync(start, end);
                var timeline = await _reportRepository.GetRevenueAndProfitTimelineAsync(start, end, timelineGrouping);

                List<StaffPerformanceData>? staff = null;
                if (IsAdmin)
                {
                    staff = await _reportRepository.GetAllStaffPerformanceAsync(start, end);
                }

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"[Export] Period: {_selectedPeriod}, Start: {start:yyyy-MM-dd}, End: {end:yyyy-MM-dd}");
                System.Diagnostics.Debug.WriteLine($"[Export] Products: {products.Count}, Customers: {customers.Count}, Timeline: {timeline.Count}");


                // Generate HTML report
                var html = GenerateHtmlReport(start, end, products, customers, staff, timeline);

                // Save to temp file
                var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                await System.IO.File.WriteAllTextAsync(tempPath, html);

                // Open in default browser - will show print dialog automatically
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);

                System.Diagnostics.Debug.WriteLine($"Report opened in browser: {tempPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating report: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GenerateHtmlReport(DateTime start, DateTime end, List<ProductSalesData> products,
            List<CustomerSalesData> customers, List<StaffPerformanceData>? staff, List<RevenueProfit> timeline)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<title>Sales Report</title>");

            sb.AppendLine("<style>");
            sb.AppendLine("@media print { @page { margin: 1cm; } .no-break { break-inside: avoid; } }");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; }");
            sb.AppendLine(".container { max-width: 1200px; margin: 0 auto; background: white; padding: 40px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("h1 { color: #003F62; margin: 0 0 10px 0; font-size: 32px; }");
            sb.AppendLine(".subtitle { color: #666; margin-bottom: 30px; font-size: 14px; }");
            sb.AppendLine("h2 { color: #003F62; margin: 40px 0 20px 0; padding-bottom: 10px; border-bottom: 2px solid #003F62; font-size: 20px; }");
            sb.AppendLine(".summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin: 30px 0; }");
            sb.AppendLine(".card { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 25px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }");
            sb.AppendLine(".card h3 { margin: 0 0 10px 0; font-size: 14px; opacity: 0.9; font-weight: normal; }");
            sb.AppendLine(".card .value { font-size: 32px; font-weight: bold; margin: 0; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            sb.AppendLine("thead { background: #003F62; color: white; }");
            sb.AppendLine("th { padding: 15px; text-align: left; font-weight: 600; }");
            sb.AppendLine("td { padding: 12px 15px; border-bottom: 1px solid #e0e0e0; }");
            sb.AppendLine("tbody tr:hover { background: #f9f9f9; }");
            sb.AppendLine("tbody tr:nth-child(even) { background: #fafafa; }");
            sb.AppendLine(".text-right { text-align: right; }");

            sb.AppendLine("@media print { body { background: white; } .container { box-shadow: none; padding: 0; } .chart-container { border: none; } }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine("<div class='container'>");

            // Header
            sb.AppendLine($"<h1>Sales Report</h1>");
            sb.AppendLine($"<div class='subtitle'>");
            sb.AppendLine($"<strong>Period:</strong> {SelectedPeriod} ({start:MMM dd, yyyy} - {end:MMM dd, yyyy})<br>");
            sb.AppendLine($"<strong>Generated:</strong> {DateTime.Now:MMM dd, yyyy HH:mm}");
            sb.AppendLine($"</div>");

            // Summary Cards
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"<div class='card'><h3>Products Sold</h3><p class='value'>{TotalProductsSold}</p></div>");
            sb.AppendLine($"<div class='card'><h3>Total Orders</h3><p class='value'>{TotalOrders}</p></div>");
            sb.AppendLine($"<div class='card'><h3>Total Revenue</h3><p class='value'>{TotalRevenue}</p></div>");
            if (IsAdmin && _reportData != null)
            {
                sb.AppendLine($"<div class='card'><h3>Total Profit</h3><p class='value'>${_reportData.TotalProfit:N2}</p></div>");
            }
            sb.AppendLine("</div>");
            


            // Top Products Table
            if (products.Any())
            {
                sb.AppendLine("<h2>Top Products Details</h2>");
                sb.AppendLine("<table>");
                sb.AppendLine("<thead><tr><th>Product</th><th class='text-right'>Quantity</th><th class='text-right'>Revenue</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var p in products.Take(5))
                {
                    sb.AppendLine($"<tr><td>{p.ProductName}</td><td class='text-right'>{p.QuantitySold}</td><td class='text-right'>${p.Revenue:N2}</td></tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            // Top Customers Table
            if (customers.Any())
            {
                sb.AppendLine("<h2>Top Customers</h2>");
                sb.AppendLine("<table>");
                sb.AppendLine("<thead><tr><th>Customer</th><th class='text-right'>Orders</th><th class='text-right'>Total Spent</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var c in customers.Take(5))
                {
                    sb.AppendLine($"<tr><td>{c.CustomerName}</td><td class='text-right'>{c.TotalOrders}</td><td class='text-right'>${c.TotalSpent:N2}</td></tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            // Staff Performance (Admin only)
            if (IsAdmin && staff != null && staff.Any())
            {
                sb.AppendLine("<h2>Staff Performance</h2>");
                sb.AppendLine("<table>");
                sb.AppendLine("<thead><tr><th>ID</th><th>Staff</th><th>Email</th><th class='text-right'>Orders</th><th class='text-right'>Revenue</th><th class='text-right'>Profit</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var s in staff)
                {
                    sb.AppendLine($"<tr><td>{s.StaffId}</td><td>{s.Username}</td><td>{s.Email}</td><td class='text-right'>{s.TotalOrders}</td><td class='text-right'>${s.TotalRevenue:N2}</td><td class='text-right'>${s.TotalProfit:N2}</td></tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            // Timeline Table
            if (timeline.Any())
            {
                sb.AppendLine("<h2>Detailed Timeline</h2>");
                sb.AppendLine("<table>");
                
                // Header with conditional Profit column for admin
                if (IsAdmin)
                {
                    sb.AppendLine("<thead><tr><th>Date</th><th class='text-right'>Orders</th><th class='text-right'>Revenue</th><th class='text-right'>Profit</th></tr></thead>");
                }
                else
                {
                    sb.AppendLine("<thead><tr><th>Date</th><th class='text-right'>Orders</th><th class='text-right'>Revenue</th></tr></thead>");
                }
                
                sb.AppendLine("<tbody>");
                foreach (var t in timeline)
                {
                    // Data rows with conditional Profit column for admin
                    if (IsAdmin)
                    {
                        sb.AppendLine($"<tr><td>{t.Date}</td><td class='text-right'>{t.Orders}</td><td class='text-right'>${t.Revenue:N2}</td><td class='text-right'>${t.Profit:N2}</td></tr>");
                    }
                    else
                    {
                        sb.AppendLine($"<tr><td>{t.Date}</td><td class='text-right'>{t.Orders}</td><td class='text-right'>${t.Revenue:N2}</td></tr>");
                    }
                }
                sb.AppendLine("</tbody></table>");
            }

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
