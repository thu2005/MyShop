using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using MyShop.App.ViewModels;
using MyShop.Core.Models;
using MyShop.Core.Interfaces.Services;

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

            Product product = null;
            bool startInEditMode = false;

            // Handle different parameter types
            if (e.Parameter is Product directProduct)
            {
                product = directProduct;
            }
            else if (e.Parameter != null)
            {
                // Handle anonymous object with Product and StartInEditMode
                var paramType = e.Parameter.GetType();
                var productProp = paramType.GetProperty("Product");
                var editModeProp = paramType.GetProperty("StartInEditMode");
                
                if (productProp != null)
                    product = productProp.GetValue(e.Parameter) as Product;
                
                if (editModeProp != null)
                    startInEditMode = (bool)editModeProp.GetValue(e.Parameter);
            }

            if (product != null)
            {
                await ViewModel.LoadProduct(product);
                
                // Enter edit mode if requested
                if (startInEditMode)
                {
                    ViewModel.EnterEditModeCommand.Execute(null);
                }
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
                
                // Reload categories in case product changed category
                await ReloadShellCategories();
            }
            else
            {
                // Check license before allowing product editing
                var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
                if (!licenseService.IsFeatureAllowed("EditProduct"))
                {
                    await ShowTrialExpiredDialog("Edit Product");
                    return;
                }

                ViewModel.EnterEditModeCommand.Execute(null);
            }
        }

        private async System.Threading.Tasks.Task ShowTrialExpiredDialog(string featureName)
        {
            var shell = ShellPage.Instance;
            if (shell != null)
            {
                await shell.ShowTrialExpiredDialog(featureName);
            }
        }

        private void OnCloseClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Check if we can go back
            if (Frame.CanGoBack)
            {
                // Get the previous page type from BackStack
                var previousPage = Frame.BackStack.LastOrDefault();
                
                // If coming from Dashboard, update sidebar to Dashboard
                if (previousPage?.SourcePageType == typeof(Dashboard))
                {
                    var shellPage = FindShellPage();
                    if (shellPage != null)
                    {
                        shellPage.SetSidebarSelectionWithoutNavigation("Dashboard");
                    }
                }
                
                Frame.GoBack();
            }
            else
            {
                // If no back stack, navigate to Products screen
                Frame.Navigate(typeof(ProductsScreen), 0);
            }
        }

        private ShellPage FindShellPage()
        {
            var frame = this.Frame;
            while (frame != null)
            {
                if (frame.Content is ShellPage shellPage)
                {
                    return shellPage;
                }
                
                var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(frame);
                frame = null;
                while (parent != null)
                {
                    if (parent is Microsoft.UI.Xaml.Controls.Frame parentFrame)
                    {
                        frame = parentFrame;
                        break;
                    }
                    parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
                }
            }
            return null;
        }

        private async void OnDeleteClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (ViewModel.CurrentProduct == null) return;

            // Check license before allowing product deletion
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            if (!licenseService.IsFeatureAllowed("DeleteProduct"))
            {
                await ShowTrialExpiredDialog("Delete Product");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Delete Product",
                Content = $"Are you sure you want to permanently delete '{ViewModel.CurrentProduct.Name}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            // Style the delete button as red
            var primaryButtonStyle = new Microsoft.UI.Xaml.Style(typeof(Microsoft.UI.Xaml.Controls.Button));
            primaryButtonStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Button.BackgroundProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red)));
            primaryButtonStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Button.ForegroundProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)));
            dialog.PrimaryButtonStyle = primaryButtonStyle;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await ViewModel.DeleteProductCommand.ExecuteAsync(null);
                    
                    // Reload categories in ShellPage to update counts in sidebar
                    await ReloadShellCategories();
                    
                    // Navigate back to Products screen after successful deletion
                    Frame.GoBack();
                }
                catch (Exception ex)
                {
                    // Show error dialog
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to delete product: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private void OnSetMainImageClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (!ViewModel.IsEditMode) return;
            
            var button = sender as Microsoft.UI.Xaml.Controls.Button;
            if (button?.Tag is ProductImage clickedImage)
            {
                // Set all images to not main
                foreach (var img in ViewModel.ProductImagesCollection)
                {
                    img.IsMain = false;
                }
                
                // Set clicked image as main
                clickedImage.IsMain = true;
                
                // Update CurrentProduct.Images to match
                if (ViewModel.CurrentProduct.Images != null)
                {
                    foreach (var img in ViewModel.CurrentProduct.Images)
                    {
                        img.IsMain = false;
                    }
                    var matchingImage = ViewModel.CurrentProduct.Images.FirstOrDefault(i => i.ImageUrl == clickedImage.ImageUrl);
                    if (matchingImage != null)
                    {
                        matchingImage.IsMain = true;
                    }
                }
                
                // Trigger UI update for thumbnails
                var temp = new System.Collections.ObjectModel.ObservableCollection<ProductImage>(ViewModel.ProductImagesCollection);
                ViewModel.ProductImagesCollection.Clear();
                foreach (var img in temp)
                {
                    ViewModel.ProductImagesCollection.Add(img);
                }
                
                // IMPORTANT: Update main image URL to change large preview
                var mainImg = ViewModel.CurrentProduct.Images?.FirstOrDefault(i => i.IsMain);
                ViewModel.MainImageUrl = mainImg?.ImageUrl ?? ViewModel.CurrentProduct.Images?.FirstOrDefault()?.ImageUrl;
            }
        }

        private void OnDeleteImageClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (!ViewModel.IsEditMode) return;
            
            var button = sender as Microsoft.UI.Xaml.Controls.Button;
            if (button?.Tag is ProductImage imageToDelete)
            {
                // Remove from both collections
                ViewModel.CurrentProduct.Images?.Remove(imageToDelete);
                ViewModel.ProductImagesCollection.Remove(imageToDelete);
                
                // If deleted image was main, set first image as main
                if (imageToDelete.IsMain && ViewModel.ProductImagesCollection.Count > 0)
                {
                    ViewModel.ProductImagesCollection[0].IsMain = true;
                    
                    // Trigger UI refresh
                    var temp = new System.Collections.ObjectModel.ObservableCollection<ProductImage>(ViewModel.ProductImagesCollection);
                    ViewModel.ProductImagesCollection.Clear();
                    foreach (var img in temp)
                    {
                        ViewModel.ProductImagesCollection.Add(img);
                    }
                }
            }
        }

        private void OnImagePointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Microsoft.UI.Xaml.Controls.Grid overlay)
            {
                overlay.Opacity = 1;
            }
        }

        private void OnImagePointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Microsoft.UI.Xaml.Controls.Grid overlay)
            {
                overlay.Opacity = 0;
            }
        }

        private async System.Threading.Tasks.Task ReloadShellCategories()
        {
            // Find the ShellPage in the navigation stack and reload its categories
            var frame = this.Frame;
            while (frame != null)
            {
                if (frame.Content is ShellPage shellPage)
                {
                    await shellPage.ViewModel.LoadCategoriesAsync();
                    break;
                }
                // Try to get parent frame
                var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(frame);
                frame = null;
                while (parent != null)
                {
                    if (parent is Microsoft.UI.Xaml.Controls.Frame parentFrame)
                    {
                        frame = parentFrame;
                        break;
                    }
                    parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
                }
            }
        }
    }
}
