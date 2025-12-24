using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation; // Added for NavigationEventArgs
using MyShop.App.ViewModels;
using MyShop.App.Views.Dialogs;
using MyShop.Core.Models;
using System;

namespace MyShop.App.Views
{
    public sealed partial class ProductsScreen : Page
    {
        public ProductViewModel ViewModel { get; }

        public ProductsScreen()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<ProductViewModel>();
        }

        // NEW: Handle incoming navigation parameter (CategoryId)
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Ensure products are loaded if this is the first visit
            if (ViewModel.Products.Count == 0)
            {
                await ViewModel.LoadProductsAsync();
            }

            // Check if a specific Category ID was passed
            if (e.Parameter is int categoryId)
            {
                ViewModel.SelectCategoryById(categoryId);
            }
            else
            {
                // If no parameter (or 0), show all products
                ViewModel.SelectCategoryById(0);
            }
        }

        private async void OnAddProductClick(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProductDialog();
            dialog.XamlRoot = this.XamlRoot;

            await dialog.ShowAsync();

            if (dialog.NewProduct != null)
            {
                await ViewModel.AddProductAsync(dialog.NewProduct);
            }
        }

        private async void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            await ViewModel.SearchProductsAsync(args.QueryText);
        }

        private async void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && string.IsNullOrWhiteSpace(sender.Text))
            {
                await ViewModel.SearchProductsAsync(string.Empty);
            }
        }

        private async void OnEditProductClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product productToEdit)
            {
                var dialog = new EditProductDialog(productToEdit);
                dialog.XamlRoot = this.XamlRoot;

                await dialog.ShowAsync();

                if (dialog.UpdatedProduct != null)
                {
                    await ViewModel.UpdateProductAsync(dialog.UpdatedProduct);
                }
            }
        }

        private async void OnDeleteProductClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product productToDelete)
            {
                var dialog = new ContentDialog
                {
                    Title = "Delete Product",
                    Content = $"Are you sure you want to permanently delete '{productToDelete.Name}'?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var primaryButtonStyle = new Style(typeof(Button));
                primaryButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red)));
                primaryButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)));
                dialog.PrimaryButtonStyle = primaryButtonStyle;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.DeleteProductAsync(productToDelete.Id);
                }
            }
        }
    }
}