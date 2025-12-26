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
                var item = new NavigationViewItem
                {
                    Content = cat.DisplayText,
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
            if (Frame.CanGoBack) Frame.GoBack();
            else Frame.Navigate(typeof(LoginScreen));
        }
    }
}