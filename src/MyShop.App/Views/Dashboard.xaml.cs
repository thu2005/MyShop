using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.App.ViewModels;

namespace MyShop.App.Views;

/// <summary>
/// Dashboard page code-behind
/// </summary>
public sealed partial class Dashboard : Page
{
    public DashboardViewModel ViewModel { get; }

    public Dashboard()
    {
        this.InitializeComponent();

        // ViewModel will be injected via dependency injection
        ViewModel = App.Current.GetService<DashboardViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Load data when navigating to the page
        await ViewModel.LoadDashboardDataCommand.ExecuteAsync(null);
    }
}
