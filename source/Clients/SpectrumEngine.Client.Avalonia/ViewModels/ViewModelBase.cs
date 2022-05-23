using Avalonia;
using ReactiveUI;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;
using Splat;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public ViewModelBase()
        {
            MainWindow = Locator.Current.GetRequiredService<IWindow>();
        }

        public IWindow MainWindow { get; }
    }
}
