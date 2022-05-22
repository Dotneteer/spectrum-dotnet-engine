using ReactiveUI.Fody.Helpers;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class MenuViewModel : ViewModelBase, IMenu
    {
        public MenuViewModel()
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
