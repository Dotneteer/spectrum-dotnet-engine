using Avalonia.Controls;
using ReactiveUI;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public interface IWindow : IScreen
    {
        Lazy<IToolBar> ToolBar { get; }
        Lazy<IStatusBar> StatusBar { get; }
        Lazy<IMenu> Menu { get; }

        string Title { get; set; }

        WindowIcon Icon { get; set; }

        bool IsMenuOpened { get; set; }

        bool IsBusy { get; set; }
    }
}
