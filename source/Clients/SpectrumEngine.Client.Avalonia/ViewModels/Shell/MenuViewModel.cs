using ReactiveUI.Fody.Helpers;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class MenuViewModel : ViewModelBase, IMenu
    {
        public MenuViewModel()
        {
            SetDefaultValues();
        }

        [Reactive]
        public string? Title { get; set; }

        [Reactive]
        public bool Visibility { get; set; }

        private void SetDefaultValues()
        {
            this.Title = string.Empty;
            this.Visibility = true;
        }
    }
}
