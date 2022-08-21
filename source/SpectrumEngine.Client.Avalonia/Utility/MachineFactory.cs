using SpectrumEngine.Client.Avalonia.Providers;
using SpectrumEngine.Emu;
using SpectrumEngine.Emu.ZxSpectrum128;

namespace SpectrumEngine.Client.Avalonia.Utility;

/// <summary>
/// This class is responsible for creating and configuring machine instances
/// </summary>
public static class MachineFactory
{
    public static IZ80Machine? CreateMachine(string id)
    {
        // --- Create the machine instance
        IZ80Machine? machine = id.ToLower() switch
        {
            "sp48" => new ZxSpectrum48Machine(),
            "sp128" => new ZxSpectrum128Machine(),
            _ => null
        };

        // --- Unknown machine, return with fail
        if (machine == null) return null;
        
        // --- Set up the machine, provided it supports a particular machine property
        machine.SetMachineProperty(MachinePropNames.TapeSaver, new DefaultTapeSaver());
        
        // --- Create a controller for the machine and let the main view model to know it
        var vm = App.AppViewModel;
        var controller = new MachineController(machine)
        {
            DebugSupport = vm.Debugger
        };
        vm.Machine.SetMachineController(controller);
        return machine;
    }
}