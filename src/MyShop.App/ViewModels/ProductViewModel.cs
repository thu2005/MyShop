using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;

namespace MyShop.App.ViewModels
{
    // Ensure this class is public and accessible
    public class CategoryStat
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int Id { get; set; }
        public string DisplayText => $"{Name} ({Count})";
    }

    public class ProductViewModel : ViewModelBase
    {
        private readonly IProductRepository _productRepository;
        private List<Product> _allProducts;
        private List<Product> _filteredProducts; // Products after filtering, before pagination
        private ObservableCollection<Product> _products;
        private CategoryStat _selectedCategory;

        // Keep this for binding in the View if needed, or internal logic
        private ObservableCollection<CategoryStat> _categories;

        private string _currentSearchTerm = string.Empty;
        private List<Product> _currentSearchResults = new List<Product>();
        private decimal? _minPrice = null;
        private decimal? _maxPrice = null;
        private string _primarySort = null;
        private string _secondarySort = null;

        // Pagination properties
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalProducts = 0;
        private int _pageSize = 20;

        public List<int> AvailablePageSizes { get; } = new List<int> { 10, 20, 50, 100 };

        public ProductViewModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
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
                var dbData = await _productRepository.GetAllAsync();
                _allProducts.Clear();
                _allProducts.AddRange(dbData);

                // We don't necessarily need to populate _categories for the UI anymore, 
                // but we keep the logic if we want to construct the "All" category internally.
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
            // Create a temporary stat to trigger the filter logic
            // In a real app, you might want to look up the exact name, but ID is enough for filtering
            SelectedCategory = new CategoryStat { Id = categoryId };
        }

        public async void ClearFilters()
        {
            // Reset all filter states
            _currentSearchTerm = string.Empty;
            _currentSearchResults.Clear();
            _minPrice = null;
            _maxPrice = null;
            _primarySort = null;
            _secondarySort = null;
            SelectedCategory = null;

            // Reload all products
            await LoadProductsAsync();
        }

        public async Task FilterProductsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                List<Product> filteredProducts;

                // Priority 1: Filter by search term using API if exists
                if (!string.IsNullOrWhiteSpace(_currentSearchTerm))
                {
                    filteredProducts = await _productRepository.SearchByNameAsync(_currentSearchTerm);
                }
                // Priority 2: Filter by category using API if selected
                else if (SelectedCategory != null && SelectedCategory.Id != 0)
                {
                    filteredProducts = await _productRepository.GetByCategoryAsync(SelectedCategory.Id);
                }
                else
                {
                    // No filter selected, get all
                    filteredProducts = await _productRepository.GetAllAsync();
                }

                // Then apply additional client-side filters on the API results
                
                // Filter by category if search was used (client-side)
                if (!string.IsNullOrWhiteSpace(_currentSearchTerm) && SelectedCategory != null && SelectedCategory.Id != 0)
                {
                    filteredProducts = filteredProducts.Where(p => p.CategoryId == SelectedCategory.Id).ToList();
                }

                // Filter by custom price range if set (client-side)
                if (_minPrice.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.Price >= _minPrice.Value).ToList();
                }
                if (_maxPrice.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.Price <= _maxPrice.Value).ToList();
                }

                // Apply sorting
                var sorted = ApplySorting(filteredProducts);

                _filteredProducts.Clear();
                _filteredProducts.AddRange(sorted.ToList());

                CurrentPage = 1;
                UpdatePagination();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to filter products: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error filtering products: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterProducts()
        {
            IEnumerable<Product> source = string.IsNullOrWhiteSpace(_currentSearchTerm)
                ? _allProducts
                : _currentSearchResults;

            // Filter by category
            if (SelectedCategory != null && SelectedCategory.Id != 0)
            {
                source = source.Where(p => p.CategoryId == SelectedCategory.Id);
            }

            // Filter by price range
            if (_minPrice.HasValue)
            {
                source = source.Where(p => p.Price >= _minPrice.Value);
            }
            if (_maxPrice.HasValue)
            {
                source = source.Where(p => p.Price <= _maxPrice.Value);
            }

            // Apply sorting
            var sorted = ApplySorting(source);

            // Store filtered results before pagination
            _filteredProducts.Clear();
            _filteredProducts.AddRange(sorted.ToList());

            // Reset to first page when filters change
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
            if (HasNextPage)
            {
                CurrentPage++;
                UpdatePagination();
            }
        }

        public void GoToPreviousPage()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                UpdatePagination();
            }
        }

        public void GoToFirstPage()
        {
            CurrentPage = 1;
            UpdatePagination();
        }

        public void GoToLastPage()
        {
            CurrentPage = TotalPages;
            UpdatePagination();
        }


        public Task SearchProductsAsync(string keyword)
        {
            _currentSearchTerm = keyword;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _currentSearchResults.Clear();
            }
            else
            {
                // Client-side filtering: search by Name, SKU, or Description
                var lowerKeyword = keyword.ToLower();
                _currentSearchResults = _allProducts
                    .Where(p => 
                        (p.Name?.ToLower().Contains(lowerKeyword) ?? false) ||
                        (p.Sku?.ToLower().Contains(lowerKeyword) ?? false) ||
                        (p.Description?.ToLower().Contains(lowerKeyword) ?? false))
                    .ToList();
            }

            FilterProducts();
            return Task.CompletedTask;
        }

        public async Task AddProductAsync(Product newProduct)
        {
            var created = await _productRepository.AddAsync(newProduct);
            _allProducts.Insert(0, created);
            FilterProducts();
        }

        public async Task UpdateProductAsync(Product updatedProduct)
        {
            await _productRepository.UpdateAsync(updatedProduct);
            var index = _allProducts.FindIndex(p => p.Id == updatedProduct.Id);
            if (index >= 0) _allProducts[index] = updatedProduct;
            FilterProducts();
        }

        public async Task DeleteProductAsync(int productId)
        {
            await _productRepository.DeleteAsync(productId);
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
            _secondarySort = secondarySort;
            FilterProducts();
        }

        private IEnumerable<Product> ApplySorting(IEnumerable<Product> source)
        {
            if (string.IsNullOrEmpty(_primarySort))
                return source;

            IOrderedEnumerable<Product> ordered = null;

            // Apply primary sort
            switch (_primarySort)
            {
                case "PriceAsc":
                    ordered = source.OrderBy(p => p.Price);
                    break;
                case "PriceDesc":
                    ordered = source.OrderByDescending(p => p.Price);
                    break;
                case "StockAsc":
                    ordered = source.OrderBy(p => p.Stock);
                    break;
                case "StockDesc":
                    ordered = source.OrderByDescending(p => p.Stock);
                    break;
                default:
                    return source;
            }

            // Apply secondary sort (tiebreaker)
            if (!string.IsNullOrEmpty(_secondarySort) && ordered != null)
            {
                switch (_secondarySort)
                {
                    case "PriceAsc":
                        ordered = ordered.ThenBy(p => p.Price);
                        break;
                    case "PriceDesc":
                        ordered = ordered.ThenByDescending(p => p.Price);
                        break;
                    case "StockAsc":
                        ordered = ordered.ThenBy(p => p.Stock);
                        break;
                    case "StockDesc":
                        ordered = ordered.ThenByDescending(p => p.Stock);
                        break;
                }
            }

            return ordered ?? source;
        }
    }
}