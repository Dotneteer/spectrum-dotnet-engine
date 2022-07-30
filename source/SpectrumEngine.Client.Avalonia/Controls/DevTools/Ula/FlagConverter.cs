using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// This converter gets the specified bit (in parameter) of the value and converts it to a Boolean value
/// </summary>
public class FlagConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte byteVal || parameter is not string strVal) return value;
        if (int.TryParse(strVal, out var bitIndex))
        {
            return ((1 << bitIndex) & byteVal) != 0;
        }
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}