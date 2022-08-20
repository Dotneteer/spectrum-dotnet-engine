using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class MemoryValueToolTipConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ushort ushortValue) return value;
        var byteVal = ushortValue >> 8;
        return $"{byteVal:X2} ({byteVal}, %{System.Convert.ToString(byteVal, 2)})";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}