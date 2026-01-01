using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using MyShop.App.ViewModels;
using MyShop.App.Services;
using MyShop.App.Models;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using MyShop.Core.Models.DTOs;

namespace MyShop.App.Views
{
    public class CreateOrderPageNavigationParams
    {
        public OrderViewModel ViewModel { get; set; }
        public int? OrderIdToView { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsEditMode { get; set; }
    }

    public sealed partial class CreateOrderPage : Page
    {
        private const string DRAFT_KEY = "CreateOrder_Draft";
        private const int AUTO_SAVE_DELAY_MS = 2000;
        
        private OrderViewModel _viewModel;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDiscountRepository _discountRepository;
        private readonly ISessionManager _sessionManager;
        private readonly IDraftService _draftService;
        private readonly ObservableCollection<OrderItem> _orderItems;
        private ObservableCollection<Product> _availableProducts;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Discount> _discounts;

        private Customer _selectedCustomer;
        private Discount _selectedDiscount;
        private int? _editingOrderId;
        private DateTime _originalCreatedAt;
        private string _originalOrderNumber;
        
        private CancellationTokenSource _autoSaveCts;
        private bool _isLoadingDraft = false;
        private Order _loadedOrder;

        public CreateOrderPage()
        {
            this.InitializeComponent();
            _customerRepository = App.Current.GetService<ICustomerRepository>();
            _discountRepository = App.Current.GetService<IDiscountRepository>();
            _sessionManager = App.Current.GetService<ISessionManager>();
            _draftService = App.Current.GetService<IDraftService>();
            _orderItems = new ObservableCollection<OrderItem>();
            _availableProducts = new ObservableCollection<Product>();
            _customers = new ObservableCollection<Customer>();
            _discounts = new ObservableCollection<Discount>();

            OrderItemsList.ItemsSource = _orderItems;
            _orderItems.CollectionChanged += (s, e) =>
            {
                UpdateTotals();
                if (!_isLoadingDraft) OnFieldChanged(s, e);
            };
            
            // Hide commission for Admin
            SetCommissionVisibility();
            
            // Attach auto-save events
            AttachAutosaveEvents();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            OrderViewModel vm = null;
            int? orderId = null;
            bool isReadOnly = false;
            bool isEditMode = false;

            if (e.Parameter is OrderViewModel v)
            {
                vm = v;
            }
            else if (e.Parameter is CreateOrderPageNavigationParams args)
            {
                vm = args.ViewModel;
                orderId = args.OrderIdToView;
                isReadOnly = args.IsReadOnly;
                isEditMode = args.IsEditMode;
            }

            _viewModel = vm ?? App.Current.GetService<OrderViewModel>();

            CurrentDateText.Text = DateTime.Now.ToString("MMM dd, yyyy");
            await LoadDataAsync();

            if (orderId.HasValue)
            {
               if (isEditMode)
               {
                   _editingOrderId = orderId.Value;
                   PageTitleText.Text = "Edit Order";
                   SaveButton.Content = new TextBlock { Text = "Update Order", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
               }

               await LoadOrderForView(orderId.Value);
            }
            else
            {
                // Load draft only when creating new order (not editing)
                await LoadDraft();
            }
            
            if (isReadOnly)
            {
                SetReadOnlyMode();
            }
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
            // Check license before allowing order modification (adding products)
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            bool isNew = !_editingOrderId.HasValue;
            string feature = isNew ? "CreateOrder" : "EditOrder";

            if (!licenseService.IsFeatureAllowed(feature))
            {
                await ShowTrialExpiredDialog(isNew ? "Create Order" : "Edit Order");
                return;
            }

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
                        totalTextBlock.Text = $"${item.Total:N2}";
                    }
                }
                
                UpdateTotals();
                if (!_isLoadingDraft) OnFieldChanged(sender, args);
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

            listView.ItemClick += async (s, args) =>
            {
                if (args.ClickedItem is Discount selected)
                {
                    // Check if discount is member-only and customer is not a member
                    if (selected.MemberOnly && (_selectedCustomer == null || !_selectedCustomer.IsMember))
                    {
                        dialog.Hide();

                        var warningDialog = new ContentDialog
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "Member Only Discount",
                            Content = "This discount is only available for members. Please select a member customer first.",
                            CloseButtonText = "OK"
                        };
                        await warningDialog.ShowAsync();
                        return;
                    }

                    // Check minimum purchase requirement
                    decimal currentSubtotal = _orderItems.Sum(i => i.Total);
                    if (selected.MinPurchase.HasValue && currentSubtotal < selected.MinPurchase.Value)
                    {
                        dialog.Hide();

                        var warningDialog = new ContentDialog
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "Minimum Purchase Required",
                            Content = $"This discount requires a minimum purchase of ${selected.MinPurchase.Value:N2}.\nYour current subtotal is ${currentSubtotal:N2}.",
                            CloseButtonText = "OK"
                        };
                        await warningDialog.ShowAsync();
                        return;
                    }

