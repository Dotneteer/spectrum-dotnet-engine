using ReactiveUI.Fody.Helpers;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class StatusBarViewModel : ViewModelBase, IStatusBar
    {
        public StatusBarViewModel()
        {
            Title = string.Empty;
            Visibility = true;
        }

        [Reactive]
        public string? Title { get; set; }

        [Reactive]
        public bool Visibility { get; set; }
    }
}
