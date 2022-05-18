using Avalonia.Controls;
using ReactiveUI;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public interface IWindow : IScreen
    {
        string Title { get; set; }

        WindowIcon Icon { get; set; }

        Lazy<IToolBar> MainToolBar { get; }
    }
}
