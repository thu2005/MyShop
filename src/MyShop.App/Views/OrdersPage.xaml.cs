using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;
using MyShop.Core.Models;
using System;

namespace MyShop.App.Views
{
    public sealed partial class OrdersPage : Page
    {
        public OrderViewModel ViewModel { get; }

        public OrdersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.GetService<OrderViewModel>();
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel == null) return;
            if (sender is TextBox textBox)
            {
                var query = textBox.Text;
                if (!string.IsNullOrWhiteSpace(query))
                {
                    await ViewModel.SearchOrdersAsync(query);
                }
                else
                {
                    await ViewModel.LoadOrdersAsync();
                }
            }
        }

        private void OnStatusFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var statusTag = selectedItem.Tag as string;
                if (statusTag == "All")
                {
                    ViewModel.SelectedStatus = null;
                }
                else if (Enum.TryParse<OrderStatus>(statusTag, out var status))
                {
                    ViewModel.SelectedStatus = status;
                }
            }
        }

        private void OnPriceFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var priceTag = selectedItem.Tag as string;
                if (priceTag == null) return;

                if (Enum.TryParse<PriceFilter>(priceTag, out var filter))
                {
                    ViewModel.SelectedPriceFilter = filter;
                }
            }
        }

        private void OnAddOrderClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateOrderPage), ViewModel);
        }

        private void OnViewOrderClick(object sender, RoutedEventArgs e)
        {
            // Handle both Button and MenuFlyoutItem triggers
            Order order = null;

            if (sender is Button button && button.Tag is Order o1)
            {
                order = o1;
            }
            else if (sender is MenuFlyoutItem item && item.Tag is Order o2)
            {
                order = o2;
            }

            if (order != null)
            {
                var navParams = new CreateOrderPageNavigationParams 
                { 
                    ViewModel = ViewModel,
                    OrderIdToView = order.Id,
                    IsReadOnly = true
                };
                Frame.Navigate(typeof(CreateOrderPage), navParams);
            }
        }

        private async void OnEditOrderClick(object sender, RoutedEventArgs e)
        {
             // Handle both Button and MenuFlyoutItem triggers
            Order order = null;

            if (sender is Button button && button.Tag is Order o1)
            {
                order = o1;
            }
            else if (sender is MenuFlyoutItem item && item.Tag is Order o2)
            {
                order = o2;
            }

            if (order != null)
            {
                // Block editing completed or cancelled orders
                if (order.Status == OrderStatus.COMPLETED || order.Status == OrderStatus.CANCELLED)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Cannot Edit Order",
                        Content = $"Orders with status '{order.Status}' cannot be edited.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                    return;
                }

                var navParams = new CreateOrderPageNavigationParams 
                { 
                    ViewModel = ViewModel,
                    OrderIdToView = order.Id,
                    IsEditMode = true
                };
                Frame.Navigate(typeof(CreateOrderPage), navParams);
            }
        }

        private async void OnCancelOrderClick(object sender, RoutedEventArgs e)
        {
            // Handle both Button and MenuFlyoutItem triggers
            Order order = null;

            if (sender is Button button && button.Tag is Order o1)
            {
                order = o1;
            }
            else if (sender is MenuFlyoutItem item && item.Tag is Order o2)
            {
                order = o2;
            }

            if (order == null) return;

            // If already cancelled, just inform and return (already deleted)
            if (order.Status == OrderStatus.CANCELLED)
            {
                var infoDialog = new ContentDialog
                {
                    Title = "Order Already Deleted",
                    Content = "This order has already been deleted/cancelled.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await infoDialog.ShowAsync();
                return;
            }

            // Block deleting completed orders
            if (order.Status == OrderStatus.COMPLETED)
            {
                var completedDialog = new ContentDialog
                {
                    Title = "Cannot Delete",
                    Content = "Completed orders cannot be deleted.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await completedDialog.ShowAsync();
                return;
            }

            var confirmDialog = new ContentDialog
            {
                Title = "Delete Order",
                Content = $"Are you sure you want to delete order {order.OrderNumber}?\nThis action will cancel the order.",
                PrimaryButtonText = "Yes, Delete",
                CloseButtonText = "No",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var success = await ViewModel.CancelOrderAsync(order.Id);
                if (success)
                {
                    var successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Order deleted successfully!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = ViewModel.ErrorMessage ?? "Failed to delete order",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private void OnPreviousPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToPreviousPage();
        }

        private void OnNextPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToNextPage();
        }

        private void OnFirstPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToFirstPage();
        }

        private void OnLastPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToLastPage();
        }

        private async void OnStartDateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            ViewModel.StartDate = args.NewDate?.DateTime;

            // Validate: If EndDate exists and is less than StartDate, clear EndDate
            if (args.NewDate.HasValue && EndDatePicker.Date.HasValue)
            {
                if (EndDatePicker.Date.Value.DateTime < args.NewDate.Value.DateTime)
                {
                    EndDatePicker.Date = null;
                    ViewModel.EndDate = null;

                    var dialog = new ContentDialog
                    {
                        Title = "Invalid Date Range",
                        Content = "The 'To Date' cannot be earlier than the 'From Date'. The 'To Date' has been cleared.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
        }

        private async void OnEndDateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            // Validate: If EndDate is less than StartDate, clear EndDate and show error
            if (args.NewDate.HasValue && StartDatePicker.Date.HasValue)
            {
                if (args.NewDate.Value.DateTime < StartDatePicker.Date.Value.DateTime)
                {
                    EndDatePicker.Date = null;
                    ViewModel.EndDate = null;

                    var dialog = new ContentDialog
                    {
                        Title = "Invalid Date Range",
                        Content = "The 'To Date' cannot be earlier than the 'From Date'. Please select a valid date.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                    return;
                }
            }

            ViewModel.EndDate = args.NewDate?.DateTime;
        }
    }
}
