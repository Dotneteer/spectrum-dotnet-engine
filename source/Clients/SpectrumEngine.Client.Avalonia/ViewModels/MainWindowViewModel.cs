using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Shared.PlatformSupport;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        private readonly Application app;

        public MainWindowViewModel()
        {
            app = Application.Current ?? throw new InvalidProgramException("Application.Current is null on MainWindowViewModel");

            app.SetCurrentLanguage(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            Icon = new WindowIcon(new AssetLoader().Open(new Uri(app.FirstResource<string>("MainWindowIcon"))));
            Title = app.FirstResource<string>("MainWindowTitle");

            // suscribirse a los eventos de ocupacion de tareas
            // this.Events.Subscribe(this);
            // establecer el contenido inicial
            // this.NavigationService.NavigateToViewModel<ITestContent>();




            Router = new RoutingState();

            // Manage the routing state. Use the Router.Navigate.Execute
            // command to navigate to different view models. 
            //
            // Note, that the Navigate.Execute method accepts an instance 
            // of a view model, this allows you to pass parameters to 
            // your view models, or to reuse existing view models.
            //
            GoNext = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new FirstViewModel()));

            // You can also ask the router to go back. One option is to 
            // execute the default Router.NavigateBack command. Another
            // option is to define your own command with custom
            // canExecute condition as such:
            var canGoBack = this
                .WhenAnyValue(x => x.Router.NavigationStack.Count)
                .Select(count => count > 0);
            GoBack = ReactiveCommand.CreateFromObservable(() => Router.NavigateBack.Execute(Unit.Default), canGoBack);

        }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public WindowIcon Icon { get; set; }


        public RoutingState Router { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }

        public ReactiveCommand<Unit, IRoutableViewModel?> GoBack { get; }










        // public Lazy<IMainToolBar> MainToolBar { get; private set; }


        ///// <summary>
        ///// Gestionar eventos indicando que existen tareas en ejecucion
        ///// </summary>
        ///// <param name="evt">Evento</param>
        //public void Handle(BusyEvent evt)
        //{
        //    this.IsBusy = evt.IsBusy;
        //}


    }
}
