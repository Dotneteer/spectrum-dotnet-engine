using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;
using System.Reactive.Disposables;

namespace SpectrumEngine.Client.Avalonia.Views
{
    public partial class MainWindowView : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowView()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();

            this.WhenActivated(disposables =>
            {
                // Bind the view model router to RoutedViewHost.Router property.
                this.OneWayBind(ViewModel, x => x.Router, x => x.RoutedViewHost.Router)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.GoNext, x => x.GoNextButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.GoBack, x => x.GoBackButton)
                    .DisposeWith(disposables);
            });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Logger.Flush();
            Close();
        }
    }
}