using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;
using SpectrumEngine.Tools.Disassembler;

// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class DisassemblyViewPanel : MachineStatusUserControl
{
    public DisassemblyViewPanel()
    {
        InitializeComponent();
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

    protected override async void OnInitialized()
    {
        await PrepareRefresh();
        await RefreshDisassembly();
        if (Vm == null) return;
        
        Vm.Disassembler.RangeChanged += async (_, _) =>
        {
            await PrepareRefresh();
            await RefreshDisassembly();
        };
        Vm.Disassembler.DisassemblyModeChanged += async (_, _) =>
        {
            await Refresh();
        };
    }

    private Task RefreshDisassembly()
    {
        var vm = Vm;
        if (vm != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var position = Dg.GetViewportInfo(Vm?.Disassembler.DisassItems?.Count ?? 0);
                vm.Disassembler.RefreshDisassembly(position.Top, position.Height + 1);
            });
        }
        return Task.FromResult(0);
    }

    private void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        e.PointerPressedEventArgs.Handled = true;
    }

    protected override async Task RefreshOnStateChanged()
    {
        if (Vm == null) return;
        if (Vm.Machine.Controller!.State != MachineControllerState.Paused) return;
        if (Vm.Disassembler.DisassemblyMode == DisassemblyMode.StartFromPc)
        {
            await RefreshFromPc();
        }
    }

    protected override async Task PrepareRefresh()
    {
        var vm = Vm;
        if (vm == null) return;
        await Task.Run(() =>
        {
            var machine = vm.Machine.Controller?.Machine as ZxSpectrumBase;
            if (machine?.GetMachineProperty(MachinePropNames.MemoryFlat) is not byte[] memory)
            {
                return;
            }
        
            var map = new MemoryMap
            {
                new(vm.Disassembler.RangeFrom, vm.Disassembler.RangeTo)
            };
            var disassembler = new Z80Disassembler(map, memory);
            vm.Disassembler.BackgroundDisassemblyItems = disassembler.Disassemble().OutputItems
                .Select(oi => new DisassemblyItemViewModel {Item = oi, Parent = vm}).ToList();
        });
    }

    protected override async Task Refresh()
    {
        if (Vm?.Disassembler.DisassItems == null) return;
        
        // --- Make sure to display the current execution point indicator
        Vm.Disassembler.ApplyNewPc(Vm.Cpu!.PC);

        // --- Flat mode: Keep the disassembly list as it is
        if (Vm.Disassembler.IsFlatMode)
        {
            return;
        }
        
        // --- Start from PC mode: Disassemble and position to the top
        if (Vm.Disassembler.IsStartFromPcMode)
        {
            await RefreshFromPc();

            // --- Always scroll to the top item
            Dg.ScrollIntoView(Vm.Disassembler.DisassItems![0], null);
            return;
        }

        // --- Follow PC mode: Find the first item with the PC address
        DisassemblyItemViewModel? pcItem = null;
        for (var i = 0; i < Vm.Disassembler.DisassItems!.Count; i++)
        {
            var item = Vm.Disassembler.DisassItems[i]; 
            if (item.Item!.Address < Vm.Cpu.PC) continue;
            pcItem = item;
            break;
        }
        Dg.ScrollIntoView(pcItem, null);
    }

    private async Task RefreshFromPc()
    {
        if (Vm == null) return;
        Vm.Disassembler.CurrentRangeFrom = Vm.Cpu!.PC;
        Vm.Disassembler.CurrentRangeTo = (ushort)(Vm.Disassembler.CurrentRangeFrom + 256);
        await PrepareRefresh();
        await RefreshDisassembly();
        await Task.Delay(50);
        Dg.SelectedIndex = 0;
    }
}