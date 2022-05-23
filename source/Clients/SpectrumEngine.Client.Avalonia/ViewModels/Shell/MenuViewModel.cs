using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.Models;
using System.Collections.Generic;
using System;
using SpectrumEngine.Client.Avalonia.Services;
using Splat;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class MenuViewModel : ViewModelBase, IMenu
    {
        public enum MenuItem
        {
            Unknown,
            Emulator,
            Exit
        }

        private readonly IApplicationService applicationService;

        public MenuViewModel(IApplicationService? applicationService = null)
        {
            this.applicationService = applicationService ?? Locator.Current.GetRequiredService<IApplicationService>();

            Title = string.Empty;
            MenuItems = CreateMenuItems();
            this.WhenAny(x => x.SelectedMenuItem, x => x.Value).Subscribe(OnSelectedMenuItem);
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
                    // TODO: implement
                    applicationService.IsBusy = !applicationService.IsBusy;
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
