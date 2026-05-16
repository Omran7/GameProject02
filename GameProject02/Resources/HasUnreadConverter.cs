using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GameProject02.Converters
{
    public class HasUnreadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int count && count > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}