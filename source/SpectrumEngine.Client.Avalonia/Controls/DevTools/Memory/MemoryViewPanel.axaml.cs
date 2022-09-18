using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;
// ReSharper disable UnusedParameter.Local

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
        Vm.MemoryViewer.ModeChanged += async (_, _) =>
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

    protected override async Task PrepareRefresh()
    {
        var vm = Vm;
        if (vm == null) return;
        await Task.Run(() =>
        {
            // --- Obtain the flat memory contents
            byte[]? memory = null;
            var rangeFrom = 0;
            var rangeTo = 0;
            switch (vm.MemoryViewer.DisplayMode)
            {
                case MemoryDisplayMode.Full:
                    memory = (vm.Machine.Controller?.Machine as ZxSpectrumBase)?.Get64KFlatMemory();
                    rangeFrom = vm.MemoryViewer.RangeFrom & 0xfff0;
                    rangeTo = (vm.MemoryViewer.RangeTo + 15) & 0xffff0;
                    break;
                case MemoryDisplayMode.RomPage:
                    memory = (vm.Machine.Controller?.Machine as ZxSpectrumBase)?
                        .Get16KPartition(-vm.MemoryViewer.RomPage-1);
                    rangeFrom = 0x0000;
                    rangeTo = memory?.Length ?? 0;
                    break;
                case MemoryDisplayMode.RamBank:
                    memory = (vm.Machine.Controller?.Machine as ZxSpectrumBase)?
                        .Get16KPartition(vm.MemoryViewer.RamBank);
                    rangeFrom = 0x0000;
                    rangeTo = memory?.Length ?? 0;
                    break;
            }
            if (memory == null) return;
            
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
                vm.MemoryViewer.MemoryItems =
                    new ObservableCollection<MemoryItemViewModel>(vm.MemoryViewer.BackgroundMemoryItems!);
            });
        });
    }

    private void ScrollToTopAddress(ushort address)
    {
        if (Vm?.MemoryViewer.MemoryItems == null) return;

        foreach (var item in Vm.MemoryViewer.MemoryItems)
        {
            if (item.Address >= address - 15 && item.Address <= address)
            {
                // Dg.ScrollIntoView(item, null);
                // await Task.Delay(50);
                // Dg.SelectedItem = item;
            }
        }
    }   
}