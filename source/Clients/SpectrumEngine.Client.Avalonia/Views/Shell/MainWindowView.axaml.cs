using Avalonia.ReactiveUI;
using ReactiveUI;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;

namespace SpectrumEngine.Client.Avalonia.Views.Shell
{
    public partial class MainWindowView : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowView()
        {
            InitializeComponent();

            this.WhenActivated(disposables => { });
        }
    }
}