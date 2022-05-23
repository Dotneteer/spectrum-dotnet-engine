using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Shared.PlatformSupport;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.Services;
using SpectrumEngine.Emu;
using Splat;
using System;
using System.Reactive;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class WindowViewModel : ReactiveObject, IWindow
    {
        private readonly IApplicationService applicationService;

        public WindowViewModel(IApplicationService? applicationService = null)
        {
            this.applicationService = applicationService ?? Locator.Current.GetRequiredService<IApplicationService>();

            ToolBar = new Lazy<IToolBar>(() => Locator.Current.GetRequiredService<IToolBar>());
            StatusBar = new Lazy<IStatusBar>(() => Locator.Current.GetRequiredService<IStatusBar>());
            Menu = new Lazy<IMenu>(() => Locator.Current.GetRequiredService<IMenu>());

            this.applicationService.Application.SetCurrentLanguage(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            Router = new RoutingState();
            Router.NavigateAndReset.Execute(new FirstViewModel());

            Icon = new WindowIcon(new AssetLoader().Open(new Uri(this.applicationService.Application.FirstResource<string>("WindowIcon"))));
            Title = this.applicationService.Application.FirstResource<string>("WindowTitle");
            IsMenuOpened = false;
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
        public bool IsBusy { get; set; }
    }
}
