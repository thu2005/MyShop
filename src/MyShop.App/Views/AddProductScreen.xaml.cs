using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using MyShop.App.ViewModels;
using MyShop.App.Services;
using MyShop.App.Models;
using MyShop.Core.Models;
using Windows.Storage.Pickers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyShop.App.Views
{
    public sealed partial class AddProductScreen : Page
    {
        private const string DRAFT_KEY = "draft_add_product";
        private const int DRAFT_EXPIRATION_DAYS = 7;
        private const int AUTO_SAVE_DELAY_MS = 500;

        public AddProductViewModel ViewModel { get; }
        private readonly IDraftService _draftService;
        private string _selectedImagePath;
        private CancellationTokenSource _autoSaveCts;

        public AddProductScreen()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<AddProductViewModel>();
            _draftService = App.Current.Services.GetService<IDraftService>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadCategoriesAsync();
            
            // Check for draft after categories are loaded
            CheckForDraft();
            
            // Attach TextChanged events for autosave
            AttachAutosaveEvents();
        }

        private void AttachAutosaveEvents()
        {
            NameBox.TextChanged += OnFieldChanged;
            DescriptionBox.TextChanged += OnFieldChanged;
            SkuBox.TextChanged += OnFieldChanged;
            StockBox.TextChanged += OnFieldChanged;
            CostPriceBox.TextChanged += OnFieldChanged;
            PriceBox.TextChanged += OnFieldChanged;
            CategoryComboBox.SelectionChanged += (s, e) => OnFieldChanged(s, null);
        }

        private void OnFieldChanged(object sender, object e)
        {
            // Cancel previous auto-save
            _autoSaveCts?.Cancel();
            _autoSaveCts = new CancellationTokenSource();

            // Debounced auto-save
            _ = AutoSaveDraftAsync(_autoSaveCts.Token);
        }

        private async Task AutoSaveDraftAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(AUTO_SAVE_DELAY_MS, cancellationToken);

                var draft = new ProductDraft
                {
                    Name = NameBox.Text,
                    Sku = SkuBox.Text,
                    Description = DescriptionBox.Text,
                    Price = PriceBox.Text,
                    CostPrice = CostPriceBox.Text,
                    Stock = StockBox.Text,
                    ImageUrl = _selectedImagePath,
                    CategoryId = CategoryComboBox.SelectedValue as int?,
                    SavedAt = DateTime.Now
                };

                _draftService.SaveDraft(DRAFT_KEY, draft);

                // Show auto-save indicator
                AutoSaveIndicator.Text = $"💾 Draft saved at {DateTime.Now:HH:mm:ss}";
                AutoSaveIndicator.Visibility = Visibility.Visible;

                // Hide indicator after 3 seconds
                await Task.Delay(3000, cancellationToken);
                AutoSaveIndicator.Visibility = Visibility.Collapsed;
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
        }

        private void CheckForDraft()
        {
            if (_draftService.HasDraft(DRAFT_KEY))
            {
                var draft = _draftService.GetDraft<ProductDraft>(DRAFT_KEY);
                if (draft != null)
                {
                    // Check if draft is expired
                    var daysOld = (DateTime.Now - draft.SavedAt).TotalDays;
                    if (daysOld > DRAFT_EXPIRATION_DAYS)
                    {
                        // Draft expired, clear it
                        _draftService.ClearDraft(DRAFT_KEY);
                        return;
                    }

                    // Show draft restore banner
                    var timeAgo = GetTimeAgo(draft.SavedAt);
                    DraftMessage.Text = $"You have unsaved changes from {timeAgo}";
                    DraftBanner.Visibility = Visibility.Visible;
                }
            }
        }

        private string GetTimeAgo(DateTime savedAt)
        {
            var span = DateTime.Now - savedAt;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} minutes ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
            return $"{(int)span.TotalDays} days ago";
        }

        private async void OnRestoreDraftClick(object sender, RoutedEventArgs e)
        {
            var draft = _draftService.GetDraft<ProductDraft>(DRAFT_KEY);
            if (draft != null)
            {
                NameBox.Text = draft.Name ?? string.Empty;
                SkuBox.Text = draft.Sku ?? string.Empty;
                DescriptionBox.Text = draft.Description ?? string.Empty;
                PriceBox.Text = draft.Price ?? string.Empty;
                CostPriceBox.Text = draft.CostPrice ?? string.Empty;
                StockBox.Text = draft.Stock ?? string.Empty;

                // Restore category selection
                if (draft.CategoryId.HasValue)
                {
                    CategoryComboBox.SelectedValue = draft.CategoryId.Value;
                }

                // Restore image preview if path exists
                if (!string.IsNullOrEmpty(draft.ImageUrl))
                {
                    try
                    {
                        var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(draft.ImageUrl);
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(await file.OpenAsync(Windows.Storage.FileAccessMode.Read));
                        PreviewImage.Source = bitmap;
                        PreviewImage.Visibility = Visibility.Visible;
                        PlaceholderIcon.Visibility = Visibility.Collapsed;
                        _selectedImagePath = draft.ImageUrl;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to restore image: {ex.Message}");
                        // File not accessible, ignore
                    }
                }

                DraftBanner.Visibility = Visibility.Collapsed;
            }
        }

        private void OnDiscardDraftClick(object sender, RoutedEventArgs e)
        {
            _draftService.ClearDraft(DRAFT_KEY);
            DraftBanner.Visibility = Visibility.Collapsed;
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
                    
                    // Trigger autosave
                    OnFieldChanged(null, null);
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

                // Clear draft on successful save
                _draftService.ClearDraft(DRAFT_KEY);

                // Reload categories in ShellPage to update counts in sidebar
                await ReloadShellCategories();

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
            // Keep draft when user cancels
            Frame.GoBack();
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
    }
}
