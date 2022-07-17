using Avalonia.Markup.Xaml;
using SpectrumEngine.Client.Avalonia.ViewModels;

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

    protected override void RefreshPanel()
    {
        if (DataContext is not MainWindowViewModel vm) return;
        vm.Cpu?.SignStateChanged();
    }
}