using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class AddressTooltipConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not ushort ushortValue ? value : $"{ushortValue} - {(ushortValue + 7) & 0xffff}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}