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
        private readonly IProductRepository _productRepository; // Injected

        public ShellViewModel(
            IAuthService authService,
            IAuthorizationService authorizationService,
            IProductRepository productRepository)
        {
            _authService = authService;
            _authorizationService = authorizationService;
            _productRepository = productRepository;

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
                var products = await _productRepository.GetAllAsync();
                
                // Switch to UI thread if necessary (though WinUI 3 usually handles this if awaited correctly)
                // But let's be safe.
                
                var stats = new List<CategoryStat>
                {
                    new CategoryStat { Id = 0, Name = "All Products", Count = products.Count },
                    new CategoryStat { Id = 1, Name = "Iphone", Count = products.Count(p => p.CategoryId == 1) },
                    new CategoryStat { Id = 2, Name = "Ipad", Count = products.Count(p => p.CategoryId == 2) },
                    new CategoryStat { Id = 3, Name = "Laptop", Count = products.Count(p => p.CategoryId == 3) },
                    new CategoryStat { Id = 4, Name = "Tablet", Count = products.Count(p => p.CategoryId == 4) },
                    new CategoryStat { Id = 5, Name = "PC", Count = products.Count(p => p.CategoryId == 5) },
                    new CategoryStat { Id = 6, Name = "TV", Count = products.Count(p => p.CategoryId == 6) }
                };

                // Clear and add items. 
                // We should ideally use the DispatcherQueue here if this can be called from background.
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