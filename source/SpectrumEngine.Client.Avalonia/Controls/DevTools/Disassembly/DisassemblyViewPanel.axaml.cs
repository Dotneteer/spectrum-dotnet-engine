using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
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

    protected override void OnInitialized()
    {
        PrepareRefresh();
        RefreshDisassembly();
        if (Vm == null) return;
        
        Vm.Disassembler.RangeChanged += (_, _) =>
        {
            PrepareRefresh();
            RefreshDisassembly();
        };
        Vm.Disassembler.DisassemblyModeChanged += (_, _) =>
        {
            Refresh();
        };
    }

    private void RefreshDisassembly()
    {
        if (Vm == null) return;
        var position = Dg.GetViewportInfo(Vm?.Disassembler.DisassItems?.Count ?? 0);
        Vm!.Disassembler.RefreshDisassembly(position.Top, position.Height + 1);
    }

    private void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        e.PointerPressedEventArgs.Handled = true;
    }

    protected override async void RefreshOnStateChanged()
    {
        if (Vm == null) return;
        if (Vm.Machine.Controller!.State != MachineControllerState.Paused) return;
        if (Vm.Disassembler.DisassemblyMode == DisassemblyMode.StartFromPc)
        {
            await RefreshFromPc();
        }
    }

    protected override void PrepareRefresh()
    {
        if (Vm == null) return;
        var machine = Vm.Machine.Controller?.Machine as ZxSpectrum48Machine;
        if (machine == null || machine.GetMachineProperty(MachinePropNames.MemoryFlat) is not byte[] memory)
        {
            return;
        }
        
        var map = new MemoryMap
        {
            new(Vm!.Disassembler.RangeFrom, Vm.Disassembler.RangeTo)
        };
        var disassembler = new Z80Disassembler(map, memory);
        Vm!.Disassembler.BackgroundDisassemblyItems = disassembler.Disassemble().OutputItems
            .Select(oi => new DisassemblyItemViewModel {Item = oi, Parent = Vm}).ToList();
    }

    protected override async void Refresh()
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
        PrepareRefresh();
        RefreshDisassembly();
        await Task.Delay(50);
        Dg.SelectedIndex = 0;
    }
}