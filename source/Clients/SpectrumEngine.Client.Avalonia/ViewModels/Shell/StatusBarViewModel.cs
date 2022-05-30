using ReactiveUI.Fody.Helpers;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class StatusBarViewModel : ViewModelBase, IStatusBar
    {
        public StatusBarViewModel()
        {
            Slot1 = string.Empty;
        }

        [Reactive]
        public string? Slot1 { get; set; }
    }
}
