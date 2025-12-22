using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using MyShop.Core.Models;
using System;

namespace MyShop.App.Converters
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UserRole userRole && parameter is string requiredRoles)
            {
                // parameter can be a comma-separated list like "ADMIN,MANAGER"
                var roles = requiredRoles.Split(',');
                foreach (var role in roles)
                {
                    if (Enum.TryParse<UserRole>(role.Trim(), true, out var required) && userRole == required)
                    {
                        return Visibility.Visible;
                    }
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
