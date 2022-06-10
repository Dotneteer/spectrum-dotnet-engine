using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class IconButtonBorderBrushConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (!boolValue) return new SolidColorBrush(Colors.Transparent);
            
            Brush? brush = new SolidColorBrush(Colors.White);
            if (Application.Current!.TryFindResource("IconButtonCheckedBorderBrush", out var res))
            {
                brush = res as Brush;
            }

            return brush;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}