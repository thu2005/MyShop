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
        private readonly ILicenseService _licenseService;
        private bool _isCategoriesLoaded = false;

        public ShellViewModel(
            IAuthService authService,
            IAuthorizationService authorizationService,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ILicenseService licenseService)
        {
            _authService = authService;
            _authorizationService = authorizationService;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _licenseService = licenseService;

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            AdminPanelCommand = new RelayCommand(_ => ExecuteOpenAdminPanel());

            // NOTE: Categories are now loaded via LoadCategoriesAsync() called from ShellPage.NavView_Loaded
        }

        public ObservableCollection<CategoryStat> Categories { get; } = new();

        public async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var products = await _productRepository.GetAllAsync();
                
                var stats = new List<CategoryStat>
                {
                    new CategoryStat { Id = 0, Name = "All Products", Count = products.Count }
                };

                foreach (var category in categories)
                {
                    stats.Add(new CategoryStat
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Count = products.Count(p => p.CategoryId == category.Id)
                    });
                }

                // Since this is called from UI thread (via ShellPage), we can update directly
                Categories.Clear();
                foreach (var s in stats) Categories.Add(s);
                
                _isCategoriesLoaded = true;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shell Load Error: {ex.Message}");
            }
        }

        public async Task EnsureCategoriesLoadedAsync()
        {
            if (!_isCategoriesLoaded)
            {
                await LoadCategoriesAsync();
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

        // License/Trial Properties
        public bool IsTrialActive => _licenseService.GetLicenseStatus() == Core.Models.LicenseStatus.TrialActive;
        public bool IsLicenseActivated => _licenseService.GetLicenseStatus() == Core.Models.LicenseStatus.Activated;
        public bool IsTrialExpired => _licenseService.GetLicenseStatus() == Core.Models.LicenseStatus.TrialExpired;
        public bool ShowTrialBanner => !IsLicenseActivated;
        public int TrialDaysRemaining => _licenseService.GetRemainingTrialDays();
        public string LicenseStatusMessage => _licenseService.GetStatusMessage();
        
        public bool CanCreateOrder => _licenseService.IsFeatureAllowed("CreateOrder");
        public bool CanAddProduct => _licenseService.IsFeatureAllowed("AddProduct");
        public bool CanEditProduct => _licenseService.IsFeatureAllowed("EditProduct");
        public bool CanManageDiscounts => _licenseService.IsFeatureAllowed("ManageDiscounts");

        public void InitializeLicense()
        {
            _licenseService.InitializeTrial();
            _licenseService.RecordAppRun();
            OnPropertyChanged(nameof(IsTrialActive));
            OnPropertyChanged(nameof(IsTrialExpired));
            OnPropertyChanged(nameof(ShowTrialBanner));
            OnPropertyChanged(nameof(TrialDaysRemaining));
            OnPropertyChanged(nameof(LicenseStatusMessage));
            OnPropertyChanged(nameof(CanCreateOrder));
            OnPropertyChanged(nameof(CanAddProduct));
            OnPropertyChanged(nameof(CanEditProduct));
            OnPropertyChanged(nameof(CanManageDiscounts));
        }

        public Core.Models.LicenseStatus GetLicenseStatus() => _licenseService.GetLicenseStatus();

#if DEBUG
        public void DebugForceExpire()
        {
            _licenseService.ForceTrialExpired();
            InitializeLicense(); // Refresh all license-related properties
        }

        public void DebugResetTrial()
        {
            _licenseService.ResetTrial();
            InitializeLicense(); // Refresh all license-related properties
        }
#endif
    }
}