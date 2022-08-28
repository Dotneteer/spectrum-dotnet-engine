using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
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

    protected override async void OnInitialized()
    {
        await PrepareRefresh();
        Vm!.MemoryViewer.MemoryItems =
            new ObservableCollection<MemoryItemViewModel>(Vm.MemoryViewer.BackgroundMemoryItems!);
        if (Vm == null) return;
        
        Vm.MemoryViewer.RangeChanged += async (_, _) =>
        {
            await PrepareRefresh();
            await RefreshMemory();
        };
        Vm.MemoryViewer.TopAddressChanged += (_, addr) => ScrollToTopAddress(addr);
    }

    protected override async Task Refresh()
    {
        await PrepareRefresh();
        await RefreshMemory();
    }

    private void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        e.PointerPressedEventArgs.Handled = true;
    }

    protected override async Task PrepareRefresh()
    {
        var vm = Vm;
        if (vm == null) return;
        await Task.Run(() =>
        {
            // --- Obtain the flat memory contents
            var memory = (vm.Machine.Controller?.Machine as ZxSpectrumBase)?.Get64KFlatMemory();
            if (memory == null) return;
            
            // --- Calculate the visible range
            var rangeFrom = vm.MemoryViewer.RangeFrom & 0xfff0;
            var rangeTo = (vm.MemoryViewer.RangeTo + 15) & 0xffff0;

            // --- Ensure memory items
            var memItems = new List<MemoryItemViewModel>();

            for (var addr = rangeFrom; addr < rangeTo; addr += 16)
            {
                var memItem = new MemoryItemViewModel {Address = (ushort) addr};
                memItem.RefreshFrom(memory, vm.Cpu!);
                memItems.Add(memItem);
            }
            vm.MemoryViewer.BackgroundMemoryItems = memItems;
        });
    }

    private async Task RefreshMemory()
    {
        var vm = Vm;
        if (vm == null) return;
        await Task.Run(() =>
        {
            var memory = (vm.Machine.Controller?.Machine as ZxSpectrumBase)?.Get64KFlatMemory();
            if (memory == null) return;

            Dispatcher.UIThread.Post(() =>
            {
                var position = Dg.GetViewportInfo(vm?.MemoryViewer.MemoryItems?.Count ?? 0);
                vm!.MemoryViewer.RefreshMemory(position.Top, position.Height + 1);
            });
        });
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