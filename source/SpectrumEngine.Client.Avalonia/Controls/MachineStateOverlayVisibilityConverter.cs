using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// This class converts the machine state to overlay visibility value
/// </summary>
public class MachineStateOverlayVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MachineController mc && (mc.State != MachineControllerState.Running || mc.IsDebugging);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}