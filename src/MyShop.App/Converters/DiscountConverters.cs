using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MyShop.Core.Models;
using System;

namespace MyShop.App.Converters
{
    /// <summary>
    /// Converts Discount object to status color based on IsActive and date range
    /// </summary>
    public class DiscountStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Discount discount)
            {
                var now = DateTime.UtcNow;

                // Check if discount has expired (highest priority)
                if (discount.EndDate.HasValue && discount.EndDate.Value < now)
                {
                    return new SolidColorBrush(Colors.Red);
                }

                // Check if discount is inactive
                if (!discount.IsActive)
                {
                    return new SolidColorBrush(Colors.Gray);
                }

                // Check if discount hasn't started yet
                if (discount.StartDate.HasValue && discount.StartDate.Value > now)
                {
                    return new SolidColorBrush(Colors.Orange);
                }

                // Discount is currently active
                return new SolidColorBrush(Colors.Green);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts Discount object to status label based on IsActive and date range
    /// </summary>
    public class DiscountStatusLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Discount discount)
            {
                var now = DateTime.UtcNow;

                // Check if discount has expired (highest priority)
                if (discount.EndDate.HasValue && discount.EndDate.Value < now)
                {
                    return "Expired";
                }

                // Check if discount is inactive
                if (!discount.IsActive)
                {
                    return "Inactive";
                }

                // Check if discount hasn't started yet
                if (discount.StartDate.HasValue && discount.StartDate.Value > now)
                {
                    return "Scheduled";
                }

                // Discount is currently active
                return "Active";
            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts Discount object to formatted value string (e.g., "10%" or "$50")
    /// </summary>
    public class DiscountValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Discount discount)
            {
                return discount.Type switch
                {
                    DiscountType.PERCENTAGE => $"{discount.Value}%",
                    DiscountType.FIXED_AMOUNT => $"${discount.Value:N0}",
                    _ => discount.Value.ToString()
                };
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
    /// <summary>
    /// Converts Discount object to formatted display value with type prefix
    /// </summary>
    public class DiscountValueDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Discount discount)
            {
                var valueText = discount.Type switch
                {
                    DiscountType.PERCENTAGE => $"{discount.Value}% off",
                    DiscountType.FIXED_AMOUNT => $"${discount.Value:N2} off",
                    _ => discount.Value.ToString()
                };

                // Add min purchase info if exists
                if (discount.MinPurchase.HasValue && discount.MinPurchase.Value > 0)
                {
                    valueText += $" (min: ${discount.MinPurchase.Value:N2})";
                }

                return valueText;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
