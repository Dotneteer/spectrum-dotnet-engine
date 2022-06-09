using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.Models;
using System.Collections.Generic;
using System;
using SpectrumEngine.Client.Avalonia.Services;
using Splat;
using System.Linq;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class MainMenuViewModel : ViewModelBase, IMainMenu
    {
        public enum MenuItem
        {
            Unknown,
            Emulator,
            Exit
        }

        private readonly IApplicationService applicationService;

        public MainMenuViewModel() : this(null) { }

        public MainMenuViewModel(IApplicationService? applicationService = null)
        {
            this.applicationService = applicationService ?? Locator.Current.GetRequiredService<IApplicationService>();

            Title = string.Empty;
            MenuItems = CreateMenuItems();
            this.WhenAnyValue(x => x.SelectedMenuItem).Subscribe(OnSelectedMenuItem);

            SelectedMenuItem = MenuItems.First();
        }

        [Reactive]
        public string? Title { get; set; }

        [Reactive]
        public IEnumerable<MenuItemModel> MenuItems { get; set; }

        [Reactive]
        public MenuItemModel? SelectedMenuItem { get; set; }

        private IEnumerable<MenuItemModel> CreateMenuItems() => new List<MenuItemModel>()
            {
                new MenuItemModel((int)MenuItem.Emulator, MaterialIconKind.ControllerClassicOutline,  applicationService.Application.FirstResource<string>($"Menu{MenuItem.Emulator}Item")),
                new MenuItemModel((int)MenuItem.Exit, MaterialIconKind.ExitToApp,  applicationService.Application.FirstResource<string>($"Menu{MenuItem.Exit}Item")),
            };

        private void OnSelectedMenuItem(MenuItemModel? selectedItem)
        {
            if (selectedItem == null) return;

            switch ((MenuItem)selectedItem.Id)
            {
                case MenuItem.Emulator:
                    MainWindow.ToolBar.Value.IsExecutionToolsVisible = true;
                    MainWindow.IsMenuOpened = false;
                    MainWindow.Router.NavigateAndReset.Execute(new EmulatorViewViewModel());
                    break;
                case MenuItem.Exit:
                    applicationService.ShutdownApplication();
                    break;
                default:
                    break;
            }
        }
    }
}
