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
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.PENDING => new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0x9F, 0x00)), // Light Orange
                    OrderStatus.COMPLETED => new SolidColorBrush(Color.FromArgb(0x20, 0x05, 0x96, 0x69)), // Light Green
                    OrderStatus.CANCELLED => new SolidColorBrush(Color.FromArgb(0x20, 0xDC, 0x35, 0x45)), // Light Red
                    OrderStatus.PROCESSING => new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x78, 0xD4)), // Light Blue
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
            
             if (value != null && Enum.TryParse<OrderStatus>(value.ToString(), out var parsedStatus))
            {
                 return parsedStatus switch
                {
                    OrderStatus.PENDING => new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0x9F, 0x00)),
                    OrderStatus.COMPLETED => new SolidColorBrush(Color.FromArgb(0x20, 0x05, 0x96, 0x69)),
                    OrderStatus.CANCELLED => new SolidColorBrush(Color.FromArgb(0x20, 0xDC, 0x35, 0x45)),
                    OrderStatus.PROCESSING => new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x78, 0xD4)),
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }

            return new SolidColorBrush(Colors.LightGray);
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
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.PENDING => new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x9F, 0x00)), // Orange
                    OrderStatus.COMPLETED => new SolidColorBrush(Color.FromArgb(0xFF, 0x05, 0x96, 0x69)), // Green
                    OrderStatus.CANCELLED => new SolidColorBrush(Color.FromArgb(0xFF, 0xDC, 0x35, 0x45)), // Red
                    OrderStatus.PROCESSING => new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x78, 0xD4)), // Blue
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            
            // Try parsing string or int if direct cast fails
             if (value != null && Enum.TryParse<OrderStatus>(value.ToString(), out var parsedStatus))
            {
                 return parsedStatus switch
                {
                    OrderStatus.PENDING => new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x9F, 0x00)),
                    OrderStatus.COMPLETED => new SolidColorBrush(Color.FromArgb(0xFF, 0x05, 0x96, 0x69)),
                    OrderStatus.CANCELLED => new SolidColorBrush(Color.FromArgb(0xFF, 0xDC, 0x35, 0x45)),
                    OrderStatus.PROCESSING => new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x78, 0xD4)),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }

            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OrderStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.PENDING => "Pending",
                    OrderStatus.PROCESSING => "Processing",
                    OrderStatus.COMPLETED => "Completed",
                    OrderStatus.CANCELLED => "Cancelled",
                    _ => status.ToString()
                };
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
