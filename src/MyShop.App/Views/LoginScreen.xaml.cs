    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using MyShop.App.ViewModels;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    namespace MyShop.App.Views
    {
        public sealed partial class LoginScreen : Page, INotifyPropertyChanged
        {
            public LoginViewModel ViewModel { get; }

            private bool _isPasswordVisible;
            public bool IsPasswordVisible
            {
                get => _isPasswordVisible;
                set
                {
                    if (_isPasswordVisible != value)
                    {
                        _isPasswordVisible = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public LoginScreen()
            {
                this.InitializeComponent();
                
                // Get ViewModel from DI container
                ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
                
                // Subscribe to events
                ViewModel.LoginSuccessful += OnLoginSuccessful;
                ViewModel.OpenConfigRequested += OnOpenConfigRequested;

                this.Unloaded += LoginScreen_Unloaded;
            }

            private void LoginScreen_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
            {
                ViewModel.LoginSuccessful -= OnLoginSuccessful;
                ViewModel.OpenConfigRequested -= OnOpenConfigRequested;
            }

            private void OnLoginSuccessful(object? sender, EventArgs e)
            {
                // Navigate to ShellPage on successful login
                Frame.Navigate(typeof(ShellPage));
            }

            private async void OnOpenConfigRequested(object? sender, EventArgs e)
            {
                var dialog = new ConfigScreen
                {
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }

            private void RevealButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
            {
                IsPasswordVisible = !IsPasswordVisible;
                
                // Update icon: E7B3 = RedEye (hidden), ED1A = Hide (visible)
                RevealIcon.Glyph = IsPasswordVisible ? "\uED1A" : "\uE7B3";
            }
        }
    }