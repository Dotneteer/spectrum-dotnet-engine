using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class MemoryRegisterRefVisibleConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not ushort ushortValue ? value : (ushortValue & 0xff) != 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}