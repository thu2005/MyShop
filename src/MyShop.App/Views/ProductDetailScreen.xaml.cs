using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using MyShop.App.ViewModels;
using MyShop.Core.Models;

namespace MyShop.App.Views
{
    public sealed partial class ProductDetailScreen : Page
    {
        public ProductDetailViewModel ViewModel { get; }

        public ProductDetailScreen()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<ProductDetailViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Product product)
            {
                await ViewModel.LoadProduct(product);
            }

            // Update initial button text
            UpdateEditButtonText();
            ViewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(ViewModel.IsEditMode))
                {
                    UpdateEditButtonText();
                }
            };
        }

        private void UpdateEditButtonText()
        {
            EditUpdateText.Text = ViewModel.IsEditMode ? "UPDATE" : "EDIT";
        }

        private async void OnEditUpdateClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (ViewModel.IsEditMode)
            {
                await ViewModel.SaveChangesCommand.ExecuteAsync(null);
            }
            else
            {
                ViewModel.EnterEditModeCommand.Execute(null);
            }
        }

        private void OnCloseClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Navigate back to Products screen
            Frame.GoBack();
        }
    }
}