                    // Check wholesale minimum quantity
                    if (selected.Type == DiscountType.WHOLESALE_DISCOUNT && selected.WholesaleMinQty.HasValue)
                    {
                        int totalQuantity = _orderItems.Sum(i => i.Quantity);
                        if (totalQuantity < selected.WholesaleMinQty.Value)
                        {
                            dialog.Hide();

                            var warningDialog = new ContentDialog
                            {
                                XamlRoot = this.XamlRoot,
                                Title = "Minimum Quantity Required",
                                Content = $"This wholesale discount requires a minimum quantity of {selected.WholesaleMinQty.Value} items.\nYour current total quantity is {totalQuantity}.",
                                CloseButtonText = "OK"
                            };
                            await warningDialog.ShowAsync();
                            return;
                        }
                    }

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
                // Debug log
                System.Diagnostics.Debug.WriteLine($"Discount: {_selectedDiscount.Name}, Type: {_selectedDiscount.Type}, Value: {_selectedDiscount.Value}, Subtotal: {subtotal}");

                // Check minimum purchase requirement
                bool meetsMinPurchase = !_selectedDiscount.MinPurchase.HasValue || subtotal >= _selectedDiscount.MinPurchase.Value;

                if (meetsMinPurchase)
                {
                    switch (_selectedDiscount.Type)
                    {
                        case DiscountType.PERCENTAGE:
                        case DiscountType.MEMBER_DISCOUNT:
                            // Value is stored as percentage (e.g., 30 for 30%)
                            discountAmount = subtotal * _selectedDiscount.Value / 100m;
                            System.Diagnostics.Debug.WriteLine($"PERCENTAGE: {subtotal} * {_selectedDiscount.Value} / 100 = {discountAmount}");
                            // Apply max discount cap if specified
                            if (_selectedDiscount.MaxDiscount.HasValue && discountAmount > _selectedDiscount.MaxDiscount.Value)
                            {
                                discountAmount = _selectedDiscount.MaxDiscount.Value;
                            }
                            break;

                        case DiscountType.FIXED_AMOUNT:
                            discountAmount = _selectedDiscount.Value;
                            break;

                        case DiscountType.WHOLESALE_DISCOUNT:
                            // Check if total quantity meets wholesale minimum
                            int totalQuantity = _orderItems.Sum(i => i.Quantity);
                            if (_selectedDiscount.WholesaleMinQty.HasValue && totalQuantity >= _selectedDiscount.WholesaleMinQty.Value)
                            {
                                // Value is stored as percentage (e.g., 30 for 30%)
                                discountAmount = subtotal * _selectedDiscount.Value / 100m;
                                if (_selectedDiscount.MaxDiscount.HasValue && discountAmount > _selectedDiscount.MaxDiscount.Value)
                                {
                                    discountAmount = _selectedDiscount.MaxDiscount.Value;
                                }
                            }
                            break;

                        case DiscountType.BUY_X_GET_Y:
                            // This type needs product-level calculation, for now apply fixed value
                            // TODO: Implement proper Buy X Get Y logic at product level
                            discountAmount = _selectedDiscount.Value;
                            break;
                    }
                }
            }

            decimal total = subtotal - discountAmount;
            if (total < 0) total = 0;

            // Calculate commission based on subtotal (real value of goods sold)
            // 5% for orders >= $1000, 3% otherwise
            decimal commissionRate = subtotal >= 1000 ? 0.05m : 0.03m;
            decimal commissionAmount = subtotal * commissionRate;

            SubtotalText.Text = $"${subtotal:N2}";
            DiscountAmountText.Text = $"-${discountAmount:N2}";
            TotalAmountText.Text = $"${total:N2}";
            
            // Update commission display
            CommissionRateText.Text = $"({commissionRate * 100:N0}%)";
            CommissionAmountText.Text = $"${commissionAmount:N2}";
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
            // Check license before allowing order modification (adding/changing customer)
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            bool isNew = !_editingOrderId.HasValue;
            string feature = isNew ? "CreateOrder" : "EditOrder";

