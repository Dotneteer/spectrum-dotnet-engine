using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Represents the panel with CPU information
/// </summary>
public partial class CpuPanel : MachineStatusUserControl
{
    public CpuPanel()
    {
        InitializeComponent();
    }

    protected override void Refresh()
    {
        if (DataContext is not MainWindowViewModel vm) return;
        vm.Cpu?.SignStateChanged();
    }
}