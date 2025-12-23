using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyShop.App.ViewModels;

namespace MyShop.App.Views
{
    public sealed partial class ProductsScreen : Page
    {
        public ProductViewModel ViewModel { get; }

        public ProductsScreen()
        {
            this.InitializeComponent();

            // Get the ViewModel (with Repository injected) from App Services
            ViewModel = App.Current.Services.GetService<ProductViewModel>();
        }
    }
}