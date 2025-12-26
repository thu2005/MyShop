using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.App.ViewModels;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Models.DTOs;

namespace MyShop.App.Views
{
    public sealed partial class CreateOrderPage : Page
    {
        private OrderViewModel _viewModel;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDiscountRepository _discountRepository;
        private readonly ObservableCollection<OrderItem> _orderItems;
        private ObservableCollection<Product> _availableProducts;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Discount> _discounts;
        private Customer _selectedCustomer;

        public CreateOrderPage()
        {
            this.InitializeComponent();
            _customerRepository = App.Current.GetService<ICustomerRepository>();
            _discountRepository = App.Current.GetService<IDiscountRepository>();
            _orderItems = new ObservableCollection<OrderItem>();
            _availableProducts = new ObservableCollection<Product>();
            _customers = new ObservableCollection<Customer>();
            _discounts = new ObservableCollection<Discount>();

            OrderItemsList.ItemsSource = _orderItems;
            _orderItems.CollectionChanged += (s, e) => UpdateTotals();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is OrderViewModel vm)
            {
                _viewModel = vm;
            }
            else
            {
                _viewModel = App.Current.GetService<OrderViewModel>();
            }

            CurrentDateText.Text = DateTime.Now.ToString("MMM dd, yyyy");
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load products
                var products = await _viewModel.GetAllProductsAsync();
                _availableProducts.Clear();
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

                DiscountComboBox.ItemsSource = _discounts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void OnAddProductClick(object sender, RoutedEventArgs e)
        {
            // Create a temporary Flyout to show product list
            var stackPanel = new StackPanel { Spacing = 8, Padding = new Thickness(12) };

            foreach (var product in _availableProducts)
            {
                var button = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Tag = product,
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                    BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 224, 224, 224)), // #E0E0E0
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

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
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 102, 102)) // #666666
                };
                nameStack.Children.Add(nameText);
                nameStack.Children.Add(skuText);
                Grid.SetColumn(nameStack, 0);

                var stockText = new TextBlock
                {
                    Text = $"Stock: {product.Stock}",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12
                };
                Grid.SetColumn(stockText, 1);

                var priceText = new TextBlock
                {
                    Text = $"{product.Price:N0} ₫",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right
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
                        AddProductToOrder(selectedProduct);
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
                Content = scrollViewer,
                Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom
            };

            if (sender is Button addProductButton)
            {
                addProductButton.Flyout = flyout;
                flyout.ShowAt(addProductButton);
            }
        }

        private void AddProductToOrder(Product product)
        {
            var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
                existingItem.Total = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                var newItem = new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price,
                    Subtotal = product.Price,
                    Total = product.Price
                };
                _orderItems.Add(newItem);
            }
            UpdateTotals();
        }

        private void OnRemoveItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is OrderItem item)
            {
                _orderItems.Remove(item);
                UpdateTotals();
            }
        }

        private void OnQuantityChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.Tag is OrderItem item && !double.IsNaN(args.NewValue) && args.NewValue > 0)
            {
                item.Quantity = (int)args.NewValue;
                item.Total = item.UnitPrice * item.Quantity;
                UpdateTotals();
            }
        }

        private void OnDiscountSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal subtotal = _orderItems.Sum(i => i.Total);
            decimal discountAmount = 0;

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
            if (total < 0) total = 0;

            SubtotalText.Text = $"{subtotal:N0} ₫";
            DiscountAmountText.Text = $"-{discountAmount:N0} ₫";
            TotalText.Text = $"{total:N0} ₫";
        }

        private void OnClearCustomerClick(object sender, RoutedEventArgs e)
        {
            CustomerSuggestBox.Text = string.Empty;
            _selectedCustomer = null;
        }

        private void OnCustomerSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var query = sender.Text.ToLower();
                if (string.IsNullOrWhiteSpace(query))
                {
                    sender.ItemsSource = null;
                }
                else
                {
                    var filtered = _customers.Where(c => 
                        (c.Name != null && c.Name.ToLower().Contains(query)) || 
                        (c.Phone != null && c.Phone.Contains(query)))
                        .Take(10) // Limit results for performance
                        .ToList();
                    sender.ItemsSource = filtered;
                }
            }
        }

        private void OnCustomerSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Customer customer)
            {
                _selectedCustomer = customer;
                sender.Text = customer.Name; // Display name after selection
            }
        }

        private async void OnAddCustomerClick(object sender, RoutedEventArgs e)
        {
             var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "New Customer",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                PrimaryButtonStyle = this.Resources["DialogPrimaryButtonStyle"] as Style
            };

            var stackPanel = new StackPanel { Spacing = 16, Width = 400 };
            var nameBox = new TextBox { Header = "Customer Name", PlaceholderText = "Enter full name" };
            var phoneBox = new TextBox { Header = "Phone Number", PlaceholderText = "Enter phone number" };
            var emailBox = new TextBox { Header = "Email", PlaceholderText = "Enter email address" };
            var addressBox = new TextBox { Header = "Address", PlaceholderText = "Enter address (Optional)", AcceptsReturn = true, Height = 80 };
            var memberSwitch = new ToggleSwitch 
            { 
                Header = "Membership Status", 
                OffContent = "Standard", 
                OnContent = "Member", 
                IsOn = false
            };
            
             // Colors
            var colorOn = Windows.UI.Color.FromArgb(255, 0, 63, 98);
            var colorHover = Windows.UI.Color.FromArgb(255, 0, 79, 122);
            var colorPressed = Windows.UI.Color.FromArgb(255, 0, 47, 74);
            memberSwitch.Resources["ToggleSwitchFillOn"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorOn);
            memberSwitch.Resources["ToggleSwitchFillOnPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorHover);
            memberSwitch.Resources["ToggleSwitchFillOnPressed"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorPressed);


            stackPanel.Children.Add(nameBox);
            stackPanel.Children.Add(phoneBox);
            stackPanel.Children.Add(emailBox);
            stackPanel.Children.Add(addressBox);
            stackPanel.Children.Add(memberSwitch);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newCustomer = new Customer
                {
                    Name = nameBox.Text,
                    Phone = phoneBox.Text,
                    Email = emailBox.Text,
                    IsMember = memberSwitch.IsOn,
                    Notes = "",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                try 
                {
                    var added = await _customerRepository.AddAsync(newCustomer);
                    _customers.Add(added);
                    _selectedCustomer = added;
                    CustomerSuggestBox.Text = added.Name;
                }
                catch (Exception ex)
                {
                     var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Failed to create customer: " + ex.Message,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private async void OnSaveOrderClick(object sender, RoutedEventArgs e)
        {
            if (_orderItems.Count == 0)
            {
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

            var selectedStatus = MyShop.Core.Models.OrderStatus.PENDING;
            if (StatusComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                if (Enum.TryParse<MyShop.Core.Models.OrderStatus>(tag, out var parsed))
                {
                    selectedStatus = parsed;
                }
            }

            var newOrder = new Order
            {
                CustomerId = _selectedCustomer?.Id,
                DiscountId = (DiscountComboBox.SelectedItem as Discount)?.Id,
                Notes = NotesTextBox.Text,
                Status = selectedStatus,
                OrderItems = _orderItems.ToList(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OrderNumber = "ORD-" + DateTime.Now.Ticks.ToString().Substring(10) // Temporary gen
            };

            var createdOrder = await _viewModel.CreateOrderAsync(newOrder);

            if (createdOrder != null)
            {
                Frame.GoBack();
            }
            else
            {
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
