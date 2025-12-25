
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models.DTOs;
using MyShop.Core.Services;
using System.Threading; // Add this using directive
using System.Threading.Tasks;
using System.Linq; // Add this using directive
namespace MyShop.App.ViewModels;
using System.Collections.Generic;
using System;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;

public partial class DashboardViewModel : ObservableObject
{
    public string CurrentDateString => DateTime.Now.ToString("MMM d, yyyy");

    private readonly IDashboardService _dashboardService;

    [ObservableProperty]
    private DashboardStatsDto? _stats;

    [ObservableProperty]
    private List<TopProductDto> _topProducts = new();

    [ObservableProperty]
    private List<RevenueByDateDto> _revenueData = new();

    [ObservableProperty]
    private List<OrderDto> _recentOrders = new();

    [ObservableProperty]
    private List<LowStockProductDto> _lowStockProducts = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    // Chart properties
    [ObservableProperty]
    private ISeries[] _chartSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _chartXAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _chartYAxes = Array.Empty<ICartesianAxis>();

    [ObservableProperty]
    private string _selectedPeriod = "WEEKLY";

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        InitializeChart();
    }

    private void InitializeChart()
    {
        // Initialize empty chart axes
        ChartXAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };

        ChartYAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12,
                MinLimit = 0
            }
        };
    }

    /// <summary>
    /// Load all dashboard data
    /// </summary>
    [RelayCommand]
    private async Task LoadDashboardDataAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            System.Diagnostics.Debug.WriteLine("Loading dashboard stats...");
            // Load dashboard stats
            Stats = await _dashboardService.GetDashboardStatsAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"Dashboard stats loaded: TotalProducts={Stats?.TotalProducts}, TotalOrders={Stats?.TotalOrders}");

            // Load sales report based on selected period
            await LoadSalesReportAsync(cancellationToken);

            // Load recent orders
            System.Diagnostics.Debug.WriteLine("Loading recent orders...");
            RecentOrders = await _dashboardService.GetRecentOrdersAsync(5, cancellationToken);
            System.Diagnostics.Debug.WriteLine($"Recent orders loaded: Count={RecentOrders?.Count}");

            // Load low stock products
            System.Diagnostics.Debug.WriteLine("Loading low stock products...");
            LowStockProducts = await _dashboardService.GetLowStockProductsAsync(10, cancellationToken);
            System.Diagnostics.Debug.WriteLine($"Low stock products loaded: Count={LowStockProducts?.Count}");
        }
        catch (OperationCanceledException)
        {
            // User cancelled the operation, ignore
        }
        catch (GraphQLException ex)
        {
            ErrorMessage = $"Failed to load dashboard data: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Load sales report and update chart based on selected period
    /// </summary>
    private async Task LoadSalesReportAsync(CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine($"Loading sales report for period: {SelectedPeriod}...");
        
        var (startDate, endDate) = GetDateRangeForPeriod(SelectedPeriod);
        
        var report = await _dashboardService.GetSalesReportAsync(
            startDate,
            endDate,
            cancellationToken);
        
        System.Diagnostics.Debug.WriteLine($"Sales report loaded: TopProducts={report?.TopProducts?.Count}, RevenueByDate={report?.RevenueByDate?.Count}");
        
        TopProducts = report.TopProducts;
        RevenueData = report.RevenueByDate;
        
        // Update chart with new data
        UpdateChart(report.RevenueByDate);
    }

    /// <summary>
    /// Get date range based on selected period
    /// </summary>
    private (DateTime startDate, DateTime endDate) GetDateRangeForPeriod(string period)
    {
        var endDate = DateTime.Now;
        DateTime startDate;

        switch (period)
        {
            case "WEEKLY":
                // Current week (Monday to Sunday)
                var dayOfWeek = (int)endDate.DayOfWeek;
                var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // Sunday = 0
                startDate = endDate.AddDays(-daysToMonday).Date;
                break;
            
            case "MONTHLY":
                // Current month
                startDate = new DateTime(endDate.Year, endDate.Month, 1);
                break;
            
            case "YEARLY":
                // Current year
                startDate = new DateTime(endDate.Year, 1, 1);
                break;
            
            default:
                // Default to weekly
                dayOfWeek = (int)endDate.DayOfWeek;
                daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                startDate = endDate.AddDays(-daysToMonday).Date;
                break;
        }

        return (startDate, endDate);
    }

    /// <summary>
    /// Update chart with revenue data
    /// </summary>
    private void UpdateChart(List<RevenueByDateDto> revenueData)
    {
        if (revenueData == null || revenueData.Count == 0)
        {
            ChartSeries = Array.Empty<ISeries>();
            return;
        }

        // Prepare data for chart
        var values = revenueData.Select(r => (double)r.Revenue).ToArray();
        var labels = revenueData.Select(r => 
        {
            var date = DateTime.Parse(r.Date);
            return SelectedPeriod switch
            {
                "WEEKLY" => date.ToString("ddd"),  // Mon, Tue, Wed...
                "MONTHLY" => date.ToString("MMM dd"),  // Jan 01, Jan 02...
                "YEARLY" => date.ToString("MMM"),  // Jan, Feb, Mar...
                _ => date.ToString("MMM dd")
            };
        }).ToArray();

        // Create line series
        ChartSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values,
                Name = "Revenue",
                Fill = null,
                Stroke = new SolidColorPaint(new SKColor(0x00, 0x3F, 0x62)) { StrokeThickness = 3 },
                GeometrySize = 10,
                GeometryFill = new SolidColorPaint(new SKColor(0x00, 0x3F, 0x62)),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 }
            }
        };

        // Update X axis with labels
        ChartXAxes = new Axis[]
        {
            new Axis
            {
                Name="",
                NamePaint = new SolidColorPaint(SKColors.Gray),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12,
                Labels = labels
            }
        };
    }

    /// <summary>
    /// Change period and reload data
    /// </summary>
    [RelayCommand]
    private async Task ChangePeriodAsync(string period)
    {
        if (SelectedPeriod == period)
            return;

        SelectedPeriod = period;
        
        try
        {
            await LoadSalesReportAsync(CancellationToken.None);
        }
        catch (GraphQLException ex)
        {
            ErrorMessage = $"Failed to load sales data: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error: {ex.Message}";
        }
    }

    /// <summary>
    /// Refresh dashboard data
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDashboardDataAsync(CancellationToken.None);
    }
}
