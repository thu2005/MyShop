using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Services;
using System;
using System.Linq;

namespace MyShop.App.Views
{
    public sealed partial class ShellPage : Page
    {
        public static ShellPage Instance { get; private set; }
        public ShellViewModel ViewModel { get; }
        private readonly IOnboardingService _onboardingService;
        private readonly IConfigService _configService;
        private readonly ISessionManager _sessionManager;

        public ShellPage()
        {
            this.InitializeComponent();
            Instance = this;
            _onboardingService = App.Current.Services.GetRequiredService<IOnboardingService>();
            _configService = App.Current.Services.GetRequiredService<IConfigService>();
            _sessionManager = App.Current.Services.GetRequiredService<ISessionManager>();
            ViewModel = App.Current.Services.GetRequiredService<ShellViewModel>();
            ViewModel.LogoutRequested += OnLogoutRequested;

            NavView.SelectionChanged += NavView_SelectionChanged;
            NavView.ItemInvoked += NavView_ItemInvoked;
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

        private async void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Restore last opened page or default to first item
            RestoreLastOpenedPage();
            
            // Load categories on UI thread
            await ViewModel.EnsureCategoriesLoadedAsync();
            
            if (ViewModel.Categories.Count > 0)
            {
                RefreshCategoryMenuItems();
            }

            // Initialize license on first run and record app usage
            ViewModel.InitializeLicense();

            // Check license status and show appropriate dialog
            CheckLicenseStatus();

            CheckOnboardingAsync();

            // Hide debug menu items in Release builds
            HideDebugMenuItems();
        }

        private async void CheckLicenseStatus()
        {
            var status = ViewModel.GetLicenseStatus();

            switch (status)
            {
                case Core.Models.LicenseStatus.ClockTampered:
                    await ShowLicenseErrorDialog(
                        "Clock Tampering Detected",
                        "The system clock appears to have been rolled back. Please restore the correct system time to continue using the application.",
                        true);
                    break;

                case Core.Models.LicenseStatus.MachineMismatch:
                    await ShowLicenseErrorDialog(
                        "License Error",
                        "This license is not valid for this machine. The application may have been copied from another computer.",
                        true);
                    break;

                case Core.Models.LicenseStatus.Invalid:
                    await ShowLicenseErrorDialog(
                        "License Error",
                        "The license data is corrupted or invalid. Please contact support.",
                        true);
                    break;

                case Core.Models.LicenseStatus.TrialExpired:
                    await ShowLicenseErrorDialog(
                        "Trial Expired",
                        "Your 15-day trial has expired. Some features like 'Create Order' and 'Add Product' are now disabled.\n\nPlease purchase a license to continue using all features.",
                        false);
                    break;
            }
        }

        private async System.Threading.Tasks.Task ShowLicenseErrorDialog(string title, string message, bool isCritical)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = isCritical ? "Exit" : "Continue (Limited)",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            if (isCritical)
            {
                dialog.PrimaryButtonText = "Retry";
#if DEBUG
                dialog.SecondaryButtonText = "Reset License (Debug)";
#endif
            }
            else
            {
                dialog.PrimaryButtonText = "Enter License Key";
            }

            var result = await dialog.ShowAsync();

            if (isCritical)
            {
                if (result == ContentDialogResult.Primary)
                {
                    // Reload license and recheck
                    ViewModel.InitializeLicense();
                    CheckLicenseStatus();
                }
                else if (result == ContentDialogResult.Secondary)
                {
#if DEBUG
                    // Force reset and re-initialize
                    ViewModel.DebugResetTrial();
                    ViewModel.InitializeLicense();
                    CheckLicenseStatus();
#endif
                }
                else
                {
                    // Exit application
                    Application.Current.Exit();
                }
            }
            else if (result == ContentDialogResult.Primary)
            {
                // Show license activation dialog
                await ShowActivationDialog();
            }
        }

        public async System.Threading.Tasks.Task ShowTrialExpiredDialog(string featureName)
        {
            var dialog = new ContentDialog
            {
                Title = "Trial Expired",
                Content = $"Your 15-day trial period has expired. The feature '{featureName}' is restricted to the full version.\n\nPlease activate your license to continue using all management features.",
                PrimaryButtonText = "Activate Now",
                CloseButtonText = "Maybe Later",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ShowActivationDialog();
            }
        }

        public async System.Threading.Tasks.Task ShowActivationDialog()
        {
            var licenseService = App.Current.Services.GetRequiredService<ILicenseService>();
            var fingerprintService = App.Current.Services.GetRequiredService<IFingerprintService>();
            var machineId = fingerprintService.GetMachineSignature();

            var machineIdBox = new TextBox
            {
                Text = machineId,
                IsReadOnly = true,
                Header = "Your Machine ID (send this to developer):",
                Margin = new Thickness(0, 0, 0, 16)
            };

            var copyBtn = new Button
            {
                Content = "Copy ID",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, -50, 0, 16)
            };
            copyBtn.Click += (s, e) =>
            {
                var package = new Windows.ApplicationModel.DataTransfer.DataPackage();
                package.SetText(machineId);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(package);
            };

