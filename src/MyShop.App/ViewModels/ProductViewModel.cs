using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
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
        private readonly IProductRepository _productRepository;

        private List<Product> _allProducts;
        private ObservableCollection<Product> _products;
        private ObservableCollection<CategoryStat> _categories;
        private CategoryStat _selectedCategory;

        // Search state
        private string _currentSearchTerm = string.Empty;
        private List<Product> _currentSearchResults = new List<Product>();

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

        public ObservableCollection<CategoryStat> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
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

                RefreshCategoryStats();
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

        public async Task AddProductAsync(Product newProduct)
        {
            try
            {
                IsBusy = true;
                var createdProduct = await _productRepository.AddAsync(newProduct);

                _allProducts.Insert(0, createdProduct);

                RefreshCategoryStats();
                FilterProducts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdateProductAsync(Product updatedProduct)
        {
            try
            {
                IsBusy = true;
                await _productRepository.UpdateAsync(updatedProduct);

                // 1. Update in main cache
                var index = _allProducts.FindIndex(p => p.Id == updatedProduct.Id);
                if (index >= 0)
                {
                    _allProducts[index] = updatedProduct;
                }

                // 2. Update in search results if active
                var searchIndex = _currentSearchResults.FindIndex(p => p.Id == updatedProduct.Id);
                if (searchIndex >= 0)
                {
                    _currentSearchResults[searchIndex] = updatedProduct;
                }

                // 3. Refresh UI
                FilterProducts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task SearchProductsAsync(string keyword)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                _currentSearchTerm = keyword;

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    _currentSearchResults.Clear();
                }
                else
                {
                    _currentSearchResults = await _productRepository.SearchByNameAsync(keyword);
                }

                FilterProducts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Search Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void RefreshCategoryStats()
        {
            var currentId = SelectedCategory?.Id ?? 0;

            var stats = new ObservableCollection<CategoryStat>
            {
                new CategoryStat { Id = 0, Name = "All", Count = _allProducts.Count },
                new CategoryStat { Id = 1, Name = "Iphone", Count = _allProducts.Count(p => p.CategoryId == 1) },
                new CategoryStat { Id = 2, Name = "Ipad", Count = _allProducts.Count(p => p.CategoryId == 2) },
                new CategoryStat { Id = 3, Name = "Laptop", Count = _allProducts.Count(p => p.CategoryId == 3) },
                new CategoryStat { Id = 4, Name = "Tablet", Count = _allProducts.Count(p => p.CategoryId == 4) },
                new CategoryStat { Id = 5, Name = "PC", Count = _allProducts.Count(p => p.CategoryId == 5) },
                new CategoryStat { Id = 6, Name = "TV", Count = _allProducts.Count(p => p.CategoryId == 6) }
            };

            Categories = stats;

            var restoredSelection = stats.FirstOrDefault(c => c.Id == currentId);
            if (restoredSelection != null)
            {
                _selectedCategory = restoredSelection;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        private void FilterProducts()
        {
            // Determine source: Search Results OR All Products
            IEnumerable<Product> source = string.IsNullOrWhiteSpace(_currentSearchTerm)
                ? _allProducts
                : _currentSearchResults;

            // Apply Category Filter
            if (SelectedCategory != null && SelectedCategory.Id != 0)
            {
                source = source.Where(p => p.CategoryId == SelectedCategory.Id);
            }

            // Update UI
            Products = new ObservableCollection<Product>(source.ToList());
        }

        public async Task DeleteProductAsync(int productId)
        {
            try
            {
                IsBusy = true;
                await _productRepository.DeleteAsync(productId);

                // 1. Remove from main cache
                var productToRemove = _allProducts.FirstOrDefault(p => p.Id == productId);
                if (productToRemove != null)
                {
                    _allProducts.Remove(productToRemove);
                }

                // 2. Remove from search results if active
                var searchItemToRemove = _currentSearchResults.FirstOrDefault(p => p.Id == productId);
                if (searchItemToRemove != null)
                {
                    _currentSearchResults.Remove(searchItemToRemove);
                }

                // 3. Refresh UI
                RefreshCategoryStats();
                FilterProducts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}