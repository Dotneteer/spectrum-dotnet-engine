using Avalonia;
using ReactiveUI;
using SpectrumEngine.Client.Avalonia.Services;
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
            Locator.CurrentMutable.RegisterLazySingleton<IMenu>(() => new MenuViewModel());
            Locator.CurrentMutable.RegisterLazySingleton<IStatusBar>(() => new StatusBarViewModel());
            Locator.CurrentMutable.RegisterLazySingleton<IWindow>(() => new WindowViewModel());

            // services
            Locator.CurrentMutable.RegisterLazySingleton<IApplicationService>(() => new ApplicationService());

            // Views
            Locator.CurrentMutable.Register(() => new EmulatorView(), typeof(IViewFor<EmulatorViewViewModel>));

            return appBuilder;
        }
    }
}
