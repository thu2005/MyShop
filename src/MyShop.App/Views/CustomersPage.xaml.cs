using System;
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

        private void FilterRadio_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.Tag is string filterValue)
            {
                ViewModel.SelectedFilter = filterValue;
            }
        }
    }
}