            if (!licenseService.IsFeatureAllowed(feature))
            {
                await ShowTrialExpiredDialog(isNew ? "Create Order" : "Edit Order");
                return;
            }

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
            // Check license before saving order
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            bool isNew = !_editingOrderId.HasValue;
            string feature = isNew ? "CreateOrder" : "EditOrder";

            if (!licenseService.IsFeatureAllowed(feature))
            {
                await ShowTrialExpiredDialog(isNew ? "Create Order" : "Edit Order");
                return;
            }

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

            // Validate member-only discount
            if (_selectedDiscount != null && _selectedDiscount.MemberOnly)
            {
                if (_selectedCustomer == null || !_selectedCustomer.IsMember)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "The selected discount is only available for members. Please select a member customer or remove the discount.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }
            }

            var newOrder = new Order
            {
                Id = _editingOrderId ?? 0,
                UserId = _sessionManager.CurrentUser?.Id ?? 0, // Track who created the order for KPI
                CustomerId = _selectedCustomer?.Id,
                DiscountId = _selectedDiscount?.Id,
                Notes = NotesTextBox.Text,
                OrderItems = _orderItems.ToList(),
                CreatedAt = _editingOrderId.HasValue ? _originalCreatedAt : DateTime.Now,
                UpdatedAt = DateTime.Now,
                OrderNumber = _editingOrderId.HasValue ? _originalOrderNumber : "ORD-" + DateTime.Now.Ticks.ToString().Substring(10)
            };

            // Only set status when editing (for UpdateOrder mutation)
            if (_editingOrderId.HasValue)
            {
                var selectedStatus = MyShop.Core.Models.OrderStatus.PENDING;
                if (StatusComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
                {
                    if (Enum.TryParse<MyShop.Core.Models.OrderStatus>(tag, out var parsed))
                    {
                        selectedStatus = parsed;
                    }
                }
                newOrder.Status = selectedStatus;
            }

            if (_editingOrderId.HasValue)
            {
                 var success = await _viewModel.UpdateOrderAsync(newOrder);
                 if (success)
                 {
                     Frame.GoBack();
                 }
                 else
                 {
                     var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = _viewModel.ErrorMessage ?? "Failed to update order",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                 }
            }
            else
            {
                var createdOrder = await _viewModel.CreateOrderAsync(newOrder);

                if (createdOrder != null)
                {
                    ClearDraft(); // Clear draft after successful save
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

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void OnStatusChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                StatusBadgeText.Text = tag;

                // Colors matched from OrderStatusColorConverters.cs
                // PENDING (Orange)
                var bgColor = Windows.UI.Color.FromArgb(32, 255, 159, 0); 
                var fgColor = Windows.UI.Color.FromArgb(255, 255, 159, 0);

                switch (tag)
                {
                    case "PROCESSING": // Blue
                        bgColor = Windows.UI.Color.FromArgb(32, 0, 120, 212);
                        fgColor = Windows.UI.Color.FromArgb(255, 0, 120, 212);
                        break;
                    case "COMPLETED": // Green
                        bgColor = Windows.UI.Color.FromArgb(32, 5, 150, 105);
                        fgColor = Windows.UI.Color.FromArgb(255, 5, 150, 105);
                        break;
                     case "CANCELLED": // Red
                        bgColor = Windows.UI.Color.FromArgb(32, 220, 53, 69);
                        fgColor = Windows.UI.Color.FromArgb(255, 220, 53, 69);
                        break;
                }

                StatusBadgeBorder.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(bgColor);
                StatusBadgeText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(fgColor);
            }
        }
        private async System.Threading.Tasks.Task LoadOrderForView(int orderId)
        {
            var order = await _viewModel.GetOrderDetailsAsync(orderId);
            if (order == null) return;
            
            _loadedOrder = order;

            _originalCreatedAt = order.CreatedAt;
            _originalOrderNumber = order.OrderNumber;

            // Set Customer
            _selectedCustomer = order.Customer;
            CustomerSuggestBox.Text = _selectedCustomer?.Name ?? "Unknown";

            // Set Notes
            NotesTextBox.Text = order.Notes ?? "";

            // Set Discount
            if (order.Discount != null)
            {
                _selectedDiscount = order.Discount;
                UpdateDiscountUI();
            }

            // Set Items
            _orderItems.Clear();
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    _orderItems.Add(item);
                }
            }

            // Set Status
            foreach(ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Tag is string tag && tag == order.Status.ToString())
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // Set Date Text
            CurrentDateText.Text = order.CreatedAt.ToString("MMM dd, yyyy");
            
            // Update Totals
            UpdateTotals();
        }

