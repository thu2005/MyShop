using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.App.Converters;

/// <summary>
/// Converts decimal currency value to formatted string
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString("N0");
        }
        if (value is double doubleValue)
        {
            return doubleValue.ToString("N0");
        }
        if (value is int intValue)
        {
            return intValue.ToString("N0");
        }
        return "0";
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
