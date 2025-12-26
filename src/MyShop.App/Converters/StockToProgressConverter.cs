using Microsoft.UI.Xaml.Data;
using MyShop.Core.Models;
using System;

namespace MyShop.App.Converters
{
    public class StockToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Product product)
            {
                int total = product.Stock + product.Popularity;
                if (total == 0) return 0.0;
                
                // Max width for progress bar is 100px, calculate percentage
                double percentage = (double)product.Stock / total;
                return percentage * 100.0;
            }
            return 50.0; // Default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
