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
    public enum PriceFilter
    {
        All,
        Low,
        Medium,
        High
    }

    public class OrderViewModel : ViewModelBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private List<Order> _allOrders;
        private ObservableCollection<Order> _orders;
        private OrderStatus? _selectedStatus;
        private PriceFilter _selectedPriceFilter = PriceFilter.All;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalOrders = 0;
        private int _pageSize = 20;

        public List<int> AvailablePageSizes { get; } = new List<int> { 10, 20, 50, 100 };

        public OrderViewModel(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _orders = new ObservableCollection<Order>();
            _allOrders = new List<Order>();

            _ = LoadOrdersAsync();
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public OrderStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    _ = FilterOrdersAsync();
                }
            }
        }

        public PriceFilter SelectedPriceFilter
        {
            get => _selectedPriceFilter;
            set
            {
                if (SetProperty(ref _selectedPriceFilter, value))
                {
                    _ = FilterOrdersAsync();
                }
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    _ = FilterOrdersAsync();
                }
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    _ = FilterOrdersAsync();
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

        public int TotalOrders
        {
            get => _totalOrders;
            set => SetProperty(ref _totalOrders, value);
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

        public async Task LoadOrdersAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                // Load orders and total count from server
                var ordersTask = _orderRepository.GetAllAsync();
                var countTask = _orderRepository.CountAsync();
                
                await Task.WhenAll(ordersTask, countTask);
                
                var orders = await ordersTask;
                var totalCount = await countTask;

                _allOrders.Clear();
                _allOrders.AddRange(orders.OrderByDescending(o => o.CreatedAt));

                // Set total from server, not from local list
                TotalOrders = totalCount;

                // Apply current filters if any
                if (SelectedStatus.HasValue || SelectedPriceFilter != PriceFilter.All || StartDate.HasValue || EndDate.HasValue)
                {
                    UpdatePagination();
                }
                else
                {
                    UpdatePagination();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load orders: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task FilterOrdersAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                List<Order> filteredOrders;

                // Filter by date range first
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    filteredOrders = await _orderRepository.GetByDateRangeAsync(
                        StartDate.Value.Date,
                        EndDate.Value.Date.AddDays(1).AddSeconds(-1)
                    );
                }
                else if (StartDate.HasValue || EndDate.HasValue)
                {
                    // If only one date is selected, filter in-memory from ALL (needs fetch)
                    var allOrders = await _orderRepository.GetAllAsync();
                    filteredOrders = allOrders.Where(o =>
                    {
                        if (StartDate.HasValue && o.CreatedAt.Date < StartDate.Value.Date)
                            return false;
                        if (EndDate.HasValue && o.CreatedAt.Date > EndDate.Value.Date)
                            return false;
                        return true;
                    }).ToList();
                }
                else
                {
                    filteredOrders = await _orderRepository.GetAllAsync();
                }

                // Then filter by status if selected
                if (SelectedStatus.HasValue)
                {
                    filteredOrders = filteredOrders.Where(o => o.Status == SelectedStatus.Value).ToList();
                }

                // Filter by price
                switch (SelectedPriceFilter)
                {
                    case PriceFilter.Low:
                        filteredOrders = filteredOrders.Where(o => o.Total < 100).ToList();
                        break;
                    case PriceFilter.Medium:
                        filteredOrders = filteredOrders.Where(o => o.Total >= 100 && o.Total <= 500).ToList();
                        break;
                    case PriceFilter.High:
                        filteredOrders = filteredOrders.Where(o => o.Total > 500).ToList();
                        break;
                }

                _allOrders.Clear();
                _allOrders.AddRange(filteredOrders.OrderByDescending(o => o.CreatedAt));

                CurrentPage = 1;
                UpdatePagination();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to filter orders: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error filtering orders: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ClearFiltersAsync()
        {
            SelectedStatus = null;
            StartDate = null;
            EndDate = null;
            await LoadOrdersAsync();
        }

        public async Task SearchOrdersAsync(string query)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                var allOrders = await _orderRepository.GetAllAsync();
                var searchQuery = query.ToLower();

                var filteredOrders = allOrders.Where(o =>
                    o.OrderNumber.ToLower().Contains(searchQuery) ||
                    (o.Customer != null && o.Customer.Name.ToLower().Contains(searchQuery))
                ).ToList();

                _allOrders.Clear();
                _allOrders.AddRange(filteredOrders.OrderByDescending(o => o.CreatedAt));

                CurrentPage = 1;
                UpdatePagination();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to search orders: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error searching orders: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdatePagination()
        {
            // TotalOrders is set from server in LoadOrdersAsync, don't override it here
            TotalPages = (int)Math.Ceiling((double)TotalOrders / PageSize);

            if (TotalPages == 0) TotalPages = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pagedOrders = _allOrders
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Orders = new ObservableCollection<Order>(pagedOrders);

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

        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            try
            {
                return await _orderRepository.GetByIdAsync(orderId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to get order details: {ex.Message}";
                return null;
            }
        }

        public async Task<Order?> CreateOrderAsync(Order newOrder)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;

                var created = await _orderRepository.AddAsync(newOrder);
                
                // Reload orders to update total count and list
                await LoadOrdersAsync();

                return created;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create order: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error creating order: {ex.Message}");
                return null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> UpdateOrderAsync(Order updatedOrder)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;

                await _orderRepository.UpdateAsync(updatedOrder);

                // Reload orders to ensure list is up-to-date
                await LoadOrdersAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update order: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error updating order: {ex.Message}");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;

                await _orderRepository.DeleteAsync(orderId);

                var order = _allOrders.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.CANCELLED;
                }

                UpdatePagination();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to cancel order: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error cancelling order: {ex.Message}");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                return await _productRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load products: {ex.Message}";
                return new List<Product>();
            }
        }

        public string GetStatusDisplayText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.PENDING => "Pending",
                OrderStatus.PROCESSING => "Processing",
                OrderStatus.COMPLETED => "Completed",
                OrderStatus.CANCELLED => "Cancelled",
                _ => status.ToString()
            };
        }

        public string GetStatusColor(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.PENDING => "#FFA500", // Orange
                OrderStatus.PROCESSING => "#4169E1", // Royal Blue
                OrderStatus.COMPLETED => "#32CD32", // Lime Green
                OrderStatus.CANCELLED => "#DC143C", // Crimson
                _ => "#808080" // Gray
            };
        }
    }
}
