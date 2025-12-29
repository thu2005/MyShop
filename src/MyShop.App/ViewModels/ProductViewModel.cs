using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MyShop.App.Services;
using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;

namespace MyShop.App.ViewModels
{
    public class CategoryStat
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int Id { get; set; }
        public string DisplayText => $"{Name} ({Count})";
    }

    public class ProductViewModel : ViewModelBase
    {
        // --- Dependencies ---
        private readonly ProductService _productService; // From Refactoring
        private readonly IAuthService _authService;      // From Main (Auth)
        private readonly IAuthorizationService _authorizationService; // From Main (Auth)

        // --- Data Collections ---
        private List<Product> _allProducts;
        private List<Product> _filteredProducts; // Needed for Pagination
        private ObservableCollection<Product> _products;
        private CategoryStat _selectedCategory;
        private ObservableCollection<CategoryStat> _categories;

        // --- Filter States ---
        private string _currentSearchTerm = string.Empty;
        private decimal? _minPrice = null;
        private decimal? _maxPrice = null;
        private string _primarySort = null;

        // --- Pagination Properties ---
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalProducts = 0;
        private int _pageSize = 20;

        public List<int> AvailablePageSizes { get; } = new List<int> { 10, 20, 50, 100 };

        // --- Role-based Properties ---
        public User? CurrentUser => _authService.CurrentUser;
        public UserRole UserRole => CurrentUser?.Role ?? UserRole.STAFF;
        public bool IsAdmin => _authorizationService.IsAuthorized(UserRole.ADMIN);

        public ProductViewModel(
            IProductRepository productRepository,
            IAuthService authService,
            IAuthorizationService authorizationService)
        {
            // Initialize the Service
            _productService = new ProductService(productRepository);

            // Initialize Auth
            _authService = authService;
            _authorizationService = authorizationService;

            _products = new ObservableCollection<Product>();
            _allProducts = new List<Product>();
            _filteredProducts = new List<Product>();
            _categories = new ObservableCollection<CategoryStat>();

            _ = LoadProductsAsync();
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public CategoryStat SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterProducts();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public int TotalProducts
        {
            get => _totalProducts;
            set => SetProperty(ref _totalProducts, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    CurrentPage = 1;
                    UpdatePagination();
                }
            }
        }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public async Task LoadProductsAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                // Use Service to fetch data
                var dbData = await _productService.LoadProductsAsync();

                _allProducts.Clear();
                _allProducts.AddRange(dbData);

                FilterProducts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void SelectCategoryById(int categoryId)
        {
            SelectedCategory = new CategoryStat { Id = categoryId, Name = "Selection" };
        }

        public async void ClearFilters()
        {
            _currentSearchTerm = string.Empty;
            _minPrice = null;
            _maxPrice = null;
            _primarySort = null;
            SelectedCategory = null;

            await LoadProductsAsync();
        }

        public Task SearchProductsAsync(string keyword)
        {
            _currentSearchTerm = keyword;
            FilterProducts();
            return Task.CompletedTask;
        }

        private void FilterProducts()
        {
            // 1. Search (Service)
            var filtered = _productService.SearchProducts(_allProducts, _currentSearchTerm);

            // 2. Filter by Category (Service)
            if (SelectedCategory != null && SelectedCategory.Id != 0)
            {
                filtered = _productService.FilterByCategory(filtered, SelectedCategory.Id);
            }

            // 3. Filter by Price (Service)
            filtered = _productService.FilterByPriceRange(filtered, _minPrice, _maxPrice);

            // 4. Sort (Service)
            if (!string.IsNullOrEmpty(_primarySort))
            {
                filtered = _productService.SortProducts(filtered, _primarySort);
            }

            // 5. Pagination Logic
            _filteredProducts.Clear();
            _filteredProducts.AddRange(filtered);

            CurrentPage = 1;
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            TotalProducts = _filteredProducts.Count;
            TotalPages = (int)Math.Ceiling((double)TotalProducts / PageSize);

            if (TotalPages == 0) TotalPages = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pagedProducts = _filteredProducts
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Products = new ObservableCollection<Product>(pagedProducts);

            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
        }

        public void GoToNextPage()
        {
            if (HasNextPage) { CurrentPage++; UpdatePagination(); }
        }

        public void GoToPreviousPage()
        {
            if (HasPreviousPage) { CurrentPage--; UpdatePagination(); }
        }

        public void GoToFirstPage()
        {
            CurrentPage = 1; UpdatePagination();
        }

        public void GoToLastPage()
        {
            CurrentPage = TotalPages; UpdatePagination();
        }

        public async Task AddProductAsync(Product newProduct)
        {
            await _productService.AddProductAsync(newProduct);
            _allProducts.Add(newProduct);
            FilterProducts();
        }

        public async Task UpdateProductAsync(Product updatedProduct)
        {
            await _productService.UpdateProductAsync(updatedProduct);
            var index = _allProducts.FindIndex(p => p.Id == updatedProduct.Id);
            if (index >= 0) _allProducts[index] = updatedProduct;
            FilterProducts();
        }

        public async Task DeleteProductAsync(int productId)
        {
            await _productService.DeleteProductAsync(productId);
            var p = _allProducts.FirstOrDefault(x => x.Id == productId);
            if (p != null) _allProducts.Remove(p);
            FilterProducts();
        }

        public void SetPriceRange(decimal? minPrice, decimal? maxPrice)
        {
            _minPrice = minPrice;
            _maxPrice = maxPrice;
            FilterProducts();
        }

        public void SetSorting(string primarySort, string secondarySort)
        {
            _primarySort = primarySort;
            FilterProducts();
        }
    }
}