            var inputBox = new TextBox
            {
                Header = "Enter License Key:",
                PlaceholderText = "XXXX-XXXX-XXXX-XXXX",
                MaxLength = 19,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var dialog = new ContentDialog
            {
                Title = "Activate MyShop License",
                Content = new StackPanel
                {
                    Children =
                    {
                        machineIdBox,
                        copyBtn,
                        inputBox
                    }
                },
                PrimaryButtonText = "Activate Now",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Attempt activation
                bool activated = licenseService.ActivateLicense(inputBox.Text.Trim());

                if (activated)
                {
                    ViewModel.InitializeLicense(); // Refresh license properties

                    var successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "License activated successfully! All features are now unlocked.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Activation Failed",
                        Content = "The license key is invalid. Please check and try again.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private async void CheckOnboardingAsync()
        {
            string username = ViewModel.CurrentUser?.Username ?? "unknown";
            if (!_onboardingService.IsOnboardingCompleted(username))
            {
                ShowOnboardingDialog();
            }
        }

        private async void ShowOnboardingDialog()
        {
            string username = ViewModel.CurrentUser?.Username ?? "unknown";
            var onboardingDialog = new Dialogs.OnboardingDialog(ViewModel.UserRole)
            {
                XamlRoot = this.XamlRoot
            };

            onboardingDialog.SecondaryButtonClick += (s, args) =>
            {
                // Skip button (now on the left/primary)
                _onboardingService.MarkOnboardingAsCompleted(username);
            };

            onboardingDialog.CloseButtonClick += (s, args) =>
            {
                // Next button (now on the right/close)
                var flipView = onboardingDialog.FindName("OnboardingFlipView") as FlipView;
                if (flipView != null)
                {
                    if (flipView.SelectedIndex < flipView.Items.Count - 1)
                    {
                        // Prevent closing and move to next slide
                        args.Cancel = true;
                        flipView.SelectedIndex++;
                    }
                    else
                    {
                        // On last slide, allow closing and mark as completed
                        _onboardingService.MarkOnboardingAsCompleted(username);
                    }
                }
            };

            await onboardingDialog.ShowAsync();
        }

        private void RestoreLastOpenedPage()
        {
            var lastPageTag = _configService.GetLastOpenedPage();
            NavigationViewItem? targetItem = null;

            if (!string.IsNullOrEmpty(lastPageTag))
            {
                // Try to find the navigation item with the saved tag
                targetItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(i => i.Tag?.ToString() == lastPageTag);

                // If not found in main items, check if it's a Products category
                if (targetItem == null && lastPageTag.StartsWith("Products"))
                {
                    // Default to "All Products" if specific category not found
                    targetItem = NavView.MenuItems
                        .OfType<NavigationViewItem>()
                        .FirstOrDefault(i => i.Tag?.ToString() == "Products_0");
                }
            }

            // Default to first item (Dashboard) if nothing found
            if (targetItem == null)
            {
                targetItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
            }

            if (targetItem != null)
            {
                NavView.SelectedItem = targetItem;
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
                        case "Discounts":
                                pageType = typeof(DiscountsPage);
                                break;
                        case "Reports":
                                pageType = typeof(ReportsPage);
                                break;
                            case "Users":
                                pageType = typeof(UsersPage);
                                break;
                        }
                    }

                    if (pageType != null)
                    {
                        ContentFrame.Navigate(pageType, navigationParam);
                        
                        // Save the last opened page only if session is persisted (Remember Me)
                        if (_sessionManager.IsSessionPersisted)
                        {
                            _configService.SaveLastOpenedPage(tag);
                        }
                    }
                }
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer is NavigationViewItem item)
            {
                if (item.Tag?.ToString() == "Help")
                {
                    ShowOnboardingDialog();
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
                        errorText.Text = "Category name cannot be empty.";
                        errorText.Visibility = Visibility.Visible;
                        dialog.Hide();
                        continue; // Retry
                    }

                    try
                    {
                        await ViewModel.UpdateCategoryAsync(categoryId, inputBox.Text.Trim());
                        success = true;
                        
                        // Refresh ProductsScreen if it's currently displayed
                        if (ContentFrame.Content is ProductsScreen productsScreen)
                        {
                            await productsScreen.ViewModel.LoadProductsAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Show error message inline
                        errorText.Text = ex.Message.Contains("already exists") 
                            ? "Category name already exists. Please choose a different name."
                            : $"Error: {ex.Message}";
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
                PrimaryButtonText = "Cancel",
                CloseButtonText = "Log out",
                CloseButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"],
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await logoutDialog.ShowAsync();

            if (result == ContentDialogResult.None)
            {
                Frame.Navigate(typeof(LoginScreen));
                Frame.BackStack.Clear();
            }
        }

        private async void DebugBanner_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
#if DEBUG
            ViewModel.DebugForceExpire();

            var dialog = new ContentDialog
            {
                Title = "Debug Mode",
                Content = "Trial has been forced to EXPIRED state for testing.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
#endif
        }

        private async void OnDebugResetLicenseClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            // Reset license completely
            ViewModel.DebugResetTrial();
            ViewModel.InitializeLicense();

            var dialog = new ContentDialog
            {
                Title = "License Reset",
                Content = "License data has been cleared. App will restart trial period.\n\nYou can now test the expired state by double-tapping the trial banner.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();

            // Re-check license status
            CheckLicenseStatus();
#else
            await System.Threading.Tasks.Task.CompletedTask;
#endif
        }

        private void HideDebugMenuItems()
        {
#if !DEBUG
            if (DebugResetLicenseMenuItem != null)
            {
                DebugResetLicenseMenuItem.Visibility = Visibility.Collapsed;
            }
#endif
        }
    }
}