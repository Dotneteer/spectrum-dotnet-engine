using Avalonia.Controls;
using Avalonia.Shared.PlatformSupport;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.Services;
using Splat;
using System;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class WindowViewModel : ReactiveObject, IWindow
    {
        private readonly IApplicationService applicationService;

        public WindowViewModel() : this(null) { }

        public WindowViewModel(IApplicationService? applicationService = null)
        {
            this.applicationService = applicationService ?? Locator.Current.GetRequiredService<IApplicationService>();

            ToolBar = new Lazy<IMainToolBar>(() => Locator.Current.GetRequiredService<IMainToolBar>());
            StatusBar = new Lazy<IMainStatusBar>(() => Locator.Current.GetRequiredService<IMainStatusBar>());
            Menu = new Lazy<IMainMenu>(() => Locator.Current.GetRequiredService<IMainMenu>());

            this.applicationService.Application.SetCurrentLanguage(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            Router = new RoutingState();

            Icon = new WindowIcon(new AssetLoader().Open(new Uri(this.applicationService.Application.FirstResource<string>("WindowIcon"))));
            Title = this.applicationService.Application.FirstResource<string>("WindowTitle");
            IsMenuOpened = false;
            VerticalScrollBarContentVisibility = false;

            this.applicationService.IsBusyChange.Subscribe(value => IsBusy = value);            
        }

        public Lazy<IMainToolBar> ToolBar { get; }
        public Lazy<IMainStatusBar> StatusBar { get; }
        public Lazy<IMainMenu> Menu { get; }

        public RoutingState Router { get; }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public WindowIcon Icon { get; set; }

        [Reactive]
        public bool IsMenuOpened { get; set; }

        [Reactive]
        public bool IsBusy { get; set; }

        [Reactive]
        public bool VerticalScrollBarContentVisibility { get; set; }
    }
}
