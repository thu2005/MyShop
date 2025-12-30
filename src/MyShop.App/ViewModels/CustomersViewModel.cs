using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.App.ViewModels
{
    public partial class CustomersViewModel : ObservableObject
    {
        private readonly ICustomerRepository _customerRepository;
        private List<SelectableCustomer> _allCustomers = new();

        [ObservableProperty]
        private ObservableCollection<SelectableCustomer> _customers = new();
        
        [ObservableProperty]
        private SelectableCustomer? _selectedCustomer;
        
        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTimeOffset? _fromDate;

        [ObservableProperty]
        private DateTimeOffset? _toDate;

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
        private int _totalCustomers;
        
        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _pageSize = 20;

        public List<int> AvailablePageSizes { get; } = new List<int> { 10, 20, 50, 100 };

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public CustomersViewModel(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
            _ = LoadCustomersFromBackendAsync();
        }

        partial void OnIsAllSelectedChanged(bool value)
        {
            foreach (var customer in Customers)
            {
                customer.IsSelected = value;
            }
        }

        partial void OnIsSelectionModeChanged(bool value)
        {
            // Update visibility for all items
            foreach (var customer in _allCustomers)
            {
                customer.IsCheckboxVisible = value;
                if (!value) customer.IsSelected = false; // Reset selection when exiting mode
            }
            
            // Also update visible collection if different references
            foreach (var customer in Customers)
            {
                customer.IsCheckboxVisible = value;
                if (!value) customer.IsSelected = false;
            }

            if (!value) IsAllSelected = false;
        }

        partial void OnFromDateChanged(DateTimeOffset? value)
        {
            CurrentPage = 1;
            ApplyFilters();
        }

        partial void OnToDateChanged(DateTimeOffset? value)
        {
            CurrentPage = 1;
            ApplyFilters();
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            UpdatePagination();
        }

        [RelayCommand]
        private async Task LoadCustomersAsync()
        {
            await LoadCustomersFromBackendAsync();
        }

        [RelayCommand]
        private void Export()
        {
            // TODO: Implement export logic
        }

        [RelayCommand]
        private async Task AddCustomer(Customer customer)
        {
             if (customer == null) return;

             try
             {
                 IsLoading = true;
                 var createdCustomer = await _customerRepository.AddAsync(customer);
                 _allCustomers.Insert(0, new SelectableCustomer(createdCustomer));
                 TotalCustomers = _allCustomers.Count;
                 ApplyFilters();
             }
             catch (Exception ex)
             {
                 System.Diagnostics.Debug.WriteLine($"Error creating customer: {ex.Message}");
             }
             finally
             {
                 IsLoading = false;
             }
        }

        [RelayCommand]
        private async Task UpdateCustomer(Customer customer)
        {
             if (customer == null) return;

             try
             {
                 IsLoading = true;
                 await _customerRepository.UpdateAsync(customer);
                 
                 var index = _allCustomers.FindIndex(c => c.Customer.Id == customer.Id);
                 if (index != -1)
                 {
                     // Preserve selection state if needed, currently we just replace the wrapper
                     bool wasSelected = _allCustomers[index].IsSelected;
                     var wrapper = new SelectableCustomer(customer) { IsSelected = wasSelected };
                     _allCustomers[index] = wrapper;
                     ApplyFilters();
                 }
             }
             catch (Exception ex)
             {
                 System.Diagnostics.Debug.WriteLine($"Error updating customer: {ex.Message}");
             }
             finally
             {
                 IsLoading = false;
             }
        }

        [RelayCommand]
        private async Task DeleteCustomer(Customer customer)
        {
            if (customer == null) return;

            try
            {
                IsLoading = true;
                await _customerRepository.DeleteAsync(customer.Id);
                
                var item = _allCustomers.FirstOrDefault(c => c.Customer.Id == customer.Id);
                if (item != null)
                {
                    _allCustomers.Remove(item);
                    TotalCustomers = _allCustomers.Count;
                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting customer: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteSelected()
        {
            var selected = _customers.Where(c => c.IsSelected).ToList();
            if (!selected.Any()) return;

            IsLoading = true;
            try
            {
                foreach (var item in selected)
                {
                    await _customerRepository.DeleteAsync(item.Customer.Id);
                    _allCustomers.Remove(item);
                }
                TotalCustomers = _allCustomers.Count;
                ApplyFilters();
                IsAllSelected = false; // Reset header checkbox
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting selected customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
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

        partial void OnSearchTextChanged(string value)
        {
            CurrentPage = 1;
            ApplyFilters();
        }

        partial void OnSelectedFilterChanged(string value)
        {
            CurrentPage = 1;
            ApplyFilters();
        }

        private List<SelectableCustomer> _filteredCustomers = new();

        private void ApplyFilters()
        {
            var filtered = _allCustomers.AsEnumerable();

            // Filter by member status
            if (SelectedFilter == "Members")
            {
                filtered = filtered.Where(c => c.Customer.IsMember);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(c =>
                    c.Customer.Name.ToLower().Contains(searchLower) ||
                    (c.Customer.Email?.ToLower().Contains(searchLower) ?? false) ||
                    c.Customer.Phone.Contains(searchLower));
            }

            // Filter by date range
            if (FromDate.HasValue)
            {
                var fromDateOnly = FromDate.Value.Date;
                filtered = filtered.Where(c => c.Customer.CreatedAt.Date >= fromDateOnly);
            }

            if (ToDate.HasValue)
            {
                var toDateOnly = ToDate.Value.Date;
                filtered = filtered.Where(c => c.Customer.CreatedAt.Date <= toDateOnly);
            }

            _filteredCustomers = filtered.OrderByDescending(c => c.Customer.CreatedAt).ToList();
            TotalCustomers = _filteredCustomers.Count;
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            TotalPages = (int)Math.Ceiling((double)TotalCustomers / PageSize);
            if (TotalPages == 0) TotalPages = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pagedCustomers = _filteredCustomers
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Customers = new ObservableCollection<SelectableCustomer>(pagedCustomers);

            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
        }

        private async Task LoadCustomersFromBackendAsync()
        {
            try
            {
                IsLoading = true;
                var (customers, total) = await _customerRepository.GetCustomersAsync(
                    page: 1,
                    pageSize: 10000, // Load all, paginate locally
                    searchText: null,
                    isMember: null
                );

                _allCustomers = customers.Select(c => new SelectableCustomer(c)).ToList();
                TotalCustomers = _allCustomers.Count;
                ApplyFilters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
                _allCustomers = new List<SelectableCustomer>();
                ApplyFilters();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
