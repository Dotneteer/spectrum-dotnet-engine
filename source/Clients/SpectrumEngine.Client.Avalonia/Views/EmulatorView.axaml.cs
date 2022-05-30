using Avalonia.ReactiveUI;
using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Views
{
    public partial class EmulatorView : ReactiveUserControl<EmulatorViewViewModel>
    {
        public EmulatorView()
        {
            InitializeComponent();
        }
    }
}