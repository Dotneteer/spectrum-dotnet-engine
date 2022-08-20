using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// Converts a parameter value to a boolean indicating whether the current clock multiplier is the
/// specified parameter value
/// </summary>
public class MachineTypeVisibilityConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return parameter is string stringValue 
            ? stringValue == App.AppViewModel.Machine.Controller!.Machine.MachineId 
            : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}