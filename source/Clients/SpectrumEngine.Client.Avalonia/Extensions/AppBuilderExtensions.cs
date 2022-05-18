using Avalonia;
using ReactiveUI;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;
using SpectrumEngine.Client.Avalonia.Views;
using Splat;

namespace SpectrumEngine.Client.Avalonia
{
    public static class AppBuilderExtensions
    {

        public static AppBuilder RegisterComponents(this AppBuilder appBuilder)
        {
            // Shell
            Locator.CurrentMutable.RegisterLazySingleton<IToolBar>(() => new ToolBarViewModel());
            Locator.CurrentMutable.RegisterLazySingleton<IWindow>(() => new WindowViewModel());

            // Views
            Locator.CurrentMutable.Register(() => new FirstView(), typeof(IViewFor<FirstViewModel>));


            return appBuilder;
        }
    }
}
