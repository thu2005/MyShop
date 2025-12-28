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
        private ObservableCollection<string> _productImages;

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
            _productImages = new ObservableCollection<string>();
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
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                Category = product.Category,
                IsActive = product.IsActive
            };

            // Initialize string wrappers (use 0 as display value if null, but don't modify product)
            EditStockText = CurrentProduct.Stock.ToString();
            EditCostPriceText = CurrentProduct.CostPrice?.ToString() ?? string.Empty;
            EditPriceText = CurrentProduct.Price.ToString();

            // Load product images
            ProductImages.Clear();
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                ProductImages.Add(product.ImageUrl);
            }

            // Load categories
            await LoadCategoriesAsync();
            
            // Trigger binding update for CategoryId after categories are loaded
            OnPropertyChanged(nameof(CurrentProduct));
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
                    ImageUrl = CurrentProduct.ImageUrl,
                    CategoryId = CurrentProduct.CategoryId,
                    Category = CurrentProduct.Category,
                    IsActive = CurrentProduct.IsActive
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
            CurrentProduct.ImageUrl = OriginalProduct.ImageUrl;
            
            // Restore string values
            EditStockText = OriginalProduct.Stock.ToString();
            EditCostPriceText = OriginalProduct.CostPrice?.ToString() ?? string.Empty;
            EditPriceText = OriginalProduct.Price.ToString();
            
            IsEditMode = false;
        }

        [RelayCommand]
        private async Task UploadImage()
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

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    IsBusy = true;

                    // Upload to backend
                    var imageUrl = await _imageUploadService.UploadProductImageAsync(file);

                    // Update product
                    CurrentProduct.ImageUrl = imageUrl;
                    ProductImages.Clear();
                    ProductImages.Add(imageUrl);
                    
                    // Auto-save to database
                    await _productRepository.UpdateAsync(CurrentProduct);
                    
                    // Update original product with new image
                    OriginalProduct.ImageUrl = imageUrl;
                    
                    // Trigger UI refresh
                    OnPropertyChanged(nameof(CurrentProduct));
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
