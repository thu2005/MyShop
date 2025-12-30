using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.App.Converters;

/// <summary>
/// Converts decimal currency value to formatted string (USD)
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal decimalValue)
        {
            return $"${decimalValue:N2}";
        }
        if (value is double doubleValue)
        {
            return $"${doubleValue:N2}";
        }
        if (value is int intValue)
        {
            return $"${intValue:N2}";
        }
        return "$0.00";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts DateTime to relative time string
/// </summary>
public class DateTimeToRelativeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays}d ago";
            
            return dateTime.ToString("MMM d, yyyy");
        }
        return "";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts Discount object to formatted value string
/// </summary>
public class DiscountDisplayConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MyShop.Core.Models.Discount discount)
        {
            if (discount.Type == MyShop.Core.Models.DiscountType.PERCENTAGE)
            {
                return $"{discount.Value:G29}%"; 
            }
            return $"${discount.Value:N2}";
        }
        return "";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class DiscountTypeDisplayConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MyShop.Core.Models.DiscountType type)
        {
            return type switch
            {
                MyShop.Core.Models.DiscountType.PERCENTAGE => "Percentage",
                MyShop.Core.Models.DiscountType.FIXED_AMOUNT => "Fixed Amount",
                MyShop.Core.Models.DiscountType.BUY_X_GET_Y => "Buy X Get Y",
                MyShop.Core.Models.DiscountType.MEMBER_DISCOUNT => "Member Discount",
                MyShop.Core.Models.DiscountType.WHOLESALE_DISCOUNT => "Wholesale Discount",
                _ => type.ToString()
            };
        }
        return "";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
