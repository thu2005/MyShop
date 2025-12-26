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
        private Discount _selectedDiscount;

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


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private async void OnAddProductClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Select Product",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                SecondaryButtonStyle = this.Resources["DialogPrimaryButtonStyle"] as Style
            };

            var container = new Grid { Height = 500, Width = 600, RowSpacing = 16 };
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Search Box
            var searchBox = new AutoSuggestBox 
            { 
                PlaceholderText = "Search by Name or SKU...",
                QueryIcon = new SymbolIcon(Symbol.Find),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // ListView
            var listView = new ListView 
            { 
                SelectionMode = ListViewSelectionMode.None,
                IsItemClickEnabled = true,
                ItemTemplate = this.Resources["ProductSelectionTemplate"] as DataTemplate,
                ItemsSource = _availableProducts // Bind to full list initially
            };

            // Search Logic
            searchBox.TextChanged += (s, args) => 
            {
                if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    var query = s.Text.ToLower();
                    if (string.IsNullOrWhiteSpace(query))
                    {
                        listView.ItemsSource = _availableProducts;
                    }
                    else
                    {
                        listView.ItemsSource = _availableProducts.Where(p => 
                            (p.Name != null && p.Name.ToLower().Contains(query)) || 
                            (p.Sku != null && p.Sku.ToLower().Contains(query))
                        ).ToList();
                    }
                }
            };

            // Add Item Logic
            listView.ItemClick += (s, args) =>
            {
                if (args.ClickedItem is Product selectedProduct)
                {
                    AddProductToOrder(selectedProduct);
                    dialog.Hide(); 
                }
            };

            Grid.SetRow(searchBox, 0);
            Grid.SetRow(listView, 1);
            container.Children.Add(searchBox);
            container.Children.Add(listView);

            dialog.Content = container;
            await dialog.ShowAsync();
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
                
                // Directly update the Total TextBlock in the row to avoid re-rendering the whole row (which causes focus loss)
                if (sender.Parent is Grid grid)
                {
                    // Total TextBlock is in Column 4 based on XAML
                    var totalTextBlock = grid.Children.OfType<TextBlock>().FirstOrDefault(t => Grid.GetColumn(t) == 4);
                    if (totalTextBlock != null)
                    {
                        totalTextBlock.Text = $"{item.Total:N0} ₫";
                    }
                }
                
                UpdateTotals();
            }
        }

        private async void OnSelectDiscountClick(object sender, RoutedEventArgs e)
        {
             var dialog = new ContentDialog
            {
                Title = "Select Discount",
                SecondaryButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
                DefaultButton = ContentDialogButton.Secondary,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                SecondaryButtonStyle = this.Resources["DialogPrimaryButtonStyle"] as Style
            };

            var listView = new ListView
            {
                SelectionMode = ListViewSelectionMode.None,
                IsItemClickEnabled = true,
                ItemTemplate = this.Resources["DiscountSelectionTemplate"] as DataTemplate,
                ItemsSource = _discounts,
                MaxHeight = 400
            };

            listView.ItemClick += (s, args) =>
            {
                if (args.ClickedItem is Discount selected)
                {
                    _selectedDiscount = selected;
                    dialog.Hide();
                    UpdateDiscountUI();
                    UpdateTotals();
                }
            };
            
            var container = new Grid();
            container.Children.Add(listView);
            dialog.Content = container;

            await dialog.ShowAsync();
        }

        private void OnRemoveDiscountClick(object sender, RoutedEventArgs e)
        {
            _selectedDiscount = null;
            UpdateDiscountUI();
            UpdateTotals();
        }

        private void UpdateDiscountUI()
        {
            if (_selectedDiscount != null)
            {
                SelectDiscountButton.Content = new TextBlock 
                { 
                    Text = _selectedDiscount.Name, 
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black),
                    VerticalAlignment = VerticalAlignment.Center
                };
                RemoveDiscountButton.Visibility = Visibility.Visible;
            }
            else
            {
                SelectDiscountButton.Content = "Select Discount";
                RemoveDiscountButton.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateTotals()
        {
            decimal subtotal = _orderItems.Sum(i => i.Total);
            decimal discountAmount = 0;

            if (_selectedDiscount != null)
            {
                if (_selectedDiscount.Type == DiscountType.PERCENTAGE)
                {
                    discountAmount = subtotal * (_selectedDiscount.Value / 100);
                    if (_selectedDiscount.MaxDiscount.HasValue && discountAmount > _selectedDiscount.MaxDiscount.Value)
                    {
                        discountAmount = _selectedDiscount.MaxDiscount.Value;
                    }
                }
                else if (_selectedDiscount.Type == DiscountType.FIXED_AMOUNT)
                {
                    discountAmount = _selectedDiscount.Value;
                }
            }

            decimal total = subtotal - discountAmount;
            if (total < 0) total = 0;

            SubtotalText.Text = $"{subtotal:N0} ₫";
            DiscountAmountText.Text = $"-{discountAmount:N0} ₫";
            TotalAmountText.Text = $"{total:N0} ₫";
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
                DiscountId = _selectedDiscount?.Id,
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
