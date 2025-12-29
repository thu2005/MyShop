using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Models;
using System;

namespace MyShop.App.Views.Dialogs
{
    public sealed partial class AddCategoryDialog : ContentDialog
    {
        public Category NewCategory { get; set; }

        public AddCategoryDialog()
        {
            this.InitializeComponent();
        }

        public void ResetForRetry()
        {
            NewCategory = null;
            ErrorText.Visibility = Visibility.Collapsed;
        }

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validate required field
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                ErrorText.Text = "Category name cannot be empty.";
                ErrorText.Visibility = Visibility.Visible;
                NameBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                args.Cancel = true;
                return;
            }

            // Reset error state
            ErrorText.Visibility = Visibility.Collapsed;
            NameBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 229, 231, 235)); // #E5E7EB

            // Create new category
            NewCategory = new Category
            {
                Name = NameBox.Text.Trim(),
                Description = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private void OnCloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            NewCategory = null;
        }
    }
}
