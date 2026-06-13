using System;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace GameProject02.Converters;

public class IsCurrentUserToBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCurrentPlayer)
        {
            // Player's messages: green background
            // Other player's messages: gray background
            return isCurrentPlayer
                ? Color.FromArgb("#27ae60")  // Green for current player
                : Color.FromArgb("#34495e");  // Gray for other player
        }
        return Color.FromArgb("#34495e");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}