        private void SetReadOnlyMode()
        {
            PageTitleText.Text = "Order Details";
            
            // Disable Header Controls
            SaveButton.Visibility = Visibility.Collapsed;
            StatusComboBox.IsEnabled = false;

            // Disable Customer Controls
            CustomerSuggestBox.IsEnabled = false;
            AddCustomerButton.Visibility = Visibility.Collapsed;
            ClearCustomerButton.Visibility = Visibility.Collapsed;
            NotesTextBox.IsReadOnly = true;

            // Disable Product Controls
            AddProductButton.Visibility = Visibility.Collapsed;
            SelectDiscountButton.IsEnabled = false;
            RemoveDiscountButton.Visibility = Visibility.Collapsed;

            // Switch Item Template to ReadOnly
            OrderItemsList.ItemTemplate = this.Resources["ReadOnlyOrderItemTemplate"] as DataTemplate;
        }

        private async Task ShowTrialExpiredDialog(string featureName)
        {
            var shell = ShellPage.Instance;
            if (shell != null)
            {
                await shell.ShowTrialExpiredDialog(featureName);
            }
        }

        private void SetCommissionVisibility()
        {
            // Hide commission for Admin, show only for Staff
            var isStaff = _sessionManager.CurrentUser?.Role == UserRole.STAFF;
            CommissionLabel.Visibility = isStaff ? Visibility.Visible : Visibility.Collapsed;
            CommissionPanel.Visibility = isStaff ? Visibility.Visible : Visibility.Collapsed;
        }

        #region Auto-Save Draft

        private void AttachAutosaveEvents()
        {
            NotesTextBox.TextChanged += OnFieldChanged;
            StatusComboBox.SelectionChanged += (s, e) => OnFieldChanged(s, null);
        }

        private void OnFieldChanged(object sender, object e)
        {
            // Don't save draft when editing or viewing existing order, or during draft loading
            if (_editingOrderId.HasValue || _loadedOrder != null || _isLoadingDraft) return;
            
            // Cancel previous auto-save
            _autoSaveCts?.Cancel();
            _autoSaveCts = new CancellationTokenSource();

            // Debounced auto-save
            _ = AutoSaveDraftAsync(_autoSaveCts.Token);
        }

        private async Task AutoSaveDraftAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(AUTO_SAVE_DELAY_MS, cancellationToken);

