using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Core.Interfaces.Services;

namespace MyShop.App.Views.Dialogs
{
    public sealed partial class CreateOrderDialog : ContentDialog
    {
        private readonly OrderViewModel _viewModel;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDiscountRepository _discountRepository;
        private readonly ObservableCollection<OrderItem> _orderItems;
        private ObservableCollection<Product> _availableProducts;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Discount> _discounts;

        public CreateOrderDialog(OrderViewModel viewModel)
        {
            this.InitializeComponent();
            _viewModel = viewModel;
            _customerRepository = App.Current.GetService<ICustomerRepository>();
            _discountRepository = App.Current.GetService<IDiscountRepository>();
            _orderItems = new ObservableCollection<OrderItem>();
            _availableProducts = new ObservableCollection<Product>();
            _customers = new ObservableCollection<Customer>();
            _discounts = new ObservableCollection<Discount>();

            OrderItemsList.ItemsSource = _orderItems;
            _orderItems.CollectionChanged += (s, e) => UpdateTotals();

            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                // Load products
                var products = await _viewModel.GetAllProductsAsync();
                foreach (var product in products.Where(p => p.IsActive && p.Stock > 0))
                {
                    _availableProducts.Add(product);
                }

                // Load customers
                var customers = await _customerRepository.GetAllAsync();
                _customers.Clear();
                foreach (var customer in customers)
                {
                    _customers.Add(customer);
                }

                // Load active discounts
                var discounts = await _discountRepository.GetActiveDiscountsAsync();
                _discounts.Clear();
                foreach (var discount in discounts)
                {
                    _discounts.Add(discount);
                }

                CustomerComboBox.ItemsSource = _customers;
                DiscountComboBox.ItemsSource = _discounts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void OnAddProductClick(object sender, RoutedEventArgs e)
        {
            // Create a simple ScrollViewer with StackPanel for products
            var stackPanel = new StackPanel { Spacing = 8, Padding = new Thickness(12) };

            foreach (var product in _availableProducts)
            {
                var button = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Tag = product,
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                var nameStack = new StackPanel();
                var nameText = new TextBlock
                {
                    Text = product.Name,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                };
                var skuText = new TextBlock
                {
                    Text = product.Sku,
                    FontSize = 11,
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemControlForegroundBaseMediumBrush"]
                };
                nameStack.Children.Add(nameText);
                nameStack.Children.Add(skuText);
                Grid.SetColumn(nameStack, 0);

                var stockText = new TextBlock
                {
                    Text = $"Stock: {product.Stock}",
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(stockText, 1);

                var priceText = new TextBlock
                {
                    Text = $"${product.Price:F2}",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold
                };
                Grid.SetColumn(priceText, 2);

                grid.Children.Add(nameStack);
                grid.Children.Add(stockText);
                grid.Children.Add(priceText);

                button.Content = grid;
                button.Click += (s, args) =>
                {
                    if (s is Button btn && btn.Tag is Product selectedProduct)
                    {
                        // Check if product already in list
                        var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == selectedProduct.Id);
                        if (existingItem != null)
                        {
                            existingItem.Quantity++;
                            existingItem.Total = existingItem.UnitPrice * existingItem.Quantity;
                        }
                        else
                        {
                            var newItem = new OrderItem
                            {
                                ProductId = selectedProduct.Id,
                                Product = selectedProduct,
                                Quantity = 1,
                                UnitPrice = selectedProduct.Price,
                                Subtotal = selectedProduct.Price,
                                Total = selectedProduct.Price
                            };
                            _orderItems.Add(newItem);
                        }
                        UpdateTotals();

                        // Close the flyout after adding product
                        if (sender is Button addButton && addButton.Flyout is Flyout flyout)
                        {
                            flyout.Hide();
                        }
                    }
                };

                stackPanel.Children.Add(button);
            }

            var scrollViewer = new ScrollViewer
            {
                Content = stackPanel,
                MaxHeight = 400,
                MinWidth = 500
            };

            var flyout = new Flyout
            {
                Content = scrollViewer
            };

            if (sender is Button addProductButton)
            {
                flyout.ShowAt(addProductButton);
            }
        }

        private void OnRemoveItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is OrderItem item)
            {
                _orderItems.Remove(item);
                UpdateTotals();
            }
        }

        private void OnClearCustomerClick(object sender, RoutedEventArgs e)
        {
            CustomerComboBox.SelectedIndex = -1;
        }

        private void OnClearDiscountClick(object sender, RoutedEventArgs e)
        {
            DiscountComboBox.SelectedIndex = -1;
            UpdateTotals();
        }

        private void OnCustomerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Customer selection changed - nothing special needed
        }

        private void OnDiscountSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Recalculate totals when discount changes
            UpdateTotals();
        }

        private void OnQuantityChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.Tag is OrderItem item && !double.IsNaN(args.NewValue))
            {
                item.Quantity = (int)args.NewValue;
                item.Total = item.UnitPrice * item.Quantity;
                UpdateTotals();

                // Force UI refresh by recreating the items list
                var items = _orderItems.ToList();
                _orderItems.Clear();
                foreach (var orderItem in items)
                {
                    _orderItems.Add(orderItem);
                }
            }
        }

        private void UpdateTotals()
        {
            decimal subtotal = _orderItems.Sum(i => i.Total);
            decimal discountAmount = 0;

            // Calculate discount if selected
            if (DiscountComboBox.SelectedItem is Discount discount)
            {
                if (discount.Type == DiscountType.PERCENTAGE)
                {
                    discountAmount = subtotal * (discount.Value / 100);
                    if (discount.MaxDiscount.HasValue && discountAmount > discount.MaxDiscount.Value)
                    {
                        discountAmount = discount.MaxDiscount.Value;
                    }
                }
                else if (discount.Type == DiscountType.FIXED_AMOUNT)
                {
                    discountAmount = discount.Value;
                }
            }

            decimal total = subtotal - discountAmount;

            SubtotalText.Text = $"${subtotal:F2}";
            DiscountText.Text = $"-${discountAmount:F2}";
            TotalText.Text = $"${total:F2}";
        }

        private async void OnCreateOrderClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validate
            if (_orderItems.Count == 0)
            {
                args.Cancel = true;
                var errorDialog = new ContentDialog
                {
                    Title = "Validation Error",
                    Content = "Please add at least one product to the order.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                return;
            }

            // Create order
            var newOrder = new Order
            {
                CustomerId = (CustomerComboBox.SelectedItem as Customer)?.Id,
                DiscountId = (DiscountComboBox.SelectedItem as Discount)?.Id,
                Notes = NotesTextBox.Text,
                OrderItems = _orderItems.ToList()
            };

            var createdOrder = await _viewModel.CreateOrderAsync(newOrder);
            if (createdOrder == null)
            {
                args.Cancel = true;
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = _viewModel.ErrorMessage ?? "Failed to create order",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}
