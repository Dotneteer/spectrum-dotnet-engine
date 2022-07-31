using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class IsInactiveBreakpointConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var address = value switch
        {
            int intValue => intValue,
            ushort ushortValue => ushortValue,
            _ => -1
        };
        return address >= 0 ? App.AppViewModel.Cpu!.PC != address : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}