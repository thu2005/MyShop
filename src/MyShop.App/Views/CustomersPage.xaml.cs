using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;

namespace MyShop.App.Views
{
    public sealed partial class CustomersPage : Page
    {
        public CustomersViewModel ViewModel { get; }

        public CustomersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.GetService<CustomersViewModel>();
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

            stackPanel.Children.Add(nameBox);
            stackPanel.Children.Add(phoneBox);
            stackPanel.Children.Add(emailBox);
            stackPanel.Children.Add(addressBox);
            stackPanel.Children.Add(memberSwitch);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newCustomer = new MyShop.Core.Models.Customer
                {
                    Name = nameBox.Text,
                    Phone = phoneBox.Text,
                    Email = emailBox.Text,
                    IsMember = memberSwitch.IsOn
                };

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
    }
}
