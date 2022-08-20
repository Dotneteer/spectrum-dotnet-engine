using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class MemoryRegisterRefToolTipConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ushort ushortValue) return value;
        var flags = ushortValue & 0xff;
        if (flags == 0) return null;
        var regList = new List<string>();
        if ((flags & 0x01) != 0) regList.Add("BC");
        if ((flags & 0x02) != 0) regList.Add("DE");
        if ((flags & 0x04) != 0) regList.Add("HL");
        if ((flags & 0x08) != 0) regList.Add("SP");
        if ((flags & 0x10) != 0) regList.Add("IX");
        if ((flags & 0x20) != 0) regList.Add("IY");
        if ((flags & 0x40) != 0) regList.Add("PC");
        return "<-- " + string.Join(", ", regList);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}