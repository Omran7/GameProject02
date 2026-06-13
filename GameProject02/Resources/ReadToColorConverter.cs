using System.Globalization;
using Microsoft.Maui.Controls;

namespace GameProject02.Converters;

public class ReadToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isRead = (bool)value;
        return isRead ? Color.FromArgb("#0a0a0a") : Color.FromArgb("#1a1a1a");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
