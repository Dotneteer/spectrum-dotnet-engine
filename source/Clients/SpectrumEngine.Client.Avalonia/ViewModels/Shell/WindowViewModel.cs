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
    public class WindowViewModel : ReactiveObject, IWindow
    {
        private readonly Application app;

        public WindowViewModel()
        {
            app = Application.Current ?? throw new InvalidProgramException("Application.Current is null on WindowViewModel");

            ToolBar = new Lazy<IToolBar>(() => Locator.Current.GetService<IToolBar>()!);
            StatusBar = new Lazy<IStatusBar>(() => Locator.Current.GetService<IStatusBar>()!);
            Menu = new Lazy<IMenu>(() => Locator.Current.GetService<IMenu>()!);

            app.SetCurrentLanguage(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            Router = new RoutingState();
            Router.NavigateAndReset.Execute(new FirstViewModel());

            Icon = new WindowIcon(new AssetLoader().Open(new Uri(app.FirstResource<string>("WindowIcon"))));
            Title = app.FirstResource<string>("WindowTitle");
            IsMenuOpened = false;
            IsBusy = false;
        }

        public Lazy<IToolBar> ToolBar { get; }
        public Lazy<IStatusBar> StatusBar { get; }
        public Lazy<IMenu> Menu { get; }

        public RoutingState Router { get; }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public WindowIcon Icon { get; set; }

        [Reactive]
        public bool IsMenuOpened { get; set; }

        [Reactive]
        public bool IsBusy { get; set; } = false;
    }
}
