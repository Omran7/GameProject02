using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GameProject02.Converters;

public class BoolToAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCurrentPlayer)
        {
            return isCurrentPlayer ? TextAlignment.End : TextAlignment.Start;
        }
        return TextAlignment.Start;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}