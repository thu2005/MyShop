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

            // Subscribe to collection changes to update UI
            ViewModel.Categories.CollectionChanged += Categories_CollectionChanged;
        }

        private void Categories_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Rebuild the dynamic category menu items
            RefreshCategoryMenuItems();
        }

        private void RefreshCategoryMenuItems()
        {
            // 1. Find the index of the Category Header  
            int headerIndex = NavView.MenuItems.IndexOf(CategoryHeader);
            if (headerIndex < 0) return;

            // 2. Remove existing dynamic items (items between Header and Separator/End)  
            while (NavView.MenuItems.Count > headerIndex + 1)
            {
                var item = NavView.MenuItems[headerIndex + 1];
                if (item is NavigationViewItemSeparator || (item is NavigationViewItem ni && ni.Tag.ToString() == "Orders"))
                    break;
                NavView.MenuItems.RemoveAt(headerIndex + 1);
            }

            // 3. Add new items  
            // Skip ID 0 (All Products) because we have a static "All Products" button  
            foreach (var cat in ViewModel.Categories.Where(c => c.Id != 0))
            {
                var item = new NavigationViewItem
                {
                    Content = cat.DisplayText, // "Iphone (5)"  
                    Tag = $"Products_{cat.Id}", // Tag format: "Products_ID"  
                    Icon = new SymbolIcon(Symbol.Folder)
                };

                // Set the tooltip using ToolTipService.SetToolTip  
                ToolTipService.SetToolTip(item, $"{cat.Count} items in stock");

                NavView.MenuItems.Insert(++headerIndex, item);
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0]; // Dashboard
            // Initial populate if data is already there
            if (ViewModel.Categories.Count > 0) RefreshCategoryMenuItems();
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected) { /* ... */ }
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
                            case "Dashboard": pageType = typeof(Dashboard); break;
                            case "Orders": pageType = typeof(OrdersPage); break;
                            case "Reports": pageType = typeof(ReportsPage); break;
                        }
                    }

                    if (pageType != null)
                    {
                        ContentFrame.Navigate(pageType, navigationParam);
                    }
                }
            }
        }

        // ... (Logout and other handlers remain the same)
        private void OnLogoutRequested() { /*...*/ }
    }
}