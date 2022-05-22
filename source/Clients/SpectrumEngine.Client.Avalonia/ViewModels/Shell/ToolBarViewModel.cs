using ReactiveUI;
using System;
using ReactiveUI.Fody.Helpers;
using System.Reactive;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class ToolBarViewModel : ViewModelBase, IToolBar
    {
        public ToolBarViewModel()
        {
            this.Title = string.Empty;

            OpenMenuCmd = ReactiveCommand.Create(() => this.MainWindow.IsMenuOpened = true);
        }

        public ReactiveCommand<Unit, bool> OpenMenuCmd { get; private set; }

        [Reactive]
        public string? Title { get; set; }
    }
}
