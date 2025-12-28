using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.App.ViewModels;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.App.Views;

/// <summary>
/// Dashboard page code-behind
/// </summary>
public sealed partial class Dashboard : Page
{
    public DashboardViewModel ViewModel { get; }
    
    // Static property to preserve period selection across navigation
    private static string LastSelectedPeriod = "WEEKLY";

    public Dashboard()
    {
        this.InitializeComponent();

        // ViewModel will be injected via dependency injection
        ViewModel = App.Current.GetService<DashboardViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Restore last selected period BEFORE loading data
        ViewModel.SelectedPeriod = LastSelectedPeriod;
        
        // Load data when navigating to the page
        await ViewModel.LoadDashboardDataCommand.ExecuteAsync(null);
        
        // Subscribe to period changes to save them
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedPeriod))
        {
            LastSelectedPeriod = ViewModel.SelectedPeriod;
        }
    }

    private void OnReportClick(object sender, RoutedEventArgs e)
    {
        // Navigate to Reports page
        Frame.Navigate(typeof(ReportsPage));
        
        // Update sidebar selection
        var shellPage = FindShellPage();
        if (shellPage != null)
        {
            // Find the REPORT menu item and select it
            var reportItem = shellPage.NavView.MenuItems
                .OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == "Reports");
            
            if (reportItem != null)
            {
                shellPage.NavView.SelectedItem = reportItem;
            }
        }
    }

    private async void OnBestSellerItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is MyShop.Core.Models.DTOs.TopProductDto topProduct)
        {
            await NavigateToProductDetail(topProduct.Product.Id);
        }
    }

    private async void OnLowStockItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is MyShop.Core.Models.DTOs.LowStockProductDto lowStockProduct)
        {
            await NavigateToProductDetail(lowStockProduct.Id);
        }
    }

    private async void OnRecentOrderClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is MyShop.Core.Models.DTOs.OrderDto orderDto)
        {
            try
            {
                // Get order repository
                var orderRepo = App.Current.Services.GetRequiredService<MyShop.Core.Interfaces.Repositories.IOrderRepository>();
                
                var order = await orderRepo.GetByIdAsync(orderDto.Id);
                
                if (order == null)
                {
                    return;
                }

                var orderViewModel = App.Current.Services.GetRequiredService<OrderViewModel>();
                var navParams = new CreateOrderPageNavigationParams 
                { 
                    ViewModel = orderViewModel,
                    OrderIdToView = order.Id,
                    IsReadOnly = true
                };
                
                Frame.Navigate(typeof(CreateOrderPage), navParams);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to navigate to order detail: {ex.Message}");
            }
        }
    }

    private async System.Threading.Tasks.Task NavigateToProductDetail(int productId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"NavigateToProductDetail called with productId: {productId}");
            
            // Get product repository
            var productRepo = App.Current.Services.GetRequiredService<MyShop.Core.Interfaces.Repositories.IProductRepository>();

            // Load full product
            System.Diagnostics.Debug.WriteLine($"Loading product {productId}...");
            var product = await productRepo.GetByIdAsync(productId);
            
            if (product == null)
            {
                System.Diagnostics.Debug.WriteLine($"Product {productId} not found!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Product loaded: {product.Name}, navigating to ProductDetailScreen...");
            
            // Update sidebar BEFORE navigation to prevent navigation event from overriding
            var shellPage = FindShellPage();
            if (shellPage != null)
            {
                shellPage.SetSidebarSelectionWithoutNavigation("Products_0");
            }
            
            // Navigate to ProductDetailScreen AFTER sidebar update
            Frame.Navigate(typeof(ProductDetailScreen), product);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to navigate to product detail: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private ShellPage FindShellPage()
    {
        var frame = this.Frame;
        while (frame != null)
        {
            if (frame.Content is ShellPage shellPage)
            {
                return shellPage;
            }
            
            var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(frame);
            frame = null;
            while (parent != null)
            {
                if (parent is Microsoft.UI.Xaml.Controls.Frame parentFrame)
                {
                    frame = parentFrame;
                    break;
                }
                parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
            }
        }
        return null;
    }
}
