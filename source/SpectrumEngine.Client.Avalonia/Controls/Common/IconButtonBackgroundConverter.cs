using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// Converts the IsChecked boolean value to the color of the icon buttons's border
/// </summary>
public class IconButtonBackgroundConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) return value;
        if (!boolValue) return new SolidColorBrush(Colors.Transparent);
            
        Brush? brush = new SolidColorBrush(Colors.White);
        if (Application.Current!.TryFindResource("IconButtonCheckedBackground", out var res))
        {
            brush = res as Brush;
        }
        return brush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}