using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class MemoryByteBackgroundConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte byteValue) return value;
        if (byteValue == 0) return new SolidColorBrush(Colors.Transparent);
            
        Brush? brush = new SolidColorBrush(Colors.White);
        if (Application.Current!.TryFindResource("RegMemoryBrush", out var res))
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