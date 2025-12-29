using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.Models;
using MyShop.App.Services;
using MyShop.App.ViewModels;

namespace MyShop.App.Views
{
    public sealed partial class CustomersPage : Page
    {
        public CustomersViewModel ViewModel { get; }
        private readonly IDraftService _draftService;
        private const string CUSTOMER_DRAFT_KEY = "CreateCustomer_Draft";
        private CancellationTokenSource _customerAutoSaveCts;

        public CustomersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.GetService<CustomersViewModel>();
            _draftService = App.Current.GetService<IDraftService>();
        }

        private async void NewCustomerButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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

            // Custom styling for ToggleSwitch
            var colorOn = Windows.UI.Color.FromArgb(255, 0, 63, 98); // #003F62
            var colorHover = Windows.UI.Color.FromArgb(255, 0, 79, 122); // #004F7A (Lighter)
            var colorPressed = Windows.UI.Color.FromArgb(255, 0, 47, 74); // #002F4A (Darker)

            memberSwitch.Resources["ToggleSwitchFillOn"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorOn);
            memberSwitch.Resources["ToggleSwitchFillOnPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorHover);
            memberSwitch.Resources["ToggleSwitchFillOnPressed"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorPressed);

            // Load Draft
            if (_draftService.HasDraft(CUSTOMER_DRAFT_KEY))
            {
                var draft = _draftService.GetDraft<CustomerDraft>(CUSTOMER_DRAFT_KEY);
                if (draft != null)
                {
                    nameBox.Text = draft.Name ?? "";
                    phoneBox.Text = draft.Phone ?? "";
                    emailBox.Text = draft.Email ?? "";
                    addressBox.Text = draft.Address ?? "";
                    memberSwitch.IsOn = draft.IsMember;
                }
            }

            stackPanel.Children.Add(nameBox);
            stackPanel.Children.Add(phoneBox);
            stackPanel.Children.Add(emailBox);
            stackPanel.Children.Add(addressBox);
            stackPanel.Children.Add(memberSwitch);

            dialog.Content = stackPanel;

            // Auto-Save Logic
            async void OnFieldChanged(object s, object args)
            {
                _customerAutoSaveCts?.Cancel();
                _customerAutoSaveCts = new CancellationTokenSource();
                var token = _customerAutoSaveCts.Token;

                try
                {
                    await Task.Delay(2000, token);
                    if (token.IsCancellationRequested) return;

                    var draft = new CustomerDraft
                    {
                        Name = nameBox.Text,
                        Phone = phoneBox.Text,
                        Email = emailBox.Text,
                        Address = addressBox.Text,
                        IsMember = memberSwitch.IsOn,
                        SavedAt = DateTime.Now
                    };

                    _draftService.SaveDraft(CUSTOMER_DRAFT_KEY, draft);
                    System.Diagnostics.Debug.WriteLine($"💾 Customer Draft saved at {DateTime.Now:HH:mm:ss}");
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to save customer draft: {ex.Message}");
                }
            }

            nameBox.TextChanged += OnFieldChanged;
            phoneBox.TextChanged += OnFieldChanged;
            emailBox.TextChanged += OnFieldChanged;
            addressBox.TextChanged += OnFieldChanged;
            memberSwitch.Toggled += OnFieldChanged;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newCustomer = new MyShop.Core.Models.Customer
                {
                    Name = nameBox.Text,
                    Phone = phoneBox.Text,
                    Email = emailBox.Text,
                    IsMember = memberSwitch.IsOn,
                    Address = addressBox.Text,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Clear draft on success
                _draftService.ClearDraft(CUSTOMER_DRAFT_KEY);

                ViewModel.AddCustomerCommand.Execute(newCustomer);
            }
        }

        private async void EditCustomer_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is SelectableCustomer wrapper)
            {
                var customer = wrapper.Customer;
                var dialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Edit Customer",
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    PrimaryButtonStyle = this.Resources["DialogPrimaryButtonStyle"] as Style
                };

                var stackPanel = new StackPanel { Spacing = 16, Width = 400 };
                var nameBox = new TextBox { Header = "Customer Name", PlaceholderText = "Enter full name", Text = customer.Name ?? "" };
                var phoneBox = new TextBox { Header = "Phone Number", PlaceholderText = "Enter phone number", Text = customer.Phone ?? "" };
                var emailBox = new TextBox { Header = "Email", PlaceholderText = "Enter email address", Text = customer.Email ?? "" };
                var addressBox = new TextBox { Header = "Address", PlaceholderText = "Enter address (Optional)", AcceptsReturn = true, Height = 80, Text = customer.Address ?? "" };
                
                var memberSwitch = new ToggleSwitch 
                { 
                    Header = "Membership Status", 
                    OffContent = "Standard", 
                    OnContent = "Member", 
                    IsOn = customer.IsMember
                };

                // Custom styling for ToggleSwitch (Same as Add)
                var colorOn = Windows.UI.Color.FromArgb(255, 0, 63, 98); // #003F62
                var colorHover = Windows.UI.Color.FromArgb(255, 0, 79, 122); // #004F7A
                var colorPressed = Windows.UI.Color.FromArgb(255, 0, 47, 74); // #002F4A

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
                    // Update properties on a valid object
                    // We create a new object to pass to ViewModel, preserving ID
                    var updatedCustomer = new MyShop.Core.Models.Customer
                    {
                        Id = customer.Id,
                        Name = nameBox.Text,
                        Phone = phoneBox.Text,
                        Email = emailBox.Text,
                        Address = addressBox.Text,
                        IsMember = memberSwitch.IsOn,
                        MemberSince = customer.MemberSince, // Preserve
                        TotalSpent = customer.TotalSpent,   // Preserve
                        CreatedAt = customer.CreatedAt,     // Preserve
                        UpdatedAt = DateTime.Now,
                        Notes = customer.Notes
                    };

                    ViewModel.UpdateCustomerCommand.Execute(updatedCustomer);
                }
            }
        }

        private async void DeleteCustomer_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is SelectableCustomer wrapper)
            {
                var customer = wrapper.Customer;
                var dialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Delete Customer",
                    Content = $"Are you sure you want to permanently delete {customer.Name}?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    ViewModel.DeleteCustomerCommand.Execute(customer);
                }
            }
        }

        private async void DeleteSelected_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Case 1: Enter Selection Mode
            if (!ViewModel.IsSelectionMode)
            {
                ViewModel.IsSelectionMode = true;
                return;
            }

            // Case 2: Already in Selection Mode
            var selectedCount = ViewModel.Customers.Count(c => c.IsSelected);
            
            // If nothing selected, just exit selection mode (Cancel)
            if (selectedCount == 0)
            {
                ViewModel.IsSelectionMode = false;
                return;
            }

            // If has items, confirm delete
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Delete Customers",
                Content = $"Are you sure you want to delete {selectedCount} selected customer(s)? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.DeleteSelectedCommand.Execute(null);
                ViewModel.IsSelectionMode = false; // Exit mode after delete
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                if (item is SelectableCustomer s) s.IsSelected = true;
            }
            foreach (var item in e.RemovedItems)
            {
                if (item is SelectableCustomer s) s.IsSelected = false;
            }
        }

        private void FilterRadio_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.Tag is string filterValue)
            {
                ViewModel.SelectedFilter = filterValue;
            }
        }

        private void OnFirstPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToFirstPage();
        }

        private void OnPreviousPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToPreviousPage();
        }

        private void OnNextPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToNextPage();
        }

        private void OnLastPageClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToLastPage();
        }
    }
}
