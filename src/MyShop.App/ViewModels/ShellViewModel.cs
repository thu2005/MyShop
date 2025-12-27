using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShop.App.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ShellViewModel(
            IAuthService authService,
            IAuthorizationService authorizationService,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _authService = authService;
            _authorizationService = authorizationService;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            AdminPanelCommand = new RelayCommand(_ => ExecuteOpenAdminPanel());

            // Load categories on startup
            _ = LoadCategoriesAsync();
        }

        public ObservableCollection<CategoryStat> Categories { get; } = new();

        public async Task LoadCategoriesAsync()
        {
            try
            {
                // Load real categories from database
                var categories = await _categoryRepository.GetAllAsync();
                var products = await _productRepository.GetAllAsync();

                var stats = new ObservableCollection<CategoryStat>
                {
                    new CategoryStat { Id = 0, Name = "All Products", Count = products.Count }
                };

                // Add real categories with product counts
                foreach (var category in categories)
                {
                    stats.Add(new CategoryStat
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Count = products.Count(p => p.CategoryId == category.Id)
                    });
                }

                Categories.Clear();
                foreach (var s in stats) Categories.Add(s);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shell Load Error: {ex.Message}");
            }
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            // Delete all products in this category first
            var products = await _productRepository.GetAllAsync();
            var productsInCategory = products.Where(p => p.CategoryId == categoryId).ToList();
            
            foreach (var product in productsInCategory)
            {
                await _productRepository.DeleteAsync(product.Id);
            }

            // Then delete the category
            await _categoryRepository.DeleteAsync(categoryId);
            await LoadCategoriesAsync();
        }

        public async Task UpdateCategoryAsync(int categoryId, string newName)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category != null)
            {
                category.Name = newName;
                await _categoryRepository.UpdateAsync(category);
                await LoadCategoriesAsync();
            }
        }

        public User? CurrentUser => _authService.CurrentUser;
        public UserRole UserRole => CurrentUser?.Role ?? UserRole.STAFF;
        public bool IsAdmin => _authorizationService.IsAuthorized(UserRole.ADMIN);

        public ICommand LogoutCommand { get; }
        public ICommand AdminPanelCommand { get; }

        public event System.Action? LogoutRequested;

        private async void ExecuteLogout()
        {
            await _authService.LogoutAsync();
            LogoutRequested?.Invoke();
        }

        private void ExecuteOpenAdminPanel()
        {
            System.Diagnostics.Debug.WriteLine("Admin Panel opened.");
        }
    }
}