using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using MyShop.App.ViewModels;
using MyShop.Core.Models;
using Windows.Storage.Pickers;
using System;

namespace MyShop.App.Views
{
    public sealed partial class AddProductScreen : Page
    {
        public AddProductViewModel ViewModel { get; }
        private string _selectedImagePath;

        public AddProductScreen()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<AddProductViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadCategoriesAsync();
        }

        private async void OnUploadImageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                // Get window handle for WinUI 3
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    // Show preview
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(await file.OpenAsync(Windows.Storage.FileAccessMode.Read));
                    PreviewImage.Source = bitmap;
                    PreviewImage.Visibility = Visibility.Visible;
                    PlaceholderIcon.Visibility = Visibility.Collapsed;

                    // Store file for upload
                    _selectedImagePath = file.Path;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image selection failed: {ex.Message}");
            }
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(SkuBox.Text))
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Product Name and SKU are required fields.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                    return;
                }

                if (CategoryComboBox.SelectedValue == null)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Please select a category.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                    return;
                }

                // Parse numeric values
                int stock = 0;
                decimal? costPrice = null;
                decimal price = 0;

                if (!string.IsNullOrWhiteSpace(StockBox.Text))
                    int.TryParse(StockBox.Text, out stock);

                if (!string.IsNullOrWhiteSpace(CostPriceBox.Text))
                {
                    if (decimal.TryParse(CostPriceBox.Text, out decimal parsedCostPrice))
                        costPrice = parsedCostPrice;
                }

                if (!string.IsNullOrWhiteSpace(PriceBox.Text))
                    decimal.TryParse(PriceBox.Text, out price);

                // Create product
                var newProduct = new Product
                {
                    Name = NameBox.Text,
                    Description = DescriptionBox.Text,
                    Sku = SkuBox.Text,
                    Stock = stock,
                    CostPrice = costPrice,
                    Price = price,
                    CategoryId = (int)CategoryComboBox.SelectedValue,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Upload image if selected
                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(_selectedImagePath);
                    var imageUrl = await ViewModel.UploadImageAsync(file);
                    newProduct.ImageUrl = imageUrl;
                }

                // Save product
                await ViewModel.AddProductAsync(newProduct);

                // Navigate back
                Frame.GoBack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
                
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to save product: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
