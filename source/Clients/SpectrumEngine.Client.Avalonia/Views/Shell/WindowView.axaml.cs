using Avalonia.ReactiveUI;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;

namespace SpectrumEngine.Client.Avalonia.Views.Shell
{
    public partial class WindowView : ReactiveWindow<WindowViewModel>
    {
        public WindowView()
        {
            InitializeComponent();
        }
	}
}