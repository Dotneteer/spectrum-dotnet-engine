using Avalonia;
using Avalonia.Controls;
using Avalonia.Shared.PlatformSupport;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using Splat;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class MainWindowViewModel : ReactiveObject, IMainWindow
    {
        private readonly Application app;

        public MainWindowViewModel()
        {
            app = Application.Current ?? throw new InvalidProgramException("Application.Current is null on MainWindowViewModel");

            MainToolBar = new Lazy<IMainToolBar>(() => Locator.Current.GetService<IMainToolBar>()!);

            app.SetCurrentLanguage(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            Icon = new WindowIcon(new AssetLoader().Open(new Uri(app.FirstResource<string>("MainWindowIcon"))));
            Title = app.FirstResource<string>("MainWindowTitle");            

            Router = new RoutingState();
            Router.NavigateAndReset.Execute(new FirstViewModel());
        }

        public Lazy<IMainToolBar> MainToolBar { get; }

        public RoutingState Router { get; }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public WindowIcon Icon { get; set; }

        public bool IsBusy { get; set; } = false;
    }
}
