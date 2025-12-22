    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using MyShop.App.ViewModels;
    using System;

    namespace MyShop.App.Views
    {
        public sealed partial class LoginScreen : Page
        {
            public LoginViewModel ViewModel { get; }

            public LoginScreen()
            {
                this.InitializeComponent();
                
                // Get ViewModel from DI container
                ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
                
                // Subscribe to events
                ViewModel.LoginSuccessful += OnLoginSuccessful;
                ViewModel.OpenConfigRequested += OnOpenConfigRequested;
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
        }
    }