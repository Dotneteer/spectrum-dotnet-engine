using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class PanelsDockConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool boolValue ? boolValue ? Dock.Bottom : Dock.Top : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}