using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using MyShop.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.App.ViewModels
{
    public partial class ProductDetailViewModel : ObservableObject
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageUploadService _imageUploadService;

        [ObservableProperty]
        private Product _currentProduct;

        [ObservableProperty]
        private Product _originalProduct; // Backup for cancel

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private ObservableCollection<Category> _categories;

        [ObservableProperty]
        private ObservableCollection<ProductImage> _productImagesCollection;

        // Editable properties
        [ObservableProperty]
        private string _editName = string.Empty;

        [ObservableProperty]
        private string _editDescription = string.Empty;

        [ObservableProperty]
        private string _editSku = string.Empty;

        [ObservableProperty]
        private int _editStock;

        [ObservableProperty]
        private decimal? _editCostPrice;

        [ObservableProperty]
        private decimal _editPrice;

        [ObservableProperty]
        private Category? _editCategory;

        // String wrappers for numeric fields to avoid TwoWay binding crashes
        [ObservableProperty]
        private string _editStockText = "0";

        [ObservableProperty]
        private string _editCostPriceText = "0";

        [ObservableProperty]
        private string _editPriceText = "0";

        private readonly IAuthService _authService;
        private readonly IAuthorizationService _authorizationService;

        // Role-based properties
        public User? CurrentUser => _authService.CurrentUser;
        public UserRole UserRole => CurrentUser?.Role ?? UserRole.STAFF;
        public bool IsAdmin => _authorizationService.IsAuthorized(UserRole.ADMIN);
        public bool IsStaff => !IsAdmin;

        // Computed property for main image
        public ProductImage? MainImage => CurrentProduct?.Images?.FirstOrDefault(i => i.IsMain) 
                                          ?? CurrentProduct?.Images?.FirstOrDefault();

        [ObservableProperty]
        private string? _mainImageUrl;

        public ProductDetailViewModel(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository, 
            IImageUploadService imageUploadService,
            IAuthService authService,
            IAuthorizationService authorizationService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _imageUploadService = imageUploadService;
            _authService = authService;
            _authorizationService = authorizationService;
            _categories = new ObservableCollection<Category>();
        }

        public async Task LoadProduct(Product product)
        {
            CurrentProduct = product;
            OriginalProduct = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Sku = product.Sku,
                Barcode = product.Barcode,
                Price = product.Price,
                CostPrice = product.CostPrice,
                Stock = product.Stock,
                MinStock = product.MinStock,
                CategoryId = product.CategoryId,
                Category = product.Category,
                IsActive = product.IsActive,
                Images = product.Images?.Select(img => new ProductImage
                {
                    Id = img.Id,
                    ProductId = img.ProductId,
                    ImageUrl = img.ImageUrl,
                    DisplayOrder = img.DisplayOrder,
                    IsMain = img.IsMain
                }).ToList() ?? new System.Collections.Generic.List<ProductImage>()
            };

            // Initialize string wrappers
            EditStockText = CurrentProduct.Stock.ToString();
            EditCostPriceText = CurrentProduct.CostPrice?.ToString() ?? string.Empty;
            EditPriceText = CurrentProduct.Price.ToString();

            // Load product images into observable collection
            ProductImagesCollection = new ObservableCollection<ProductImage>(CurrentProduct.Images ?? new System.Collections.Generic.List<ProductImage>());

            // Update main image URL
            UpdateMainImageUrl();

            // Load categories
            await LoadCategoriesAsync();
            
            // Trigger binding update
            OnPropertyChanged(nameof(CurrentProduct));
            OnPropertyChanged(nameof(MainImage));
        }

        private void UpdateMainImageUrl()
        {
            var mainImg = CurrentProduct?.Images?.FirstOrDefault(i => i.IsMain);
            MainImageUrl = mainImg?.ImageUrl ?? CurrentProduct?.Images?.FirstOrDefault()?.ImageUrl;
        }

        private void SyncEditablePropertiesFromProduct()
        {
            EditName = CurrentProduct.Name;
            EditDescription = CurrentProduct.Description ?? string.Empty;
            EditSku = CurrentProduct.Sku;
            EditStock = CurrentProduct.Stock;
            EditCostPrice = CurrentProduct.CostPrice;
            EditPrice = CurrentProduct.Price;
            EditCategory = CurrentProduct.Category;
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var cats = await _categoryRepository.GetAllAsync();
                Categories.Clear();
                foreach (var cat in cats)
                {
                    Categories.Add(cat);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load categories: {ex.Message}");
            }
        }

        [RelayCommand]
        private void EnterEditMode()
        {
            IsEditMode = true;
        }

        [RelayCommand]
        private async Task SaveChanges()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                
                // Parse string values using InvariantCulture
                if (int.TryParse(EditStockText, out int stock))
                    CurrentProduct.Stock = stock;
                
                if (decimal.TryParse(EditCostPriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal costPrice))
                    CurrentProduct.CostPrice = costPrice;
                
                if (decimal.TryParse(EditPriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                    CurrentProduct.Price = price;
                
                // Update category - find category object from ID
                if (CurrentProduct.CategoryId > 0)
                {
                    CurrentProduct.Category = Categories.FirstOrDefault(c => c.Id == CurrentProduct.CategoryId);
                }

                await _productRepository.UpdateAsync(CurrentProduct);
                
                // Update original with new values
                OriginalProduct = new Product
                {
                    Id = CurrentProduct.Id,
                    Name = CurrentProduct.Name,
                    Description = CurrentProduct.Description,
                    Sku = CurrentProduct.Sku,
                    Barcode = CurrentProduct.Barcode,
                    Price = CurrentProduct.Price,
                    CostPrice = CurrentProduct.CostPrice,
                    Stock = CurrentProduct.Stock,
                    MinStock = CurrentProduct.MinStock,
                    CategoryId = CurrentProduct.CategoryId,
                    Category = CurrentProduct.Category,
                    IsActive = CurrentProduct.IsActive,
                    Images = CurrentProduct.Images?.Select(img => new ProductImage
                    {
                        Id = img.Id,
                        ProductId = img.ProductId,
                        ImageUrl = img.ImageUrl,
                        DisplayOrder = img.DisplayOrder,
                        IsMain = img.IsMain
                    }).ToList() ?? new System.Collections.Generic.List<ProductImage>()
                };
                
                // Update string wrappers to reflect saved values
                EditStockText = CurrentProduct.Stock.ToString();
                EditCostPriceText = CurrentProduct.CostPrice?.ToString() ?? string.Empty;
                EditPriceText = CurrentProduct.Price.ToString();
                
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelChanges()
        {
            // Restore original values
            CurrentProduct.Name = OriginalProduct.Name;
            CurrentProduct.Description = OriginalProduct.Description;
            CurrentProduct.Sku = OriginalProduct.Sku;
            CurrentProduct.Stock = OriginalProduct.Stock;
            CurrentProduct.CostPrice = OriginalProduct.CostPrice;
            CurrentProduct.Price = OriginalProduct.Price;
            CurrentProduct.CategoryId = OriginalProduct.CategoryId;
            CurrentProduct.Category = OriginalProduct.Category;
            CurrentProduct.Images = OriginalProduct.Images?.Select(img => new ProductImage
            {
                Id = img.Id,
                ProductId = img.ProductId,
                ImageUrl = img.ImageUrl,
                DisplayOrder = img.DisplayOrder,
                IsMain = img.IsMain
            }).ToList() ?? new System.Collections.Generic.List<ProductImage>();
            
            // Restore string values
            EditStockText = OriginalProduct.Stock.ToString();
            EditCostPriceText = OriginalProduct.CostPrice?.ToString() ?? string.Empty;
            EditPriceText = OriginalProduct.Price.ToString();
            
            OnPropertyChanged(nameof(MainImage));
            IsEditMode = false;
        }

        [RelayCommand]
        private async Task ManageImages()
        {
            if (IsBusy) return;

            try
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                // Get window handle for WinUI 3
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                // Allow multiple file selection
                var files = await picker.PickMultipleFilesAsync();
                if (files != null && files.Count > 0)
                {
                    IsBusy = true;

                    foreach (var file in files)
                    {
                        // Upload to backend to get URL (but don't save to DB yet)
                        var imageUrl = await _imageUploadService.UploadProductImageAsync(file);

                        // Add new image to product (in memory only)
                        var newImage = new ProductImage
                        {
                            ProductId = CurrentProduct.Id,
                            ImageUrl = imageUrl,
                            DisplayOrder = CurrentProduct.Images?.Count ?? 0,
                            IsMain = CurrentProduct.Images?.Count == 0 // First image is main
                        };
                        
                        if (CurrentProduct.Images == null)
                            CurrentProduct.Images = new System.Collections.Generic.List<ProductImage>();
                        
                        CurrentProduct.Images.Add(newImage);
                        
                        // Add to observable collection for instant UI preview
                        ProductImagesCollection.Add(newImage);
                    }
                    
                    // Don't auto-save - user needs to click UPDATE button
                    // Images will be saved when SaveChanges is called
                    
                    // Trigger UI refresh
                    OnPropertyChanged(nameof(MainImage));
                    var temp = CurrentProduct;
                    CurrentProduct = null;
                    CurrentProduct = temp;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Upload failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteProduct()
        {
            if (IsBusy || CurrentProduct == null) return;

            try
            {
                IsBusy = true;
                await _productRepository.DeleteAsync(CurrentProduct.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete failed: {ex.Message}");
                throw; // Re-throw to let the UI handle the error
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
