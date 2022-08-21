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
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MachineController mc)
        {
            if (mc.State != MachineControllerState.Running || mc.IsDebugging)
            {
                return true;
            }
            var tapeMode = mc.Machine.GetMachineProperty(MachinePropNames.TapeMode);
            return tapeMode is TapeMode tp && tp != TapeMode.Passive && mc.State == MachineControllerState.Running;
        }
        return value;
        // return value is not MachineController mc
        //     ? value
        //     : mc.State != MachineControllerState.Running || mc.IsDebugging ;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}