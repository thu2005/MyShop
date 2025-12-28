using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.App.ViewModels;
using System;

namespace MyShop.App.Views
{
    public sealed partial class ImportProductsScreen : Page
    {
        public ImportProductsViewModel ViewModel { get; }

        public ImportProductsScreen()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ImportProductsViewModel>();
            
            // Subscribe to import completion
            ViewModel.ImportCompleted += OnImportCompleted;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string filePath)
            {
                await ViewModel.LoadFileAsync(filePath);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            // Unsubscribe to prevent memory leaks
            ViewModel.ImportCompleted -= OnImportCompleted;
        }

        private async void OnImportCompleted(object sender, EventArgs e)
        {
            // Small delay to ensure database commits complete
            await System.Threading.Tasks.Task.Delay(500);
            
            // Show success message
            var dialog = new ContentDialog
            {
                Title = "Import Successful",
                Content = $"Products imported successfully!",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            
            await dialog.ShowAsync();
            
            // Navigate back to Products screen
            Frame.GoBack();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            // Navigate back to Products screen
            Frame.GoBack();
        }
    }
}
