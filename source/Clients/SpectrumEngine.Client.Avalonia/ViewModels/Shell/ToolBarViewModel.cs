using ReactiveUI;
using System;
using ReactiveUI.Fody.Helpers;
using System.Reactive;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class ToolBarViewModel : ViewModelBase, IToolBar
    {
        public ToolBarViewModel()
        {
            this.Title = string.Empty;
            this.BackButtonVisibility = false;
            this.Visibility = true;

            //var canGoBack = this.WhenAnyValue(x => x.Router.NavigationStack.Count).Select(count => count > 0);
            OpenMenuCmd = ReactiveCommand.Create(() => this.MainWindow.IsMenuOpened = true);

            //this.WhenAnyValue(x => x.MainWindow.IsMenuOpened).Subscribe(value => MainWindow.IsMenuOpened = value);
        }

        public ReactiveCommand<Unit, bool> OpenMenuCmd { get; private set; }


        //public void GoBackCmd()
        //{
        //    this.NavigationService.GoBack();
        //}
        //public bool CanGoBackCmd => !this.IsBusy;

        [Reactive]
        public string? Title { get; set; }

        [Reactive]
        public bool Visibility { get; set; }

        [Reactive]
        public bool BackButtonVisibility { get; set; }
    }
}
