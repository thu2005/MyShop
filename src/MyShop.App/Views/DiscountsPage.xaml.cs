using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;
using MyShop.Core.Models;

namespace MyShop.App.Views
{
    public sealed partial class DiscountsPage : Page
    {
        public DiscountsViewModel ViewModel { get; }

        public DiscountsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.GetService<DiscountsViewModel>();
        }

        public bool IsAdminUser()
        {
            return ViewModel.IsAdmin;
        }

        private async void NewDiscountButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsAdmin) return;

            var stackPanel = new StackPanel { Spacing = 16, Width = 450 };

            var errorText = new TextBlock
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 0, 0, 8),
                TextWrapping = TextWrapping.Wrap
            };

            var codeBox = new TextBox { Header = "Discount Code", PlaceholderText = "Enter unique code" };
            var nameBox = new TextBox { Header = "Discount Name", PlaceholderText = "Enter discount name" };
            var descBox = new TextBox { Header = "Description", PlaceholderText = "Optional description", AcceptsReturn = true, Height = 60 };

            var typeCombo = new ComboBox
            {
                Header = "Discount Type",
                PlaceholderText = "Select type",
                ItemsSource = new[] { DiscountType.PERCENTAGE, DiscountType.FIXED_AMOUNT },
                SelectedIndex = 0,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var valueBox = new NumberBox { Header = "Value", PlaceholderText = "Enter value", Minimum = 0 };
            var minPurchaseBox = new NumberBox { Header = "Minimum Purchase (Optional)", PlaceholderText = "Min amount", Minimum = 0 };
            var maxDiscountBox = new NumberBox { Header = "Maximum Discount (Optional)", PlaceholderText = "Max amount", Minimum = 0 };

            var startDatePicker = new CalendarDatePicker { Header = "Start Date (Optional)", PlaceholderText = "Select date" };
            var endDatePicker = new CalendarDatePicker { Header = "End Date (Optional)", PlaceholderText = "Select date" };

            var memberOnlyCheckBox = new CheckBox
            {
                Content = "Member Only",
                IsChecked = false
            };

            stackPanel.Children.Add(errorText);
            stackPanel.Children.Add(codeBox);
            stackPanel.Children.Add(nameBox);
            stackPanel.Children.Add(descBox);
            stackPanel.Children.Add(typeCombo);
            stackPanel.Children.Add(valueBox);
            stackPanel.Children.Add(minPurchaseBox);
            stackPanel.Children.Add(maxDiscountBox);
            stackPanel.Children.Add(startDatePicker);
            stackPanel.Children.Add(endDatePicker);
            stackPanel.Children.Add(memberOnlyCheckBox);

            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "New Discount",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                PrimaryButtonStyle = this.Resources["DialogPrimaryButtonStyle"] as Style,
                Content = new ScrollViewer { Content = stackPanel, MaxHeight = 500 }
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

                // Validation
                if (string.IsNullOrWhiteSpace(codeBox.Text))
                {
                    errorText.Text = "⚠️ Discount Code is required!";
                    errorText.Visibility = Visibility.Visible;
                    dialog.Hide();
                    continue;
                }

                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    errorText.Text = "⚠️ Discount Name is required!";
                    errorText.Visibility = Visibility.Visible;
                    dialog.Hide();
                    continue;
                }

                if (valueBox.Value <= 0)
                {
                    errorText.Text = "⚠️ Discount Value must be greater than 0!";
                    errorText.Visibility = Visibility.Visible;
                    dialog.Hide();
                    continue;
                }

                try
                {
                    var newDiscount = new Discount
                    {
                        Code = codeBox.Text.Trim(),
                        Name = nameBox.Text.Trim(),
                        Description = descBox.Text?.Trim(),
                        Type = (DiscountType)(typeCombo.SelectedItem ?? DiscountType.PERCENTAGE),
                        Value = (decimal)(valueBox.Value),
                        MinPurchase = minPurchaseBox.Value > 0 ? (decimal?)minPurchaseBox.Value : null,
                        MaxDiscount = maxDiscountBox.Value > 0 ? (decimal?)maxDiscountBox.Value : null,
                        StartDate = startDatePicker.Date?.DateTime,
                        EndDate = endDatePicker.Date?.DateTime,
                        MemberOnly = memberOnlyCheckBox.IsChecked ?? false
                        // IsActive is set by backend (defaults to true)
                    };

                    await ViewModel.AddDiscountCommand.ExecuteAsync(newDiscount);

                    // Check if there was an error from ViewModel
                    if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
                    {
                        errorText.Text = $"⚠️ {ViewModel.ErrorMessage}";
                        errorText.Visibility = Visibility.Visible;
                        dialog.Hide();
                        continue;
                    }

                    success = true;

                    // Show success message
                    var successDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Success",
                        Content = "Discount created successfully!",
                        CloseButtonText = "OK"
                    };
                    await successDialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    errorText.Text = ex.Message.Contains("already exists")
                        ? "⚠️ Discount code already exists. Please use a different code."
                        : $"⚠️ Error: {ex.Message}";
                    errorText.Visibility = Visibility.Visible;
                    dialog.Hide();
                }
            }
        }

        private async void ViewDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is SelectableDiscount wrapper)
            {
                var discount = wrapper.Discount;
                var dialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Discount Details",
                    CloseButtonText = "Close",
                    DefaultButton = ContentDialogButton.Close
                };

                var mainStack = new StackPanel { Spacing = 20, Width = 500 };

                // Header with Code and Status
                var headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var codeText = new TextBlock
                {
                    Text = discount.Code,
                    FontSize = 20,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 63, 98))
                };
                Grid.SetColumn(codeText, 0);
                headerGrid.Children.Add(codeText);

                var statusBorder = new Border
                {
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 6, 12, 6),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        discount.IsActive ? Windows.UI.Color.FromArgb(255, 34, 197, 94) : Windows.UI.Color.FromArgb(255, 239, 68, 68)
                    )
                };
                var statusText = new TextBlock
                {
                    Text = discount.IsActive ? "Active" : "Inactive",
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    FontSize = 12
                };
                statusBorder.Child = statusText;
                Grid.SetColumn(statusBorder, 1);
                headerGrid.Children.Add(statusBorder);

                mainStack.Children.Add(headerGrid);

                // Basic Information Section
                AddSectionHeader(mainStack, "Basic Information");
                AddDetailField(mainStack, "Discount Name", discount.Name);
                AddDetailField(mainStack, "Description", discount.Description ?? "No description");
                AddDetailField(mainStack, "Discount Type", FormatDiscountType(discount.Type));

                // Value Information Section
                AddSectionHeader(mainStack, "Value & Limits");
                AddDetailField(mainStack, "Discount Value", FormatValue(discount.Type, discount.Value));
                AddDetailField(mainStack, "Minimum Purchase", discount.MinPurchase.HasValue ? $"₫{discount.MinPurchase.Value:N0}" : "No minimum");
                AddDetailField(mainStack, "Maximum Discount", discount.MaxDiscount.HasValue ? $"₫{discount.MaxDiscount.Value:N0}" : "No limit");
                AddDetailField(mainStack, "Member Only", discount.MemberOnly ? "Yes" : "No");

                // Date Information Section
                AddSectionHeader(mainStack, "Validity Period");
                AddDetailField(mainStack, "Start Date", discount.StartDate?.ToString("MMM dd, yyyy") ?? "No start date");
                AddDetailField(mainStack, "End Date", discount.EndDate?.ToString("MMM dd, yyyy") ?? "No end date");

                if (discount.EndDate.HasValue)
                {
                    var daysLeft = (discount.EndDate.Value - DateTime.UtcNow).Days;
                    if (daysLeft > 0)
                    {
                        AddDetailField(mainStack, "Days Remaining", $"{daysLeft} days", Windows.UI.Color.FromArgb(255, 34, 197, 94));
                    }
                    else if (daysLeft == 0)
                    {
                        AddDetailField(mainStack, "Status", "Expires today", Windows.UI.Color.FromArgb(255, 251, 146, 60));
                    }
                    else
                    {
                        AddDetailField(mainStack, "Status", "Expired", Windows.UI.Color.FromArgb(255, 239, 68, 68));
                    }
                }

                // Usage Information
                AddSectionHeader(mainStack, "Usage Statistics");
                AddDetailField(mainStack, "Usage Count", discount.UsageCount.ToString());
                AddDetailField(mainStack, "Created At", discount.CreatedAt.ToString("MMM dd, yyyy HH:mm"));

                dialog.Content = new ScrollViewer { Content = mainStack, MaxHeight = 600 };
                await dialog.ShowAsync();
            }
        }

        private void AddSectionHeader(StackPanel parent, string title)
        {
            var headerText = new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 63, 98)),
                Margin = new Thickness(0, 8, 0, 8)
            };
            parent.Children.Add(headerText);
        }

        private void AddDetailField(StackPanel parent, string label, string value, Windows.UI.Color? valueColor = null)
        {
            var grid = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var labelText = new TextBlock
            {
                Text = label + ":",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 102, 102)),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(labelText, 0);
            grid.Children.Add(labelText);

            var valueText = new TextBlock
            {
                Text = value,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                Foreground = valueColor.HasValue
                    ? new Microsoft.UI.Xaml.Media.SolidColorBrush(valueColor.Value)
                    : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0))
            };
            Grid.SetColumn(valueText, 1);
            grid.Children.Add(valueText);

            parent.Children.Add(grid);
        }

        private string FormatDiscountType(DiscountType type)
        {
            return type switch
            {
                DiscountType.PERCENTAGE => "Percentage Discount",
                DiscountType.FIXED_AMOUNT => "Fixed Amount Discount",
                DiscountType.BUY_X_GET_Y => "Buy X Get Y",
                DiscountType.MEMBER_DISCOUNT => "Member Exclusive",
                DiscountType.WHOLESALE_DISCOUNT => "Wholesale Discount",
                _ => type.ToString()
            };
        }

        private string FormatValue(DiscountType type, decimal value)
        {
            return type switch
            {
                DiscountType.PERCENTAGE => $"{value}%",
                DiscountType.FIXED_AMOUNT => $"₫{value:N0}",
                _ => value.ToString()
            };
        }

        private async void EditDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsAdmin) return;

            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is SelectableDiscount wrapper)
            {
                var discount = wrapper.Discount;

                var stackPanel = new StackPanel { Spacing = 16, Width = 450 };

                var errorText = new TextBlock
                {
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                    Visibility = Visibility.Collapsed,
                    Margin = new Thickness(0, 0, 0, 8),
                    TextWrapping = TextWrapping.Wrap
                };

                var codeBox = new TextBox { Header = "Discount Code", Text = discount.Code };
                var nameBox = new TextBox { Header = "Discount Name", Text = discount.Name };
                var descBox = new TextBox { Header = "Description", Text = discount.Description ?? "", AcceptsReturn = true, Height = 60 };

                var typeCombo = new ComboBox
                {
                    Header = "Discount Type",
                    ItemsSource = new[] { DiscountType.PERCENTAGE, DiscountType.FIXED_AMOUNT },
                    SelectedItem = discount.Type,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                var valueBox = new NumberBox { Header = "Value", Value = (double)discount.Value, Minimum = 0 };
                var minPurchaseBox = new NumberBox { Header = "Minimum Purchase (Optional)", Value = (double)(discount.MinPurchase ?? 0), Minimum = 0 };
                var maxDiscountBox = new NumberBox { Header = "Maximum Discount (Optional)", Value = (double)(discount.MaxDiscount ?? 0), Minimum = 0 };

                var startDatePicker = new CalendarDatePicker { Header = "Start Date (Optional)" };
                if (discount.StartDate.HasValue)
                    startDatePicker.Date = new DateTimeOffset(discount.StartDate.Value);

                var endDatePicker = new CalendarDatePicker { Header = "End Date (Optional)" };
                if (discount.EndDate.HasValue)
                    endDatePicker.Date = new DateTimeOffset(discount.EndDate.Value);

                var activeSwitch = new ToggleSwitch
                {
                    Header = "Status",
                    OffContent = "Inactive",
                    OnContent = "Active",
                    IsOn = discount.IsActive
                };

                // Custom styling
                var colorOn = Windows.UI.Color.FromArgb(255, 0, 63, 98);
                var colorHover = Windows.UI.Color.FromArgb(255, 0, 79, 122);
                var colorPressed = Windows.UI.Color.FromArgb(255, 0, 47, 74);

                activeSwitch.Resources["ToggleSwitchFillOn"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorOn);
                activeSwitch.Resources["ToggleSwitchFillOnPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorHover);
                activeSwitch.Resources["ToggleSwitchFillOnPressed"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(colorPressed);

                var memberOnlyCheckBox = new CheckBox
                {
                    Content = "Member Only",
                    IsChecked = discount.MemberOnly
                };

                stackPanel.Children.Add(errorText);
                stackPanel.Children.Add(codeBox);
                stackPanel.Children.Add(nameBox);
                stackPanel.Children.Add(descBox);
                stackPanel.Children.Add(typeCombo);
                stackPanel.Children.Add(valueBox);
                stackPanel.Children.Add(minPurchaseBox);
                stackPanel.Children.Add(maxDiscountBox);
                stackPanel.Children.Add(startDatePicker);
                stackPanel.Children.Add(endDatePicker);
                stackPanel.Children.Add(memberOnlyCheckBox);
                stackPanel.Children.Add(activeSwitch);

                var dialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Edit Discount",
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    PrimaryButtonStyle = this.Resources["DialogPrimaryButtonStyle"] as Style,
                    Content = new ScrollViewer { Content = stackPanel, MaxHeight = 500 }
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

                    // Validation
                    if (string.IsNullOrWhiteSpace(codeBox.Text))
                    {
                        errorText.Text = "⚠️ Discount Code is required!";
                        errorText.Visibility = Visibility.Visible;
                        dialog.Hide();
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(nameBox.Text))
                    {
                        errorText.Text = "⚠️ Discount Name is required!";
                        errorText.Visibility = Visibility.Visible;
                        dialog.Hide();
                        continue;
                    }

                    if (valueBox.Value <= 0)
                    {
                        errorText.Text = "⚠️ Discount Value must be greater than 0!";
                        errorText.Visibility = Visibility.Visible;
                        dialog.Hide();
                        continue;
                    }

                    try
                    {
                        var updatedDiscount = new Discount
                        {
                            Id = discount.Id,
                            Code = codeBox.Text.Trim(),
                            Name = nameBox.Text.Trim(),
                            Description = descBox.Text?.Trim(),
                            Type = (DiscountType)(typeCombo.SelectedItem ?? DiscountType.PERCENTAGE),
                            Value = (decimal)valueBox.Value,
                            MinPurchase = minPurchaseBox.Value > 0 ? (decimal?)minPurchaseBox.Value : null,
                            MaxDiscount = maxDiscountBox.Value > 0 ? (decimal?)maxDiscountBox.Value : null,
                            StartDate = startDatePicker.Date?.DateTime,
                            EndDate = endDatePicker.Date?.DateTime,
                            IsActive = activeSwitch.IsOn,
                            MemberOnly = memberOnlyCheckBox.IsChecked ?? false,
                            CreatedAt = discount.CreatedAt,
                            UpdatedAt = DateTime.UtcNow,
                            UsageCount = discount.UsageCount
                        };

                        await ViewModel.UpdateDiscountCommand.ExecuteAsync(updatedDiscount);

                        // Check if there was an error from ViewModel
                        if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
                        {
                            errorText.Text = $"⚠️ {ViewModel.ErrorMessage}";
                            errorText.Visibility = Visibility.Visible;
                            dialog.Hide();
                            continue;
                        }

                        success = true;
                    }
                    catch (Exception ex)
                    {
                        errorText.Text = ex.Message.Contains("already exists")
                            ? "⚠️ Discount code already exists. Please use a different code."
                            : $"⚠️ Error: {ex.Message}";
                        errorText.Visibility = Visibility.Visible;
                        dialog.Hide();
                    }
                }
            }
        }

        private async void DeleteDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsAdmin) return;

            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is SelectableDiscount wrapper)
            {
                var discount = wrapper.Discount;
                var dialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Delete Discount",
                    Content = $"Are you sure you want to delete discount '{discount.Code}'?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    ViewModel.DeleteDiscountCommand.Execute(discount);
                }
            }
        }


        private void FilterRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.Tag is string filterValue)
            {
                ViewModel.SelectedFilter = filterValue;
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SelectableDiscount wrapper)
            {
                var menuFlyout = new MenuFlyout();

                var viewItem = new MenuFlyoutItem
                {
                    Text = "View",
                    Icon = new SymbolIcon(Symbol.View),
                    Tag = wrapper
                };
                viewItem.Click += ViewDiscount_Click;
                menuFlyout.Items.Add(viewItem);

                // Only add Edit and Delete for Admin
                if (ViewModel.IsAdmin)
                {
                    var editItem = new MenuFlyoutItem
                    {
                        Text = "Edit",
                        Icon = new SymbolIcon(Symbol.Edit),
                        Tag = wrapper
                    };
                    editItem.Click += EditDiscount_Click;
                    menuFlyout.Items.Add(editItem);

                    var deleteItem = new MenuFlyoutItem
                    {
                        Text = "Delete",
                        Icon = new SymbolIcon(Symbol.Delete),
                        Tag = wrapper
                    };
                    deleteItem.Click += DeleteDiscount_Click;
                    menuFlyout.Items.Add(deleteItem);
                }

                menuFlyout.ShowAt(button);
            }
        }
    }
}
