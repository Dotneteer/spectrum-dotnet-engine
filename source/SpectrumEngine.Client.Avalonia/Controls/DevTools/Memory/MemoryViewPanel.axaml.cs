using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        PrepareRefresh();
        Vm!.MemoryViewer.MemoryItems =
            new ObservableCollection<MemoryItemViewModel>(Vm.MemoryViewer.BackgroundMemoryItems!);
        if (Vm == null) return;
        
        Vm.MemoryViewer.RangeChanged += (_, _) =>
        {
            PrepareRefresh();
            RefreshMemory();
        };
        Vm.MemoryViewer.TopAddressChanged += (_, addr) => ScrollToTopAddress(addr);
    }

    protected override void Refresh()
    {
        RefreshMemory();
    }

    private void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        e.PointerPressedEventArgs.Handled = true;
    }

    protected override void PrepareRefresh()
    {
        if (Vm == null) return;
        var machine = Vm.Machine.Controller?.Machine as ZxSpectrum48Machine;
        if (machine == null || machine.GetMachineProperty(MachinePropNames.MemoryFlat) is not byte[] memory)
        {
            return;
        }
        // --- Calculate the visible range
        var rangeFrom = Vm.MemoryViewer.RangeFrom & 0xfff0;
        var rangeTo = (Vm.MemoryViewer.RangeTo + 15) & 0xffff0;

        // --- Ensure memory items
        var memItems = new List<MemoryItemViewModel>();

        for (var addr = rangeFrom; addr < rangeTo; addr += 16)
        {
            var memItem = new MemoryItemViewModel {Address = (ushort) addr};
            memItem.RefreshFrom(memory, Vm.Cpu!);
            memItems.Add(memItem);
        }
        Vm.MemoryViewer.BackgroundMemoryItems = memItems;
    }

    private void RefreshMemory()
    {
        if (Vm == null) return;
        var machine = Vm.Machine.Controller?.Machine as ZxSpectrum48Machine;
        if (machine == null || machine.GetMachineProperty(MachinePropNames.MemoryFlat) is not byte[] memory)
        {
            return;
        }
        var position = Dg.GetViewportInfo(Vm?.MemoryViewer.MemoryItems?.Count ?? 0);
        Vm!.MemoryViewer.RefreshMemory(position.Top, position.Height + 1);
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