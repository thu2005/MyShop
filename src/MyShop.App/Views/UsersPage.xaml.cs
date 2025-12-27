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
            var nameBox = new TextBox { Header = "Username", Margin = new Thickness(0, 0, 0, 8), IsSpellCheckEnabled = false };
            var emailBox = new TextBox { Header = "Email", Margin = new Thickness(0, 0, 0, 8), IsSpellCheckEnabled = false };
            var passwordBox = new PasswordBox { Header = "Initial Password", Margin = new Thickness(0, 0, 0, 8) };
            var errorText = new TextBlock 
            { 
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed
            };

            var stack = new StackPanel();
            stack.Children.Add(nameBox);
            stack.Children.Add(emailBox);
            stack.Children.Add(passwordBox);
            stack.Children.Add(errorText);

            var dialog = new ContentDialog
            {
                Title = "Add New Staff Member",
                Content = stack,
                PrimaryButtonText = "Create account",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            dialog.Closing += async (s, args) =>
            {
                if (args.Result == ContentDialogResult.Primary)
                {
                    var deferral = args.GetDeferral();
                    try
                    {
                        string username = nameBox.Text.Trim();
                        string email = emailBox.Text.Trim();
                        string password = passwordBox.Password;

                        string? error = null;
                        if (string.IsNullOrWhiteSpace(username)) error = "Username is required.";
                        else if (string.IsNullOrWhiteSpace(email)) error = "Email is required.";
                        else if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) error = "Invalid email format.";
                        else if (password.Length < 6) error = "Password must be at least 6 characters long.";
                        else if (!await ViewModel.IsUsernameAvailableAsync(username)) error = "Username is already taken.";

                        if (error != null)
                        {
                            args.Cancel = true;
                            errorText.Text = error;
                            errorText.Visibility = Visibility.Visible;
                        }
                    }
                    finally
                    {
                        deferral.Complete();
                    }
                }
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var user = new User
                {
                    Username = nameBox.Text.Trim(),
                    Email = emailBox.Text.Trim(),
                    PasswordHash = passwordBox.Password,
                    Role = UserRole.STAFF
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

            var emailBox = new TextBox { Header = "Email", Text = user.Email ?? "", Margin = new Thickness(0, 0, 0, 8), IsSpellCheckEnabled = false };
            var errorText = new TextBlock 
            { 
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed
            };
            
            var activeToggle = new ToggleSwitch 
            { 
                Header = "Account Status", 
                IsOn = user.IsActive,
                OnContent = "Active",
                OffContent = "Inactive"
            };

            var stack = new StackPanel();
            stack.Children.Add(emailBox);
            stack.Children.Add(activeToggle);
            stack.Children.Add(errorText);
            dialog.Content = stack;

            dialog.Closing += (s, args) =>
            {
                if (args.Result == ContentDialogResult.Primary)
                {
                    string email = emailBox.Text.Trim();
                    string? error = null;
                    if (string.IsNullOrWhiteSpace(email)) error = "Email is required.";
                    else if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) error = "Invalid email format.";

                    if (error != null)
                    {
                        args.Cancel = true;
                        errorText.Text = error;
                        errorText.Visibility = Visibility.Visible;
                    }
                }
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                user.Email = emailBox.Text.Trim();
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
