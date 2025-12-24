using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MyShop.Core.Models.DTOs;
using System;
using Windows.UI;

namespace MyShop.App.Converters
{
    public class OrderStatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            OrderStatus status;
            
            // Handle enum, string, and int values
            if (value is OrderStatus statusEnum)
            {
                status = statusEnum;
            }
            else if (value is string statusString && Enum.TryParse<OrderStatus>(statusString, true, out var parsed))
            {
                status = parsed;
            }
            else if (value is int statusInt && Enum.IsDefined(typeof(OrderStatus), statusInt))
            {
                status = (OrderStatus)statusInt;
            }
            else
            {
                return new SolidColorBrush(Colors.LightGray);
            }

            return status switch
            {
                OrderStatus.PENDING => new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0x9F, 0x00)), // Light Orange
                OrderStatus.COMPLETED => new SolidColorBrush(Color.FromArgb(0x20, 0x05, 0x96, 0x69)), // Light Green
                OrderStatus.CANCELLED => new SolidColorBrush(Color.FromArgb(0x20, 0xDC, 0x35, 0x45)), // Light Red
                OrderStatus.PROCESSING => new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x78, 0xD4)), // Light Blue
                _ => new SolidColorBrush(Colors.LightGray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OrderStatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            OrderStatus status;
            
            // Handle enum, string, and int values
            if (value is OrderStatus statusEnum)
            {
                status = statusEnum;
            }
            else if (value is string statusString && Enum.TryParse<OrderStatus>(statusString, true, out var parsed))
            {
                status = parsed;
            }
            else if (value is int statusInt && Enum.IsDefined(typeof(OrderStatus), statusInt))
            {
                status = (OrderStatus)statusInt;
            }
            else
            {
                // Fallback for debugging - if still gray, it means value is null or unrecognized type
                return new SolidColorBrush(Colors.LightGray);
            }

            return status switch
            {
                OrderStatus.PENDING => new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x9F, 0x00)), // Orange
                OrderStatus.COMPLETED => new SolidColorBrush(Color.FromArgb(0xFF, 0x05, 0x96, 0x69)), // Green
                OrderStatus.CANCELLED => new SolidColorBrush(Color.FromArgb(0xFF, 0xDC, 0x35, 0x45)), // Red
                OrderStatus.PROCESSING => new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x78, 0xD4)), // Blue
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
