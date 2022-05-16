using Avalonia.Controls;
using ReactiveUI;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public interface IMainWindow : IScreen
    {
        string Title { get; set; }

        WindowIcon Icon { get; set; }

        Lazy<IMainToolBar> MainToolBar { get; }
    }
}
