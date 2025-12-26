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

        private async void OnViewOrderClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Order order)
            {
                // Load full order details including order items
                var fullOrder = await ViewModel.GetOrderDetailsAsync(order.Id);
                if (fullOrder != null)
                {
                    var dialog = new Dialogs.OrderDetailDialog(fullOrder);
                    dialog.XamlRoot = this.XamlRoot;
                    await dialog.ShowAsync();
                }
            }
        }

        private async void OnEditOrderClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Order order)
            {
                var dialog = new Dialogs.EditOrderDialog(ViewModel, order);
                dialog.XamlRoot = this.XamlRoot;

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.LoadOrdersAsync();
                }
            }
        }

        private async void OnCancelOrderClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Order order)
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "Cancel Order",
                    Content = $"Are you sure you want to cancel order {order.OrderNumber}?",
                    PrimaryButtonText = "Yes, Cancel",
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
                            Content = "Order cancelled successfully!",
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
                            Content = ViewModel.ErrorMessage ?? "Failed to cancel order",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
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
