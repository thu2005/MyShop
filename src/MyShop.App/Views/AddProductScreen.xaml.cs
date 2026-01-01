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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.App.Views
{
    public sealed partial class AddProductScreen : Page
    {
        private const string DRAFT_KEY = "draft_add_product";
        private const int DRAFT_EXPIRATION_DAYS = 7;
        private const int AUTO_SAVE_DELAY_MS = 500;

        public AddProductViewModel ViewModel { get; }
        private readonly IDraftService _draftService;
        private ObservableCollection<ProductImage> _imageUrls;
        private int _mainImageIndex = 0;
        private CancellationTokenSource _autoSaveCts;

        public AddProductScreen()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<AddProductViewModel>();
            _draftService = App.Current.Services.GetService<IDraftService>();
            _imageUrls = new ObservableCollection<ProductImage>();
            ThumbnailsRepeater.ItemsSource = _imageUrls;
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

                    ImageUrl = _imageUrls.Count > 0 ? _imageUrls[_mainImageIndex].ImageUrl : null,
                    Images = _imageUrls.ToList(),
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

                // Restore images
                if (draft.Images != null && draft.Images.Count > 0)
                {
                    try
                    {
                        _imageUrls.Clear();
                        foreach (var img in draft.Images)
                        {
                            _imageUrls.Add(img);
                            if (img.IsMain) _mainImageIndex = _imageUrls.Count - 1;
                        }

                        if (_imageUrls.Count > 0 && !_imageUrls.Any(i => i.IsMain))
                        {
                             // Fallback if no main image marked
                             _mainImageIndex = 0;
                             _imageUrls[0].IsMain = true;
                        }
                        
                        UpdateImagePreview();
                        ThumbnailsContainer.Visibility = Visibility.Visible;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to restore images: {ex.Message}");
                    }
                }
                else if (!string.IsNullOrEmpty(draft.ImageUrl))
                {
                    try
                    {
                        // Legacy restore for single image path drafts
                        _imageUrls.Clear();
                        var productImg = new ProductImage 
                        { 
                            ImageUrl = draft.ImageUrl, 
                            IsMain = true, 
                            DisplayOrder = 0 
                        };
                        _imageUrls.Add(productImg);
                        _mainImageIndex = 0;
                        UpdateImagePreview();
                        ThumbnailsContainer.Visibility = Visibility.Visible;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to restore image: {ex.Message}");
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

                var files = await picker.PickMultipleFilesAsync();
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        var imageUrl = await ViewModel.UploadImageAsync(file);
                        var newImg = new ProductImage 
                        { 
                            ImageUrl = imageUrl, 
                            DisplayOrder = _imageUrls.Count,
                            IsMain = _imageUrls.Count == 0 // First image added is main
                        };
                        _imageUrls.Add(newImg);
                    }

                    UpdateImagePreview();
                    ThumbnailsContainer.Visibility = _imageUrls.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    
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

                // Create product base
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

                // Prepare images for saving (Reorder to ensure IsMain is index 0)
                var imagesToSave = new List<ProductImage>();
                if (_imageUrls.Count > 0)
                {
                    // Fix main index if out of bounds
                    if (_mainImageIndex >= _imageUrls.Count) _mainImageIndex = 0;
                    
                    // Get main image and add first
                    var mainImage = _imageUrls[_mainImageIndex];
                    mainImage.IsMain = true;
                    mainImage.DisplayOrder = 0;
                    imagesToSave.Add(mainImage);

                    // Add others
                    int displayOrder = 1;
                    foreach (var img in _imageUrls)
                    {
                        if (img == mainImage) continue;
                        img.IsMain = false;
                        img.DisplayOrder = displayOrder++;
                        imagesToSave.Add(img);
                    }
                }
                
                newProduct.Images = imagesToSave;

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

    private void UpdateImagePreview()
        {
            if (_imageUrls.Count > 0)
            {
                if (_mainImageIndex >= _imageUrls.Count) _mainImageIndex = 0;
                var mainImageUrl = _imageUrls[_mainImageIndex].ImageUrl;
                MainImagePreview.Source = new BitmapImage(new Uri(mainImageUrl));
                MainImagePreview.Visibility = Visibility.Visible;
                PlaceholderIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainImagePreview.Visibility = Visibility.Collapsed;
                PlaceholderIcon.Visibility = Visibility.Visible;
            }
        }

        private void OnSetMainImageClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductImage imageItem)
            {
                var index = _imageUrls.IndexOf(imageItem);
                if (index >= 0)
                {
                    foreach (var img in _imageUrls) img.IsMain = false;
                    imageItem.IsMain = true;
                    _mainImageIndex = index;
                    UpdateImagePreview();
                    UpdateStarIndicators();
                    // Trigger autosave
                    OnFieldChanged(null, null);
                }
            }
        }

        private void OnDeleteImageClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductImage imageItem)
            {
                var index = _imageUrls.IndexOf(imageItem);
                _imageUrls.Remove(imageItem);
                
                if (index == _mainImageIndex)
                {
                    _mainImageIndex = 0;
                    if (_imageUrls.Count > 0) _imageUrls[0].IsMain = true;
                }
                else if (index < _mainImageIndex)
                {
                    _mainImageIndex--;
                }
                
                UpdateImagePreview();
                ThumbnailsContainer.Visibility = _imageUrls.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                UpdateStarIndicators();
                // Trigger autosave
                OnFieldChanged(null, null);
            }
        }

        private void UpdateStarIndicators()
        {
            // Force refresh of binding by recreating collection
            var temp = new List<ProductImage>(_imageUrls);
            _imageUrls.Clear();
            foreach (var item in temp) _imageUrls.Add(item);
        }

        private void OnImagePointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Grid overlay) overlay.Opacity = 1;
        }

        private void OnImagePointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Grid overlay) overlay.Opacity = 0;
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