                var draft = new OrderDraft
                {
                    CustomerId = _selectedCustomer?.Id,
                    CustomerName = _selectedCustomer?.Name,
                    DiscountId = _selectedDiscount?.Id,
                    DiscountCode = _selectedDiscount?.Code,
                    Notes = NotesTextBox.Text,
                    Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag as string,
                    Items = _orderItems.Select(item => new OrderItemDraft
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name ?? "",
                        ProductSku = item.Product?.Sku ?? "",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Total = item.Total
                    }).ToList(),
                    SavedAt = DateTime.Now
                };

                _draftService.SaveDraft(DRAFT_KEY, draft);
                System.Diagnostics.Debug.WriteLine($"💾 Draft saved at {DateTime.Now:HH:mm:ss}");
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save draft: {ex.Message}");
            }
        }

        private async Task LoadDraft()
        {
            try
            {
                if (_editingOrderId.HasValue) return; // Don't load draft when editing
                if (!_draftService.HasDraft(DRAFT_KEY)) return;

                var draft = _draftService.GetDraft<OrderDraft>(DRAFT_KEY);
                if (draft == null) return;

                _isLoadingDraft = true;

                // Restore customer
                if (draft.CustomerId.HasValue)
                {
                    var customer = _customers.FirstOrDefault(c => c.Id == draft.CustomerId.Value);
                    if (customer != null)
                    {
                        _selectedCustomer = customer;
                        CustomerSuggestBox.Text = customer.Name;
                        ClearCustomerButton.Visibility = Visibility.Visible;
                    }
                }

                // Restore discount
                if (draft.DiscountId.HasValue)
                {
                    var discount = _discounts.FirstOrDefault(d => d.Id == draft.DiscountId.Value);
                    if (discount != null)
                    {
                        _selectedDiscount = discount;
                        SelectDiscountButton.Content = $"{discount.Code} - {discount.Name}";
                        RemoveDiscountButton.Visibility = Visibility.Visible;
                    }
                }

                // Restore notes
                NotesTextBox.Text = draft.Notes ?? string.Empty;

                // Restore status
                if (!string.IsNullOrEmpty(draft.Status))
                {
                    foreach (ComboBoxItem item in StatusComboBox.Items)
                    {
                        if (item.Tag as string == draft.Status)
                        {
                            StatusComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Restore order items
                _orderItems.Clear();
                foreach (var draftItem in draft.Items)
                {
                    var product = _availableProducts.FirstOrDefault(p => p.Id == draftItem.ProductId);
                    if (product != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"OrderItem: Product={product?.Name}, MainImage={product?.MainImage ?? "NULL"}");
                        _orderItems.Add(new OrderItem
                        {
                            ProductId = draftItem.ProductId,
                            Product = product,
                            Quantity = draftItem.Quantity,
                            UnitPrice = draftItem.UnitPrice,
                            Subtotal = draftItem.Quantity * draftItem.UnitPrice,
                            Total = draftItem.Total
                        });
                    }
                }

                _isLoadingDraft = false;

                System.Diagnostics.Debug.WriteLine($"✅ Draft restored from {draft.SavedAt:g}");
            }
            catch (Exception ex)
            {
                _isLoadingDraft = false;
                System.Diagnostics.Debug.WriteLine($"Failed to load draft: {ex.Message}");
            }
        }

        private void ClearDraft()
        {
            try
            {
                _draftService.ClearDraft(DRAFT_KEY);
                System.Diagnostics.Debug.WriteLine("🗑️ Draft cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear draft: {ex.Message}");
            }
        }

        #endregion

        private async void OnPrintClick(object sender, RoutedEventArgs e)
        {
            // Get current status from ComboBox
            var currentStatus = MyShop.Core.Models.OrderStatus.PENDING;
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string statusTag)
            {
                if (Enum.TryParse<MyShop.Core.Models.OrderStatus>(statusTag, out var parsed))
                {
                    currentStatus = parsed;
                }
            }

            // Build a temporary Order object from current state if creating new order
            Order orderToPrint;
            
            if (_loadedOrder != null)
            {
                // Use loaded order but update with current items and totals
                orderToPrint = new Order
                {
                    Id = _loadedOrder.Id,
                    OrderNumber = _loadedOrder.OrderNumber,
                    Status = currentStatus,
                    Customer = _selectedCustomer ?? _loadedOrder.Customer,
                    OrderItems = _orderItems.ToList(),
                    Discount = _selectedDiscount,
                    Notes = NotesTextBox.Text,
                    Subtotal = _orderItems.Sum(i => i.Total),
                    DiscountAmount = CalculateDiscountAmount(),
                    TaxAmount = 0,
                    Total = CalculateFinalTotal(),
                    CreatedAt = _loadedOrder.CreatedAt
                };
            }
            else
            {
                // Creating new order - build from current state
                orderToPrint = new Order
                {
                    OrderNumber = "NEW",
                    Status = currentStatus,
                    Customer = _selectedCustomer,
                    OrderItems = _orderItems.ToList(),
                    Discount = _selectedDiscount,
                    Notes = NotesTextBox.Text,
                    Subtotal = _orderItems.Sum(i => i.Total),
                    DiscountAmount = CalculateDiscountAmount(),
                    TaxAmount = 0,
                    Total = CalculateFinalTotal(),
                    CreatedAt = DateTime.Now
                };
            }

            if (orderToPrint.OrderItems == null || !orderToPrint.OrderItems.Any())
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "No Items",
                    Content = "Cannot print an order without items.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
                return;
            }

            await _viewModel.ExportSingleOrderAsync(orderToPrint);
        }

        private decimal CalculateDiscountAmount()
        {
            if (_selectedDiscount == null) return 0;
            var subtotal = _orderItems.Sum(i => i.Total);
            
            // Check minimum purchase requirement
            if (_selectedDiscount.MinPurchase.HasValue && subtotal < _selectedDiscount.MinPurchase.Value)
            {
                return 0; // Discount not applicable
            }
            
            if (_selectedDiscount.Type == DiscountType.PERCENTAGE)
            {
                var discountAmt = subtotal * (_selectedDiscount.Value / 100);
                // Apply max discount cap if specified
                if (_selectedDiscount.MaxDiscount.HasValue && discountAmt > _selectedDiscount.MaxDiscount.Value)
                {
                    discountAmt = _selectedDiscount.MaxDiscount.Value;
                }
                return discountAmt;
            }
            else
            {
                return Math.Min(_selectedDiscount.Value, subtotal);
            }
        }

        private decimal CalculateFinalTotal()
        {
            var subtotal = _orderItems.Sum(i => i.Total);
            var discount = CalculateDiscountAmount();
            return subtotal - discount;
        }

    }
}
