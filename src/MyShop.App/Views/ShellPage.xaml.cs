using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;
using System;
using System.Linq;

namespace MyShop.App.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; }

        public ShellPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ShellViewModel>();
            ViewModel.LogoutRequested += OnLogoutRequested;

            NavView.SelectionChanged += NavView_SelectionChanged;
            NavView.Loaded += NavView_Loaded;

            ViewModel.Categories.CollectionChanged += Categories_CollectionChanged;

            this.Unloaded += ShellPage_Unloaded;
        }

        private void ShellPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LogoutRequested -= OnLogoutRequested;
            ViewModel.Categories.CollectionChanged -= Categories_CollectionChanged;
        }

        private void Categories_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshCategoryMenuItems();
        }

        private void RefreshCategoryMenuItems()
        {
            if (AllProductsNavItem == null) return;

            AllProductsNavItem.MenuItems.Clear();

            foreach (var cat in ViewModel.Categories.Where(c => c.Id != 0))
            {
                // Create a Grid to hold the category name and the menu button
                var grid = new Grid
                {
                    ColumnSpacing = 8
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Category name TextBlock
                var textBlock = new TextBlock
                {
                    Text = cat.DisplayText,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(textBlock, 0);
                grid.Children.Add(textBlock);

                // Three dots menu button (Admin only)
                var menuButton = new Button
                {
                    Content = new FontIcon { Glyph = "\uE712", FontSize = 12 }, // Three dots icon
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(4),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = cat.Id,
                    Visibility = ViewModel.IsAdmin ? Visibility.Visible : Visibility.Collapsed
                };

                // Create MenuFlyout
                var menuFlyout = new MenuFlyout();
                
                var editItem = new MenuFlyoutItem
                {
                    Text = "Edit",
                    Icon = new SymbolIcon(Symbol.Edit),
                    Tag = cat.Id
                };
                editItem.Click += OnEditCategoryClick;

                var deleteItem = new MenuFlyoutItem
                {
                    Text = "Delete",
                    Icon = new SymbolIcon(Symbol.Delete),
                    Tag = cat.Id
                };
                deleteItem.Click += OnDeleteCategoryClick;

                menuFlyout.Items.Add(editItem);
                menuFlyout.Items.Add(deleteItem);
                menuButton.Flyout = menuFlyout;

                Grid.SetColumn(menuButton, 1);
                grid.Children.Add(menuButton);

                // Create NavigationViewItem with custom content
                var item = new NavigationViewItem
                {
                    Content = grid,
                    Tag = $"Products_{cat.Id}",
                    Icon = new SymbolIcon(Symbol.Folder)
                };
                ToolTipService.SetToolTip(item, $"{cat.Count} items in stock");
                AllProductsNavItem.MenuItems.Add(item);
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            if (ViewModel.Categories.Count > 0)
            {
                RefreshCategoryMenuItems();
            }
        }

        public void SetSidebarSelectionWithoutNavigation(string tag)
        {
            // Temporarily remove event handler
            NavView.SelectionChanged -= NavView_SelectionChanged;
            
            // Find and select the item
            var item = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(i => i.Tag?.ToString() == tag);
            
            if (item != null)
            {
                NavView.SelectedItem = item;
            }
            
            // Re-attach event handler
            NavView.SelectionChanged += NavView_SelectionChanged;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var selectedItem = args.SelectedItem as NavigationViewItem;
                if (selectedItem != null)
                {
                    string tag = selectedItem.Tag?.ToString();
                    if (string.IsNullOrEmpty(tag)) return;

                    Type pageType = null;
                    object navigationParam = null;

                    // FIX: Safer check for Product tags
                    if (tag.StartsWith("Products"))
                    {
                        pageType = typeof(ProductsScreen);

                        // Default to 0 (All)
                        int catId = 0;

                        // Try to extract ID if underscore exists (e.g. "Products_5")
                        if (tag.Contains('_'))
                        {
                            var parts = tag.Split('_');
                            if (parts.Length > 1)
                            {
                                int.TryParse(parts[1], out catId);
                            }
                        }

                        navigationParam = catId;
                    }
                    else
                    {
                        switch (tag)
                        {
                            case "Dashboard":
                                pageType = typeof(Dashboard);
                                break;
                            case "Orders":
                                pageType = typeof(OrdersPage);
                                break;
                            case "Customers":
                            pageType = typeof(CustomersPage);
                            break;
                        case "Reports":
                                pageType = typeof(ReportsPage);
                                break;
                            case "Users":
                                // STEP 2: Điều hướng đến trang quản lý nhân viên
                                pageType = typeof(UsersPage);
                                break;
                        }
                    }

                    if (pageType != null)
                    {
                        ContentFrame.Navigate(pageType, navigationParam);
                    }
                }
            }
        }

        private async void OnEditCategoryClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is int categoryId)
            {
                var category = ViewModel.Categories.FirstOrDefault(c => c.Id == categoryId);
                if (category == null) return;

                var inputBox = new TextBox
                {
                    Text = category.Name,
                    PlaceholderText = "Enter category name",
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var errorText = new TextBlock
                {
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                    Visibility = Visibility.Collapsed,
                    Margin = new Thickness(0, 4, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };

                // Move cursor to end of text when dialog opens
                inputBox.Loaded += (s, args) =>
                {
                    inputBox.SelectionStart = inputBox.Text.Length;
                    inputBox.SelectionLength = 0;
                };

                var dialog = new ContentDialog
                {
                    Title = "Edit Category",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock { Text = "Category Name:" },
                            inputBox,
                            errorText
                        }
                    },
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                ContentDialogResult result;
                bool success = false;

                while (!success)
                {
                    result = await dialog.ShowAsync();

                    if (result != ContentDialogResult.Primary)
                    {
                        break; // User cancelled
                    }

                    // Validate empty input
                    if (string.IsNullOrWhiteSpace(inputBox.Text))
                    {
                        errorText.Text = "⚠️ Category name cannot be empty.";
                        errorText.Visibility = Visibility.Visible;
                        dialog.Hide();
                        continue; // Retry
                    }

                    try
                    {
                        await ViewModel.UpdateCategoryAsync(categoryId, inputBox.Text.Trim());
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        // Show error message inline
                        errorText.Text = ex.Message.Contains("already exists") 
                            ? "⚠️ Category name already exists. Please choose a different name."
                            : $"⚠️ Error: {ex.Message}";
                        errorText.Visibility = Visibility.Visible;
                        
                        // Keep dialog open for retry
                        dialog.Hide();
                    }
                }
            }
        }

        private async void OnDeleteCategoryClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is int categoryId)
            {
                var category = ViewModel.Categories.FirstOrDefault(c => c.Id == categoryId);
                if (category == null) return;

                var dialog = new ContentDialog
                {
                    Title = "Delete Category",
                    Content = $"Are you sure you want to delete '{category.Name}'?\n\n⚠️ WARNING: All products in this category ({category.Count} products) will also be permanently deleted!",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var primaryButtonStyle = new Style(typeof(Button));
                primaryButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red)));
                primaryButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)));
                dialog.PrimaryButtonStyle = primaryButtonStyle;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        await ViewModel.DeleteCategoryAsync(categoryId);
                        
                        // Navigate to "All Products" to refresh the ProductsScreen
                        ContentFrame.Navigate(typeof(ProductsScreen), 0);
                    }
                    catch (Exception ex)
                    {
                        var errorDialog = new ContentDialog
                        {
                            Title = "Error",
                            Content = $"Failed to delete category: {ex.Message}",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
                }
            }
        }

        private async void OnLogoutRequested()
        {
            ContentDialog logoutDialog = new ContentDialog
            {
                Title = "Log out",
                Content = "Are you sure you want to log out?",
                PrimaryButtonText = "Log out",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await logoutDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(LoginScreen));
                Frame.BackStack.Clear();
            }
        }
    }
}