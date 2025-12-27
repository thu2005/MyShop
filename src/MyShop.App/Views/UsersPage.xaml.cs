using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.App.ViewModels;
using MyShop.Core.Models;
using System;

namespace MyShop.App.Views
{
    public sealed partial class UsersPage : Page
    {
        public UsersViewModel ViewModel { get; }

        public UsersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<UsersViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadUsersAsync();
        }

        private async void AddStaff_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add New Staff Member",
                PrimaryButtonText = "Create account",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var nameBox = new TextBox { Header = "Username", Margin = new Thickness(0, 0, 0, 8) };
            var emailBox = new TextBox { Header = "Email", Margin = new Thickness(0, 0, 0, 8) };
            var passwordBox = new PasswordBox { Header = "Initial Password", Margin = new Thickness(0, 0, 0, 8) };
            var roleCombo = new ComboBox { Header = "Role", HorizontalAlignment = HorizontalAlignment.Stretch };
            roleCombo.Items.Add(UserRole.STAFF);
            roleCombo.Items.Add(UserRole.ADMIN);
            roleCombo.SelectedIndex = 0;

            var stack = new StackPanel();
            stack.Children.Add(nameBox);
            stack.Children.Add(emailBox);
            stack.Children.Add(passwordBox);
            stack.Children.Add(roleCombo);
            dialog.Content = stack;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var user = new User
                {
                    Username = nameBox.Text,
                    Email = emailBox.Text,
                    PasswordHash = passwordBox.Password,
                    Role = (UserRole)roleCombo.SelectedItem
                };

                await ViewModel.AddUserAsync(user);
            }
        }

        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var user = ((FrameworkElement)sender).Tag as User;
            if (user == null) user = (sender as Button)?.DataContext as User;
            if (user == null) return;

            var dialog = new ContentDialog
            {
                Title = $"Edit User: {user.Username}",
                PrimaryButtonText = "Save changes",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var emailBox = new TextBox { Header = "Email", Text = user.Email ?? "", Margin = new Thickness(0, 0, 0, 8) };
            
            var roleCombo = new ComboBox { Header = "Role", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 8) };
            roleCombo.Items.Add(UserRole.STAFF);
            roleCombo.Items.Add(UserRole.ADMIN);
            roleCombo.SelectedItem = user.Role;

            var activeToggle = new ToggleSwitch 
            { 
                Header = "Account Status", 
                IsOn = user.IsActive,
                OnContent = "Active",
                OffContent = "Inactive"
            };

            var stack = new StackPanel();
            stack.Children.Add(emailBox);
            stack.Children.Add(roleCombo);
            stack.Children.Add(activeToggle);
            dialog.Content = stack;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                user.Email = emailBox.Text;
                user.Role = (UserRole)roleCombo.SelectedItem;
                user.IsActive = activeToggle.IsOn;

                await ViewModel.UpdateUserAsync(user);
            }
        }
        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var user = ((FrameworkElement)sender).Tag as User;
            if (user == null) user = (sender as Button)?.DataContext as User;
            if (user == null) return;

            var dialog = new ContentDialog
            {
                Title = "Delete User",
                Content = $"Are you sure you want to delete user '{user.Username}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteUserAsync(user);
            }
        }
    }
}
