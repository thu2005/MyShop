using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;
using System;

namespace MyShop.App.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; }

        public ShellPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ShellViewModel>();
            ViewModel.LogoutRequested += OnLogoutRequested;
        }

        private void OnManageProductsClick(object sender, RoutedEventArgs e)
        {
            // Navigate the current Frame to your new ProductsScreen
            this.Frame.Navigate(typeof(ProductsScreen));
        }

        private void OnLogoutRequested()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(LoginScreen));
            }
        }
    }
}
