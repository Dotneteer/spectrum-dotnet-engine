using ReactiveUI.Fody.Helpers;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public class ToolBarViewModel : ViewModelBase, IToolBar
    {
        public ToolBarViewModel()
        {
            SetDefaultValues();

            // REactiveUI
            //        var canGoBack = this
            //.WhenAnyValue(x => x.Router.NavigationStack.Count)
            //.Select(count => count > 0);
            //        GoBack = ReactiveCommand.CreateFromObservable(() => Router.NavigateBack.Execute(Unit.Default), canGoBack);
        }

        //public ReactiveCommand<Unit, IRoutableViewModel?> GoBack { get; }


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

        private void SetDefaultValues()
        {
            this.Title = string.Empty;
            this.BackButtonVisibility = false;
            this.Visibility = true;
        }
    }
}
