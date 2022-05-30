using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class EmulatorViewViewModel : RoutableViewModelBase
    {
        public EmulatorViewViewModel()
        {
            Machine = new MachineController(new ZxSpectrum48Machine());
            Machine.Start();
        }

        public override string? UrlPathSegment => "Emulator";

        [Reactive]
        public MachineController Machine { get; set; }
    }
}
