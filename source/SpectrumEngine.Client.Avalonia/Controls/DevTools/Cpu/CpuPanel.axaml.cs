using System.Threading.Tasks;
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

    protected override Task Refresh()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Cpu?.SignStateChanged();
        }
        return Task.FromResult(0);
    }
}