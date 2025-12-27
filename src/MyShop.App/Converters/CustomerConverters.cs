using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace MyShop.App.Converters
{
    public class BoolToMemberStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isMember = value is bool b && b;
            string? param = parameter as string;

            if (param == "Background")
            {
                // Member: Light Emerald (#ECFDF5), Standard: Light Slate (#F1F5F9)
                return isMember ? new SolidColorBrush(Color.FromArgb(255, 236, 253, 245)) : new SolidColorBrush(Color.FromArgb(255, 241, 245, 249));
            }
            else // Foreground
            {
                // Member: Emerald 700 (#047857), Standard: Slate 600 (#475569)
                return isMember ? new SolidColorBrush(Color.FromArgb(255, 4, 120, 87)) : new SolidColorBrush(Color.FromArgb(255, 71, 85, 105));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToMemberStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool b && b) ? "Member" : "Standard";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                return dt.ToString("MMM dd, yyyy");
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public class BoolToSelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool b && b) ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is ListViewSelectionMode mode && mode == ListViewSelectionMode.Multiple);
        }
    }
}
