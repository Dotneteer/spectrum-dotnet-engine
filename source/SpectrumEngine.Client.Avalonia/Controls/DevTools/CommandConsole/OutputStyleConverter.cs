using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class OutputStyleConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool boolValue
            ? boolValue ? FontStyle.Italic : FontStyle.Normal
            : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}