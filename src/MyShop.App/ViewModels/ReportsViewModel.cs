using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches; // Add this using
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.App.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        public string CurrentDateString => DateTime.Now.ToString("MMM d, yyyy");

        private readonly IReportRepository _reportRepository;
        private readonly IAuthService _authService;
        private readonly IAuthorizationService _authorizationService;
        
        private PeriodType _selectedPeriod = PeriodType.WEEKLY;
        private DateTimeOffset? _startDate = DateTimeOffset.Now.AddDays(-7);
        private DateTimeOffset? _endDate = DateTimeOffset.Now;
        private PeriodReport? _reportData;
        
        // Summary Cards
        private string _totalProductsSold = "0";
        private string _totalOrders = "0";
        private string _totalRevenue = "0";
        private double _productsChange;
        private double _ordersChange;
        private double _revenueChange;

        // Charts
        private ObservableCollection<CustomerSalesData> _topCustomers = new();

        [ObservableProperty]
        private bool _isBusy;

        // Commands
        public IAsyncRelayCommand LoadReportCommand { get; }
        public IAsyncRelayCommand ExportToPdfCommand { get; }

        // Role-based properties
        public User? CurrentUser => _authService.CurrentUser;
        public UserRole UserRole => CurrentUser?.Role ?? UserRole.STAFF;
        public bool IsAdmin => _authorizationService.IsAuthorized(UserRole.ADMIN);
        public bool IsStaff => !IsAdmin;

        public ReportsViewModel(
            IReportRepository reportRepository,
            IAuthService authService,
            IAuthorizationService authorizationService)
        {
            _reportRepository = reportRepository;
            _authService = authService;
            _authorizationService = authorizationService;
            
            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
            ExportToPdfCommand = new AsyncRelayCommand(ExportToPdfAsync);
            
            // Initialize default dates
            _endDate = DateTimeOffset.Now;
            _startDate = DateTimeOffset.Now.AddDays(-6); // Last 7 days including today
            
            // Initialize charts
            InitializeCharts();
            
            // Initial Load
            _ = LoadReportAsync();
        }

        private void InitializeCharts()
        {
            // Initialize empty chart axes to prevent crashes
            ProductsXAxes = new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 12
                }
            };

            ProductsYAxes = new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 12
                }
            };

            RevenueProfitXAxes = new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 12
                }
            };
        }

        // Properties
        public PeriodType SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    OnPeriodChanged();
                    OnPropertyChanged(nameof(IsComparisonVisible));
                    _ = LoadReportAsync();
                }
            }
        }

        public DateTimeOffset? StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    if (_selectedPeriod == PeriodType.CUSTOM) _ = LoadReportAsync();
                }
            }
        }

        public DateTimeOffset? EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    if (_selectedPeriod == PeriodType.CUSTOM) _ = LoadReportAsync();
                }
            }
        }

        public string TotalProductsSold { get => _totalProductsSold; set => SetProperty(ref _totalProductsSold, value); }
        public string TotalOrders { get => _totalOrders; set => SetProperty(ref _totalOrders, value); }
        public string TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }

        public double ProductsChange { get => _productsChange; set => SetProperty(ref _productsChange, value); }
        public double OrdersChange { get => _ordersChange; set => SetProperty(ref _ordersChange, value); }
        public double RevenueChange { get => _revenueChange; set => SetProperty(ref _revenueChange, value); }

        [ObservableProperty]
        private IEnumerable<ISeries> _productsSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private IEnumerable<ICartesianAxis> _productsXAxes = Array.Empty<ICartesianAxis>();

        [ObservableProperty]
        private IEnumerable<ICartesianAxis> _productsYAxes = Array.Empty<ICartesianAxis>();

        [ObservableProperty]
        private double _productsChartMinWidth = 600;

        [ObservableProperty]
        private IEnumerable<ISeries> _revenueProfitSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private IEnumerable<ICartesianAxis> _revenueProfitXAxes = Array.Empty<ICartesianAxis>();

        public ObservableCollection<CustomerSalesData> TopCustomers => _topCustomers;
        
        private ObservableCollection<StaffPerformanceData> _allStaff = new();
        public ObservableCollection<StaffPerformanceData> AllStaff => _allStaff;

        public ObservableCollection<PeriodType> Periods { get; } = new ObservableCollection<PeriodType>(Enum.GetValues(typeof(PeriodType)).Cast<PeriodType>());

        public bool IsCustomPeriod => SelectedPeriod == PeriodType.CUSTOM;
        public bool IsComparisonVisible => !IsCustomPeriod;

        // Helper properties for UI
        public string ProductsChangeText => $"{Math.Abs(ProductsChange):N1}%";
        public string OrdersChangeText => $"{Math.Abs(OrdersChange):N1}%";
        public string RevenueChangeText => $"{Math.Abs(RevenueChange):N1}%";

        // Up arrow: E70D (ChevronUp? No, let's check Segoe MDL2 Assets)
        // Up: E96C (UpArrowShiftKey), E74A (Up)
        // Down: E74B
        // Using common arrows: Up = \uE96D (CaretSolidUp), Down = \uE96E (CaretSolidDown) or E70E/E70D
        // Let's use generic arrows. Up: \uE70E, Down: \uE70D (These are chevrons)
        // Better: UpArrow = \uE74A, DownArrow = \uE74B
        
        public string ProductsChangeIcon => ProductsChange >= 0 ? "\uE74A" : "\uE74B";
        public string OrdersChangeIcon => OrdersChange >= 0 ? "\uE74A" : "\uE74B";
        public string RevenueChangeIcon => RevenueChange >= 0 ? "\uE74A" : "\uE74B";

        public SolidColorBrush ProductsChangeColor => new SolidColorBrush(ProductsChange >= 0 ? Colors.Green : Colors.Red);
        public SolidColorBrush OrdersChangeColor => new SolidColorBrush(OrdersChange >= 0 ? Colors.Green : Colors.Red);
        public SolidColorBrush RevenueChangeColor => new SolidColorBrush(RevenueChange >= 0 ? Colors.Green : Colors.Red);



        private void OnPeriodChanged()
        {
            OnPropertyChanged(nameof(IsCustomPeriod));
            
            var now = DateTime.Now;
            switch (_selectedPeriod)
            {
                case PeriodType.WEEKLY:
                    // Handled by backend logic, UI dates just for display if needed
                    break;
                case PeriodType.MONTHLY:
                    break;
                case PeriodType.YEARLY:
                    break;
                case PeriodType.CUSTOM:
                    // Keep current selection
                    break;
            }
        }

        private async Task LoadReportAsync()
        {
            try
            {
                IsBusy = true;

                // 1. Load Summary Report
                var startVal = _startDate ?? DateTime.Now.AddDays(-7);
                var endVal = _endDate ?? DateTime.Now;
                var report = await _reportRepository.GetReportByPeriodAsync(_selectedPeriod, startVal.DateTime, endVal.DateTime);
                if (report != null)
                {
                    _reportData = report;
                    TotalProductsSold = report.TotalProductsSold.ToString("N0");
                    TotalOrders = report.TotalOrders.ToString("N0");
                    TotalRevenue = report.TotalRevenue.ToString("N0"); // Remove currency symbol

                    ProductsChange = report.ProductsChange ?? 0;
                    OrdersChange = report.OrdersChange ?? 0;
                    RevenueChange = report.RevenueChange ?? 0;

                    OnPropertyChanged(nameof(ProductsChangeText));
                    OnPropertyChanged(nameof(OrdersChangeText));
                    OnPropertyChanged(nameof(RevenueChangeText));
                    OnPropertyChanged(nameof(ProductsChangeIcon));
                    OnPropertyChanged(nameof(OrdersChangeIcon));
                    OnPropertyChanged(nameof(RevenueChangeIcon));
                    OnPropertyChanged(nameof(ProductsChangeColor));
                    OnPropertyChanged(nameof(OrdersChangeColor));
                    OnPropertyChanged(nameof(RevenueChangeColor));
                }

                // Calculate actual date range used by backend for charts
                DateTime start = startVal.DateTime;
                DateTime end = endVal.DateTime;
                
                if (_selectedPeriod != PeriodType.CUSTOM && report != null)
                {
                    start = report.PeriodStart;
                    end = report.PeriodEnd;
                }

                // 2. Load Top Products Chart (Row Chart: Y=Product Name, X=Quantity)
                var topProducts = await _reportRepository.GetTopProductsByQuantityAsync(start, end);
                SetupProductsChart(topProducts);

                // 3. Load Top Customers List
                var customers = await _reportRepository.GetTopCustomersAsync(start, end);
                _topCustomers.Clear();
                foreach (var c in customers) _topCustomers.Add(c);
                
                // 4. Load All Staff Performance
                try
                {
                    var staff = await _reportRepository.GetAllStaffPerformanceAsync(start, end);
                    
                    _allStaff.Clear();
                    foreach (var s in staff)
                    {
                        _allStaff.Add(s);
                    }
                }
                catch (Exception ex)
                {
                }

                // 5. Load Revenue/Profit Timeline Chart (Column Chart)
                var timelineGrouping = TimelineGrouping.DAY;
                if (_selectedPeriod == PeriodType.YEARLY) timelineGrouping = TimelineGrouping.MONTH;
                
                var timeline = await _reportRepository.GetRevenueAndProfitTimelineAsync(start, end, timelineGrouping);
                SetupRevenueProfitChart(timeline);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading report: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void SetupProductsChart(List<ProductSalesData> products)
        {
            var quantityValues = products.Select(p => (double)p.QuantitySold).ToArray();
            
            // Use FULL labels for the tooltip
            var productNames = products.Select(p => p.ProductName).ToArray();

            ProductsSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Quantity Sold",
                    Values = quantityValues,
                    Fill = null,
                    Stroke = new SolidColorPaint(new SKColor(0, 63, 98)) { StrokeThickness = 3 }, // #003F62
                    GeometrySize = 10,
                    GeometryFill = new SolidColorPaint(new SKColor(0, 63, 98)),
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 }
                }
            };

            ProductsYAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N0")
                }
            };

            ProductsXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = productNames,
                    // Use Labeler to truncate text on the Axis only
                    Labeler = value => 
                    {
                        int index = (int)value;
                        if (index >= 0 && index < productNames.Length)
                        {
                            var name = productNames[index];
                            var firstWord = name.Split(' ').FirstOrDefault() ?? name;
                            if (name.Contains(' ')) return firstWord + "...";
                            if (firstWord.Length > 10) return firstWord.Substring(0, 8) + "...";
                            return firstWord;
                        }
                        return "";
                    },
                    LabelsRotation = -15,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 10,
                    MinStep = 1,
                    ForceStepToMin = true,
                    Padding = new LiveChartsCore.Drawing.Padding(0, 15, 0, 0),
                    // Set max width to force wrapping
                    MaxLimit = null,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200))
                    {
                        StrokeThickness = 1
                    }
                }
            };
            
            // Calculate MinWidth: 80px per product, minimum 600px
            int productCount = products.Count;
            ProductsChartMinWidth = Math.Max(600, productCount * 80);
        }


        private void SetupRevenueProfitChart(List<RevenueProfit> timeline)
        {
            var revenueValues = timeline.Select(t => (double)t.Revenue).ToArray();
            var profitValues = timeline.Select(t => (double)t.Profit).ToArray();
            var labels = timeline.Select(t => t.Date).ToArray();

            // Build series list based on user role
            var seriesList = new List<ISeries>
            {
                new ColumnSeries<double>
                {
                    Name = "Revenue",
                    Values = revenueValues,
                    Fill = new SolidColorPaint(new SKColor(150, 200, 215)), // PowderBlue
                    Stroke = null,
                    MaxBarWidth = 30 
                }
            };

            // Only show Profit for ADMIN users
            if (IsAdmin)
            {
                seriesList.Add(new ColumnSeries<double>
                {
                    Name = "Profit",
                    Values = profitValues,
                    Fill = new SolidColorPaint(new SKColor(0, 64, 96)), // Dark Blue
                    Stroke = null,
                    MaxBarWidth = 30 
                });
            }

            RevenueProfitSeries = seriesList.ToArray();

            RevenueProfitXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0,
                    TextSize = 10
                }
            };
        }
    }
}
