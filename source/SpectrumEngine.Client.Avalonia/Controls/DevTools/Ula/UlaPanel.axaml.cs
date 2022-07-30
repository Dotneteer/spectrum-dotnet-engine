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

    protected override void RefreshPanel()
    {
        if (DataContext is not MainWindowViewModel vm) return;
        vm.Ula?.SignStateChanged();
    }
}