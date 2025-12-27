using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System.Collections.Generic;
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
            // GIỮ LẠI DÒNG NÀY TỪ MAIN
            _categoryRepository = categoryRepository; 

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            AdminPanelCommand = new RelayCommand(_ => ExecuteOpenAdminPanel());

            // Load categories on startup with proper error handling
            Task.Run(async () => {
                try 
                {
                    await LoadCategoriesAsync();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Initial Category Load failed: {ex.Message}");
                }
            });
        }

        public ObservableCollection<CategoryStat> Categories { get; } = new();

        public async Task LoadCategoriesAsync()
        {
            try
            {
                // Load real categories from database
                var categories = await _categoryRepository.GetAllAsync();
                var products = await _productRepository.GetAllAsync();
                
                // Switch to UI thread if necessary (though WinUI 3 usually handles this if awaited correctly)
                // But let's be safe.
                
                var stats = new List<CategoryStat>
                {
                    new CategoryStat { Id = 0, Name = "All Products", Count = products.Count }
                };

                // GIỮ LẠI LOGIC TỪ MAIN (Có thể giữ comment của bạn nếu cần)
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