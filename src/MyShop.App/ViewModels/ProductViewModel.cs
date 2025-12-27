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

        public ProductViewModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _products = new ObservableCollection<Product>();
            _allProducts = new List<Product>();
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

            Products = new ObservableCollection<Product>(sorted.ToList());
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