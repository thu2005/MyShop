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
        private List<Customer> _allCustomers = new();

        [ObservableProperty]
        private ObservableCollection<Customer> _customers = new();
        
        [ObservableProperty]
        private Customer? _selectedCustomer;
        
        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilter = "All";
        
        [ObservableProperty]
        private int _currentPage = 1;
        
        [ObservableProperty]
        private int _totalPages = 1;
        
        [ObservableProperty]
        private int _totalCustomers;
        
        [ObservableProperty]
        private bool _isLoading;
        
        private int _pageSize = 10;

        public CustomersViewModel(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
            _ = LoadCustomersFromBackendAsync();
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
                 _allCustomers.Insert(0, createdCustomer);
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
            _ = LoadCustomersAsync();
        }

        private void ApplyFilters()
        {
            var filtered = _allCustomers.AsEnumerable();

            // Filter by member status
            if (SelectedFilter == "Members")
            {
                filtered = filtered.Where(c => c.IsMember);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(c =>
                    c.Name.ToLower().Contains(searchLower) ||
                    (c.Email?.ToLower().Contains(searchLower) ?? false) ||
                    c.Phone.Contains(searchLower));
            }

            Customers = new ObservableCollection<Customer>(filtered);
            TotalCustomers = Customers.Count;
        }

        private async Task LoadCustomersFromBackendAsync()
        {
            try
            {
                IsLoading = true;
                var (customers, total) = await _customerRepository.GetCustomersAsync(
                    page: 1,
                    pageSize: 100,
                    searchText: null,
                    isMember: null
                );

                _allCustomers = customers;
                ApplyFilters();
                TotalPages = 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
                _allCustomers = new List<Customer>();
                ApplyFilters();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
