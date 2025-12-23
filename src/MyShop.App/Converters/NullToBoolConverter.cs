using System;
using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;
namespace MyShop.App.Converters;

/// <summary>
/// Converts null to boolean (null = false, not null = true)
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
