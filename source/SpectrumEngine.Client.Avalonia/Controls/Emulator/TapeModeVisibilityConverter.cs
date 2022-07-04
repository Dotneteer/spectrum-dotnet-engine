using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class TapeModeVisibilityConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TapeMode tapemode)
        {
            return tapemode != TapeMode.Passive;
        }
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}