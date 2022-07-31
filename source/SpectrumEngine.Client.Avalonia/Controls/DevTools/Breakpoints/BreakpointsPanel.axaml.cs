using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class BreakpointsPanel : MachineStatusUserControl
{
    public BreakpointsPanel()
    {
        InitializeComponent();
    }

    protected override void RefreshPanel()
    {
        if (DataContext is not MainWindowViewModel vm) return;
        vm.Debugger.SignStateChanged();
    }
}