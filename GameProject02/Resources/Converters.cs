using Microsoft.Maui.Controls;
using System.Globalization;

namespace GameProject02.Resources;

// Convert locked state to background color
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

// Convert locked state to sell button color
public class LockedToSellColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Color.FromArgb("#95a5a6") : Color.FromArgb("#e74c3c");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Inverse boolean converter (for IsEnabled binding)
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }
}