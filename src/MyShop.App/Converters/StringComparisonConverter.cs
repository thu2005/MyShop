using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using Windows.UI;

namespace MyShop.App.Converters;

/// <summary>
/// Converter to compare two strings and return accent color if equal
/// </summary>
public class StringComparisonToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
            return new SolidColorBrush(Colors.White);

        bool isEqual = value.ToString() == parameter.ToString();
        
        if (isEqual)
        {
            // Return #1D546C for selected button
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 29, 84, 108));
        }
        else
        {
            // Return white for non-selected
            return new SolidColorBrush(Colors.White);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to compare two strings and return white or default text color
/// </summary>
public class StringComparisonToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
            return new SolidColorBrush(Colors.Black);

        bool isEqual = value.ToString() == parameter.ToString();
        
        if (isEqual)
        {
            return new SolidColorBrush(Colors.White);
        }
        else
        {
            return new SolidColorBrush(Colors.Black);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
