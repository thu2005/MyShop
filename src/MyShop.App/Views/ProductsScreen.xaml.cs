using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation; // Added for NavigationEventArgs
using MyShop.App.ViewModels;
using MyShop.App.Views.Dialogs;
using MyShop.Core.Interfaces.Services;
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
            
            // Disable page cache to ensure fresh data on navigation
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        // NEW: Handle incoming navigation parameter (CategoryId)
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Always reload products when navigating to this page (in case of edits/deletes from detail screen)
            await ViewModel.LoadProductsAsync();
            
            // Reload categories to update sidebar counts
            await ReloadShellCategories();

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
            // Check license before allowing product creation
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            if (!licenseService.IsFeatureAllowed("AddProduct"))
            {
                await ShowTrialExpiredDialog("Add Product");
                return;
            }
            
            Frame.Navigate(typeof(AddProductScreen));
        }

        private async System.Threading.Tasks.Task ShowTrialExpiredDialog(string featureName)
        {
            var shell = ShellPage.Instance; // Assuming we add a static Instance to ShellPage or find it
            if (shell != null)
            {
                await shell.ShowTrialExpiredDialog(featureName);
            }
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
            // Check license before allowing product editing
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            if (!licenseService.IsFeatureAllowed("EditProduct"))
            {
                await ShowTrialExpiredDialog("Edit Product");
                return;
            }

            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is Product productToEdit)
            {
                // Navigate to ProductDetailScreen with edit mode enabled
                Frame.Navigate(typeof(ProductDetailScreen), new { Product = productToEdit, StartInEditMode = true });
            }
        }

        private async void OnDeleteProductClick(object sender, RoutedEventArgs e)
        {
            // Check license before allowing product deletion
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            if (!licenseService.IsFeatureAllowed("DeleteProduct"))
            {
                await ShowTrialExpiredDialog("Delete Product");
                return;
            }

            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is Product productToDelete)
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
                    try
                    {
                        await ViewModel.DeleteProductAsync(productToDelete.Id);
                        
                        // Reload categories in ShellPage to update counts in sidebar
                        await ReloadShellCategories();
                    }
                    catch (Exception ex)
                    {
                        // Show error dialog
                        var errorDialog = new ContentDialog
                        {
                            Title = "Delete Failed",
                            Content = ex.Message,
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task ReloadShellCategories()
        {
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

        private void OnClearFiltersClick(object sender, RoutedEventArgs e)
        {
            // Clear UI controls
            SearchBox.Text = string.Empty;
            FromPriceBox.Text = string.Empty;
            ToPriceBox.Text = string.Empty;
            PrimarySortComboBox.SelectedIndex = -1;
            SecondarySortComboBox.SelectedIndex = -1;
            
            // Clear ViewModel filters
            ViewModel.ClearFilters();
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

        private async void OnImportClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Import button clicked");
                
                // Create file picker
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.FileTypeFilter.Add(".xlsx");
                picker.FileTypeFilter.Add(".xls");
                
                // Initialize with window handle
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                System.Diagnostics.Debug.WriteLine("File picker initialized");

                // Pick file
                var file = await picker.PickSingleFileAsync();
                if (file == null)
                {
                    System.Diagnostics.Debug.WriteLine("No file selected");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"File selected: {file.Path}");

                // Navigate to import screen with file path
                Frame.Navigate(typeof(ImportProductsScreen), file.Path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Import error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Import Error",
                    Content = $"Failed to open import screen:\n\n{ex.Message}\n\nCheck Debug Output for details.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void OnAddCategoryClick(object sender, RoutedEventArgs e)
        {
            // Check license before allowing category creation
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            if (!licenseService.IsFeatureAllowed("AddCategory"))
            {
                await ShowTrialExpiredDialog("Add Category");
                return;
            }

            var dialog = new Dialogs.AddCategoryDialog();
            dialog.XamlRoot = this.XamlRoot;

            bool success = false;
            while (!success)
            {
                var result = await dialog.ShowAsync();

                if (result != ContentDialogResult.Primary || dialog.NewCategory == null)
                {
                    break; // User cancelled
                }

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

                    success = true;
                }
                catch (Exception ex)
                {
                    // Show error in dialog
                    dialog.ErrorText.Text = ex.Message.Contains("already exists")
                        ? "Category name already exists. Please choose a different name."
                        : $"Error: {ex.Message}";
                    dialog.ErrorText.Visibility = Visibility.Visible;

                    // Reset for retry
                    dialog.NewCategory = null;
                    dialog.Hide();
                }
            }
        }

        private void OnPreviousPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToPreviousPage();
        }

        private void OnNextPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToNextPage();
        }

        private void OnFirstPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToFirstPage();
        }

        private void OnLastPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToLastPage();
        }
    }
}