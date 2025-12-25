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

            // Subscribe to collection changes
            ViewModel.Categories.CollectionChanged += Categories_CollectionChanged;
        }

        private void Categories_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshCategoryMenuItems();
        }

        private void RefreshCategoryMenuItems()
        {
            // Clear existing children from the "All Products" item
            AllProductsNavItem.MenuItems.Clear();

            // Add categories as children
            // Skip ID 0 because that is the parent "All Products" itself
            foreach (var cat in ViewModel.Categories.Where(c => c.Id != 0))
            {
                var item = new NavigationViewItem
                {
                    Content = cat.DisplayText,
                    Tag = $"Products_{cat.Id}", // Tag format: "Products_ID"
                    Icon = new SymbolIcon(Symbol.Folder) // Optional: Add folder icon
                };

                // Add tooltip for stock count
                ToolTipService.SetToolTip(item, $"{cat.Count} items in stock");

                AllProductsNavItem.MenuItems.Add(item);
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0]; // Dashboard

            // Initial populate
            if (ViewModel.Categories.Count > 0)
            {
                RefreshCategoryMenuItems();
            }
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

                    if (tag.StartsWith("Products_"))
                    {
                        pageType = typeof(ProductsScreen);
                        // Extract ID from "Products_1"
                        if (int.TryParse(tag.Split('_')[1], out int catId))
                        {
                            navigationParam = catId;
                        }
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
                            case "Reports":
                                pageType = typeof(ReportsPage);
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

        private void OnLogoutRequested()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(LoginScreen));
            }
        }
    }
}