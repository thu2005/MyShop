using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyShop.App.Views.Dialogs
{
    public sealed partial class OnboardingDialog : ContentDialog
    {
        private ObservableCollection<double> _dotOpacities = new();

        public OnboardingDialog(UserRole role)
        {
            this.InitializeComponent();
            DotsIndicator.ItemsSource = _dotOpacities;
            
            this.Loaded += (s, e) => InitializeSlides(role);
        }

        private void InitializeSlides(UserRole role)
        {
            if (OnboardingFlipView == null) return;

            // Remove slides not intended for the current role
            var slidesToRemove = OnboardingFlipView.Items
                .Cast<FlipViewItem>()
                .Where(item => 
                {
                    string? tag = item.Tag as string;
                    if (tag == "ALL") return false;
                    if (tag == "ADMIN" && role == UserRole.ADMIN) return false;
                    if (tag == "STAFF" && role == UserRole.STAFF) return false;
                    return true;
                })
                .ToList();

            foreach (var slide in slidesToRemove)
            {
                OnboardingFlipView.Items.Remove(slide);
            }

            // Initialize dots
            UpdateIndicators();
        }

        private void OnboardingFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateIndicators();
        }

        private void UpdateIndicators()
        {
            if (OnboardingFlipView == null || DotsIndicator == null) return;

            int count = OnboardingFlipView.Items.Count;
            int index = OnboardingFlipView.SelectedIndex;

            _dotOpacities.Clear();
            for (int i = 0; i < count; i++)
            {
                _dotOpacities.Add(i == index ? 1.0 : 0.3);
            }

            // Update secondary button text
            if (index == count - 1)
            {
                SecondaryButtonText = "Get Started";
            }
            else
            {
                SecondaryButtonText = "Next";
            }
        }
    }
}
