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

        private void OnAddProductClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddProductScreen));
        }

        private async void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            await ViewModel.SearchProductsAsync(args.QueryText);
        }

        private async void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Trigger search on every text change for real-time filtering
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await ViewModel.SearchProductsAsync(sender.Text);
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

        private void OnProductCardClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                Frame.Navigate(typeof(ProductDetailScreen), product);
            }
        }

        private void OnPriceRangeChanged(object sender, TextChangedEventArgs e)
        {
            // Parse price values
            decimal? minPrice = null;
            decimal? maxPrice = null;

            if (!string.IsNullOrWhiteSpace(FromPriceBox.Text))
            {
                if (decimal.TryParse(FromPriceBox.Text, out decimal min))
                    minPrice = min;
            }

            if (!string.IsNullOrWhiteSpace(ToPriceBox.Text))
            {
                if (decimal.TryParse(ToPriceBox.Text, out decimal max))
                    maxPrice = max;
            }

            ViewModel.SetPriceRange(minPrice, maxPrice);
        }

        private void OnPrimarySortChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PrimarySortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var primaryTag = selectedItem.Tag?.ToString();
                
                // Enable secondary ComboBox and populate with remaining options
                SecondarySortComboBox.IsEnabled = true;
                SecondarySortComboBox.Items.Clear();
                
                // Add opposite options based on primary selection
                if (primaryTag?.StartsWith("Price") == true)
                {
                    // Primary is Price, so secondary shows Stock options
                    SecondarySortComboBox.Items.Add(new ComboBoxItem { Content = "Stock (Low to High)", Tag = "StockAsc" });
                    SecondarySortComboBox.Items.Add(new ComboBoxItem { Content = "Stock (High to Low)", Tag = "StockDesc" });
                }
                else if (primaryTag?.StartsWith("Stock") == true)
                {
                    // Primary is Stock, so secondary shows Price options
                    SecondarySortComboBox.Items.Add(new ComboBoxItem { Content = "Price (Low to High)", Tag = "PriceAsc" });
                    SecondarySortComboBox.Items.Add(new ComboBoxItem { Content = "Price (High to Low)", Tag = "PriceDesc" });
                }
                
                // Reset secondary selection
                SecondarySortComboBox.SelectedIndex = -1;
                
                // Apply primary sort only
                ViewModel.SetSorting(primaryTag, null);
            }
            else
            {
                // No selection, disable secondary
                SecondarySortComboBox.IsEnabled = false;
                SecondarySortComboBox.Items.Clear();
                ViewModel.SetSorting(null, null);
            }
        }

        private void OnSecondarySortChanged(object sender, SelectionChangedEventArgs e)
        {
            var primaryTag = (PrimarySortComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            var secondaryTag = (SecondarySortComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            
            ViewModel.SetSorting(primaryTag, secondaryTag);
        }

        private async void OnAddCategoryClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.AddCategoryDialog();
            dialog.XamlRoot = this.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.NewCategory != null)
            {
                try
                {
                    // Get category repository
                    var categoryRepo = App.Current.Services.GetService<MyShop.Core.Interfaces.Repositories.ICategoryRepository>();
                    await categoryRepo.AddAsync(dialog.NewCategory);

                    // Find the ShellPage in the navigation stack and reload its categories
                    var frame = this.Frame;
                    while (frame != null)
                    {
                        if (frame.Content is ShellPage shellPage)
                        {
                            await shellPage.ViewModel.LoadCategoriesAsync();
                            break;
                        }
                        // Try to get parent frame
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

                    // // Show success message
                    // var successDialog = new ContentDialog
                    // {
                    //     Title = "Success",
                    //     Content = $"Category '{dialog.NewCategory.Name}' has been added successfully.",
                    //     CloseButtonText = "OK",
                    //     XamlRoot = this.XamlRoot
                    // };
                    // await successDialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to add category: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}