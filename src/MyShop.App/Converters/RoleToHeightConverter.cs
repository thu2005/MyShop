using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.App.Converters
{
    public class RoleToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isAdmin)
            {
                return isAdmin ? 340.0 : 300.0;
            }
            return 340.0; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
