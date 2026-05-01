using Microsoft.Maui.Controls;
using System.Globalization;

namespace GameProject02.Converters;

public class LockedToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Color.FromArgb("#3d3d3d") : Color.FromArgb("#2c2c2c");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
