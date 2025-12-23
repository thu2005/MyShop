using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;


namespace MyShop.App.Converters;

/// <summary>
/// Converts boolean to color brush (true = green, false = red)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isPositive)
        {
            return new SolidColorBrush(isPositive ? 
                Color.FromArgb(255, 34, 197, 94) :  // Green
                Color.FromArgb(255, 239, 68, 68));   // Red
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to arrow glyph (true = up arrow, false = down arrow)
/// </summary>
public class BoolToArrowGlyphConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isPositive)
        {
            return isPositive ? "\uE70E" : "\uE70D";  // Up/Down arrows
        }
        return "";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
