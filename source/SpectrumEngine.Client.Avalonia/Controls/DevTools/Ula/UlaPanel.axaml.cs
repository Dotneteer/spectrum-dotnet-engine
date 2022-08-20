using System.Threading.Tasks;
using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// This control displays the status of the ULA
/// </summary>
public partial class UlaPanel : MachineStatusUserControl
{
    public UlaPanel()
    {
        InitializeComponent();
    }

    protected override Task Refresh()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Ula?.SignStateChanged();
        }
        return Task.FromResult(0);
    }
}