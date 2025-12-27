using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Models;
using System;

namespace MyShop.App.Views.Dialogs
{
    public sealed partial class AddCategoryDialog : ContentDialog
    {
        public Category NewCategory { get; private set; }

        public AddCategoryDialog()
        {
            this.InitializeComponent();
        }

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validate required field
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                NameBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                args.Cancel = true;
                return;
            }

            // Create new category
            NewCategory = new Category
            {
                Name = NameBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim(),
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
