using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;
using Splat;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public ViewModelBase()
        {
            MainWindow = Locator.Current.GetService<IWindow>()!;
        }

        public IWindow MainWindow { get; }
    }
}
