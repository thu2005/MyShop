using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.App.ViewModels
{
    public partial class DiscountsViewModel : ObservableObject
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IAuthService _authService;
        private readonly IAuthorizationService _authorizationService;
        private List<SelectableDiscount> _allDiscounts = new();

        [ObservableProperty]
        private ObservableCollection<SelectableDiscount> _discounts = new();

        [ObservableProperty]
        private SelectableDiscount? _selectedDiscount;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilter = "All";

        [ObservableProperty]
        private bool _isAllSelected;

        [ObservableProperty]
        private bool _isSelectionMode;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalDiscounts;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        // Role-based properties
        public User? CurrentUser => _authService.CurrentUser;
        public UserRole UserRole => CurrentUser?.Role ?? UserRole.STAFF;
        public bool IsAdmin => _authorizationService.IsAuthorized(UserRole.ADMIN);

        private int _pageSize = 10;

        public DiscountsViewModel(
            IDiscountRepository discountRepository,
            IAuthService authService,
            IAuthorizationService authorizationService)
        {
            _discountRepository = discountRepository;
            _authService = authService;
            _authorizationService = authorizationService;
            _ = LoadDiscountsFromBackendAsync();
        }

        partial void OnIsAllSelectedChanged(bool value)
        {
            foreach (var discount in Discounts)
            {
                discount.IsSelected = value;
            }
        }

        partial void OnIsSelectionModeChanged(bool value)
        {
            foreach (var discount in _allDiscounts)
            {
                discount.IsCheckboxVisible = value;
                if (!value) discount.IsSelected = false;
            }

            foreach (var discount in Discounts)
            {
                discount.IsCheckboxVisible = value;
                if (!value) discount.IsSelected = false;
            }

            if (!value) IsAllSelected = false;
        }

        [RelayCommand]
        private async Task LoadDiscountsAsync()
        {
            await LoadDiscountsFromBackendAsync();
        }

        [RelayCommand]
        private async Task AddDiscount(Discount discount)
        {
             if (discount == null) return;
             if (!IsAdmin)
             {
                 ErrorMessage = "Only Admin can create discounts!";
                 return;
             }

             try
             {
                 IsLoading = true;
                 ErrorMessage = null;
                 var createdDiscount = await _discountRepository.AddAsync(discount);
                 _allDiscounts.Insert(0, new SelectableDiscount(createdDiscount));
                 ApplyFilters();
             }
             catch (Exception ex)
             {
                 ErrorMessage = $"Error creating discount: {ex.Message}";
                 System.Diagnostics.Debug.WriteLine($"Error creating discount: {ex.Message}");
                 throw; // Re-throw to let UI handle it
             }
             finally
             {
                 IsLoading = false;
             }
        }

        [RelayCommand]
        private async Task UpdateDiscount(Discount discount)
        {
             if (discount == null) return;
             if (!IsAdmin) return; // Only admin can update discounts

             try
             {
                 IsLoading = true;
                 await _discountRepository.UpdateAsync(discount);

                 var index = _allDiscounts.FindIndex(d => d.Discount.Id == discount.Id);
                 if (index != -1)
                 {
                     bool wasSelected = _allDiscounts[index].IsSelected;
                     var wrapper = new SelectableDiscount(discount) { IsSelected = wasSelected };
                     _allDiscounts[index] = wrapper;
                     ApplyFilters();
                 }
             }
             catch (Exception ex)
             {
                 System.Diagnostics.Debug.WriteLine($"Error updating discount: {ex.Message}");
             }
             finally
             {
                 IsLoading = false;
             }
        }

        [RelayCommand]
        private async Task DeleteDiscount(Discount discount)
        {
            if (discount == null) return;
            if (!IsAdmin) return; // Only admin can delete discounts

            try
            {
                IsLoading = true;
                await _discountRepository.DeleteAsync(discount.Id);

                var item = _allDiscounts.FirstOrDefault(d => d.Discount.Id == discount.Id);
                if (item != null)
                {
                    _allDiscounts.Remove(item);
                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting discount: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteSelected()
        {
            if (!IsAdmin) return; // Only admin can delete discounts

            var selected = _discounts.Where(d => d.IsSelected).ToList();
            if (!selected.Any()) return;

            IsLoading = true;
            try
            {
                foreach (var item in selected)
                {
                    await _discountRepository.DeleteAsync(item.Discount.Id);
                    _allDiscounts.Remove(item);
                }
                ApplyFilters();
                IsAllSelected = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting selected discounts: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }

        [RelayCommand]
        private void PrevPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedFilterChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnCurrentPageChanged(int value)
        {
            _ = LoadDiscountsAsync();
        }

        private void ApplyFilters()
        {
            var filtered = _allDiscounts.AsEnumerable();

            // Filter by active status
            if (SelectedFilter == "Active")
            {
                filtered = filtered.Where(d => d.Discount.IsActive &&
                    (!d.Discount.EndDate.HasValue || d.Discount.EndDate.Value >= DateTime.UtcNow));
            }
            else if (SelectedFilter == "Expired")
            {
                filtered = filtered.Where(d => d.Discount.EndDate.HasValue && d.Discount.EndDate.Value < DateTime.UtcNow);
            }
            else if (SelectedFilter == "Inactive")
            {
                filtered = filtered.Where(d => !d.Discount.IsActive);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(d =>
                    d.Discount.Code.ToLower().Contains(searchLower) ||
                    d.Discount.Name.ToLower().Contains(searchLower) ||
                    (d.Discount.Description?.ToLower().Contains(searchLower) ?? false));
            }

            Discounts = new ObservableCollection<SelectableDiscount>(filtered);
            TotalDiscounts = Discounts.Count;
        }

        private async Task LoadDiscountsFromBackendAsync()
        {
            try
            {
                IsLoading = true;
                var discounts = await _discountRepository.GetAllAsync();

                _allDiscounts = discounts.Select(d => new SelectableDiscount(d)).ToList();
                ApplyFilters();
                TotalPages = 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading discounts: {ex.Message}");
                _allDiscounts = new List<SelectableDiscount>();
                ApplyFilters();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public partial class SelectableDiscount : ObservableObject
    {
        public Discount Discount { get; }

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isCheckboxVisible;

        public SelectableDiscount(Discount discount)
        {
            Discount = discount;
        }
    }
}
