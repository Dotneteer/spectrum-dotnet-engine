using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.Keyboards;
using SpectrumEngine.Emu;
using Splat;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class EmulatorViewViewModel : RoutableViewModelBase
    {
        public EmulatorViewViewModel()
        {
            Machine = new MachineController(new ZxSpectrum48Machine());           
            Machine.Start();

            KeyboardProvider = Locator.Current.GetRequiredService<IKeyboardProviderManager>();
            KeyboardProvider.Keyboard = Keyboard.ZxSpectrum48;
        }

        public override string? UrlPathSegment => "Emulator";

        [Reactive]
        public MachineController Machine { get; set; }

        [Reactive]
        public IKeyboardProviderManager KeyboardProvider { get; set; }
    }
}
