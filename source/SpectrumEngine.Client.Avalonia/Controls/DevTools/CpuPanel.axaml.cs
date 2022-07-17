using Avalonia.Markup.Xaml;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class CpuPanel : MachineStatusUserControl
{
    public CpuPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void NewControllerSet(MachineController controller)
    {
        DataContext = new CpuPanelViewModel(controller);
    }

    protected override void RefreshPanel()
    {
        if (DataContext is CpuPanelViewModel cpuPanelVm)
        {
            cpuPanelVm.SignStateChanged();
        }
    }
}