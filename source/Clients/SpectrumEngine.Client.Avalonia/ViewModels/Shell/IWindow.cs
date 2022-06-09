using Avalonia.Controls;
using ReactiveUI;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public interface IWindow : IScreen
    {
        Lazy<IMainToolBar> ToolBar { get; }
        Lazy<IMainStatusBar> StatusBar { get; }
        Lazy<IMainMenu> Menu { get; }

        string Title { get; set; }

        WindowIcon Icon { get; set; }

        bool IsMenuOpened { get; set; }
        bool VerticalScrollBarContentVisibility { get; set; }
    }
}
