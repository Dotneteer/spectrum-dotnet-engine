using Avalonia;
using ReactiveUI;
using SpectrumEngine.Client.Avalonia.Models;
using SpectrumEngine.Client.Avalonia.Providers;
using SpectrumEngine.Client.Avalonia.Services;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell.ToolBars;
using SpectrumEngine.Client.Avalonia.Views;
using SpectrumEngine.Emu;
using Splat;

namespace SpectrumEngine.Client.Avalonia
{
    public static class AppBuilderExtensions
    {

        public static AppBuilder RegisterComponents(this AppBuilder appBuilder)
        {
            // Shell            
            Locator.CurrentMutable.RegisterLazySingleton<IMainMenu>(() => new MainMenuViewModel());
            Locator.CurrentMutable.RegisterLazySingleton<IMainStatusBar>(() => new MainStatusBarViewModel());
            Locator.CurrentMutable.RegisterLazySingleton<IWindow>(() => new WindowViewModel());
            Locator.CurrentMutable.RegisterLazySingleton<IMainToolBar>(() => new MainToolBarViewModel());
            Locator.CurrentMutable.RegisterLazySingleton<IToolBar>(() => new ExecutionToolBarViewModel(), nameof(ExecutionToolBarViewModel));

            // keyboards
            Locator.CurrentMutable.Register<IKeyboardProvider, ZxSpectrum48Provider>(Machine.ZxSpectrum48.ToString());

            // Machines
            Locator.CurrentMutable.Register<IZ80Machine, ZxSpectrum48Machine>(Machine.ZxSpectrum48.ToString());

            // services
            Locator.CurrentMutable.RegisterLazySingleton<IApplicationService>(() => new ApplicationService());
            Locator.CurrentMutable.RegisterLazySingleton<IEmulatorService>(() => new EmulatorService());

            // Views
            Locator.CurrentMutable.Register(() => new EmulatorView(), typeof(IViewFor<EmulatorViewViewModel>));

            return appBuilder;
        }
    }
}
