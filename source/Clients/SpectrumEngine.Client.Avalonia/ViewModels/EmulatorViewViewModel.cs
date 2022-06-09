using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.Providers;
using SpectrumEngine.Client.Avalonia.Services;
using SpectrumEngine.Emu;
using Splat;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class EmulatorViewViewModel : RoutableViewModelBase
    {
        private readonly IEmulatorService emulatorService;

        public EmulatorViewViewModel(IEmulatorService? emulatorService = null)
        {
            this.emulatorService = emulatorService ?? Locator.Current.GetRequiredService<IEmulatorService>();            

            this.emulatorService.IsMachineChange.Subscribe(value => Machine = value);
            this.emulatorService.IskeyboardProviderChange.Subscribe(value => KeyboardProvider = value);

            this.emulatorService.Machine = Models.Machine.ZxSpectrum48;
            if (this.emulatorService.MachineController?.State != MachineControllerState.Running) this.emulatorService.MachineController?.Start();
        }

        public override string? UrlPathSegment => "Emulator";

        [Reactive]
        public MachineController? Machine { get; set; }

        [Reactive]
        public IKeyboardProvider? KeyboardProvider { get; set; }
    }
}
