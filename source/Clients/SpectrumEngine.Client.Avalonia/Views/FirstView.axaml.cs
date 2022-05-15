using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SpectrumEngine.Client.Avalonia.ViewModels;
using System.Reactive.Disposables;

namespace SpectrumEngine.Client.Avalonia.Views
{
    public partial class FirstView : ReactiveUserControl<FirstViewModel>
    {
        public FirstView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, x => x.UrlPathSegment, x => x.PathTextBlock.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}