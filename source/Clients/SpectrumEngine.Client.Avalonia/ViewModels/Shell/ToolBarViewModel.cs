using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class ToolBarViewModel : ViewModelBase, IToolBar
    {
        public ToolBarViewModel()
        {
            OpenMenuCmd = ReactiveCommand.Create(() => this.MainWindow.IsMenuOpened = true);
            IsExecutionToolsVisible = false;
        }

        public ReactiveCommand<Unit, bool> OpenMenuCmd { get; private set; }


        [Reactive]
        public bool IsExecutionToolsVisible { get; set; }
    }
}
