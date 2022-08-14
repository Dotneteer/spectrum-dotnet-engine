using System.Threading.Tasks;
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
        if (Vm == null) return;
        
        Vm.MemoryViewer.RangeChanged += (_, _) => RefreshMemory();
        Vm.MemoryViewer.TopAddressChanged += (_, addr) => ScrollToTopAddress(addr);
    }

    protected override void RefreshOnStateChanged()
    {
        RefreshMemory();
    }

    private void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        e.PointerPressedEventArgs.Handled = true;
    }

    private void RefreshMemory()
    {
        if (Vm == null) return;
        var machine = Vm.Machine.Controller?.Machine as ZxSpectrum48Machine;
        if (machine == null || machine.GetMachineProperty(MachinePropNames.MemoryFlat) is not byte[] memory)
        {
            return;
        }
        Vm.MemoryViewer.RefreshMemory(memory);
    }

    private async void ScrollToTopAddress(ushort address)
    {
        if (Vm?.MemoryViewer.MemoryItems == null) return;

        foreach (var item in Vm.MemoryViewer.MemoryItems)
        {
            if (item.Address >= address - 15 && item.Address <= address)
            {
                Dg.ScrollIntoView(item, null);
                await Task.Delay(50);
                Dg.SelectedItem = item;
            }
        }
    }   
}