using ReactiveUI.Fody.Helpers;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class MainStatusBarViewModel : ViewModelBase, IMainStatusBar
    {
        public MainStatusBarViewModel()
        {
            Slot1 = string.Empty;
        }

        [Reactive]
        public string? Slot1 { get; set; }
    }
}
