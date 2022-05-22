using Avalonia;
using ReactiveUI;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;
using Splat;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public ViewModelBase()
        {
            App = Application.Current ?? throw new InvalidProgramException($"Application.Current is null on {this.GetType()}");
            MainWindow = Locator.Current.GetService<IWindow>()!;
        }

        public Application App { get; }

        public IWindow MainWindow { get; }
    }
}
