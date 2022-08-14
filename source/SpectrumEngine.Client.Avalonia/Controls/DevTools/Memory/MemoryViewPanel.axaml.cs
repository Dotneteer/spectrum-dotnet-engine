using Avalonia.Controls;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class MemoryViewPanel : MachineStatusUserControl
{
    public MemoryViewPanel()
    {
        InitializeComponent();
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

    protected override void OnInitialized()
    {
        RefreshMemory();
        if (Vm != null)
        {
            Vm.MemoryViewer.RangeChanged += (_, _) =>
            {
                RefreshMemory();
            };
        }
    }

    private void RefreshMemory()
    {
        if (Vm == null) return;
        var machine = Vm.Machine.Controller?.Machine as ZxSpectrum48Machine;
        if (machine == null || machine.GetMachineProperty(MachinePropNames.MemoryFlat) is not byte[] memory)
        {
            return;
        }
        Dg.BeginBatchUpdate();
        Vm.MemoryViewer.RefreshMemory(memory);
        Dg.EndBatchUpdate();
    }

    private void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        e.PointerPressedEventArgs.Handled = true;
    }

    protected override void Refresh()
    {
        RefreshMemory();        
    }
}