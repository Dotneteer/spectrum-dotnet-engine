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

            MainToolBar = new Lazy<IToolBar>(() => Locator.Current.GetService<IToolBar>()!);

            app.SetCurrentLanguage(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            Icon = new WindowIcon(new AssetLoader().Open(new Uri(app.FirstResource<string>("WindowIcon"))));
            Title = app.FirstResource<string>("WindowTitle");            

            Router = new RoutingState();
            Router.NavigateAndReset.Execute(new FirstViewModel());
        }

        public Lazy<IToolBar> MainToolBar { get; }

        public RoutingState Router { get; }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public WindowIcon Icon { get; set; }

        public bool IsBusy { get; set; } = false;
    }
}
