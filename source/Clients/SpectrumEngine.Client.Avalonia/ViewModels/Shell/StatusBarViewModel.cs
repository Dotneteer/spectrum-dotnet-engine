using ReactiveUI.Fody.Helpers;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class StatusBarViewModel : ViewModelBase, IStatusBar
    {
        public StatusBarViewModel()
        {
            Title = string.Empty;
        }

        [Reactive]
        public string? Title { get; set; }
    }
}
