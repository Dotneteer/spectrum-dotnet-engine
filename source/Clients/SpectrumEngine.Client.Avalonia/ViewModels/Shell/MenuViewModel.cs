using ReactiveUI.Fody.Helpers;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class MenuViewModel : ViewModelBase, IMenu
    {
        public MenuViewModel()
        {
            Title = string.Empty;            
        }

        [Reactive]
        public string? Title { get; set; }
    }
}
