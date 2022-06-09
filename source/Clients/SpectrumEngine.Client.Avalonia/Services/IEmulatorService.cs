using SpectrumEngine.Client.Avalonia.Models;
using SpectrumEngine.Client.Avalonia.Providers;
using SpectrumEngine.Emu;
using System.Reactive.Subjects;

namespace SpectrumEngine.Client.Avalonia.Services
{
    /// <summary>
    /// Service for manage Emulators
    /// </summary>
    public interface IEmulatorService
    {
        /// <summary>
        /// Subject for track Keyboard Provider changes
        /// </summary>
        Subject<IKeyboardProvider> IskeyboardProviderChange { get; }

        /// <summary>
        /// Subject for track machine emulator controller changes
        /// </summary>
        Subject<MachineController> IsMachineChange { get; }

        /// <summary>
        /// subject for track machin state changes
        /// </summary>
        Subject<MachineControllerState> IsMachineControllerStateChange { get; }

        /// <summary>
        /// Get keyboard provider for current machine
        /// </summary>
        IKeyboardProvider? KeyboardProvider { get; }

        /// <summary>
        /// Get current machine
        /// </summary>
        Machine? Machine { get; set; }

        /// <summary>
        /// Get machine controller for current machine
        /// </summary>
        MachineController? MachineController { get; }
        
    }
}