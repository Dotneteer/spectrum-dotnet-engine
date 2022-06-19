using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// Converts a parameter value to a boolean indicating whether the current clock multiplier is the
/// specified parameter value
/// </summary>
public class ClockMultiplierConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return parameter is string stringValue && int.TryParse(stringValue, out var intValue) 
            ? intValue == App.AppViewModel.Machine.ClockMultiplier 
            : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}