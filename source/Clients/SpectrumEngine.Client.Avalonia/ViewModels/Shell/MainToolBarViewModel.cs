using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell.ToolBars;
using Splat;
using System;
using System.Reactive;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class MainToolBarViewModel : ViewModelBase, IMainToolBar
    {
        public MainToolBarViewModel()
        {
            ExecutionToolbar = new Lazy<IToolBar>(() => Locator.Current.GetRequiredService<IToolBar>(nameof(ExecutionToolBarViewModel)));

            OpenMenuCmd = ReactiveCommand.Create(() => this.MainWindow.IsMenuOpened = true);
            IsExecutionToolsVisible = false;
        }

        public ReactiveCommand<Unit, bool> OpenMenuCmd { get; private set; }

        [Reactive]
        public bool IsExecutionToolsVisible { get; set; }

        public Lazy<IToolBar> ExecutionToolbar { get; }
    }
}
