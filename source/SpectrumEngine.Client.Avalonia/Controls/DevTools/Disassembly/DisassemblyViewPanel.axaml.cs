using System;
using Avalonia.Controls;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class DisassemblyViewPanel : MachineStatusUserControl
{
    public DisassemblyViewPanel()
    {
        InitializeComponent();
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

    private void OnInitialized(object? sender, EventArgs e)
    {
        RefreshDisassembly();
        if (Vm != null)
        {
            Vm.Disassembler.RangeChanged += (_, _) =>
            {
                RefreshDisassembly();
            };
        }
    }

    private void RefreshDisassembly()
    {
        if (Vm == null) return;
        var machine = Vm.Machine.Controller?.Machine as ZxSpectrum48Machine;
        if (machine == null || machine.GetMachineProperty(MachinePropNames.MemoryFlat) is not byte[] memory)
        {
            return;
        }
        Vm.Disassembler.RefreshDisassembly(memory);
    }

    private void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        e.PointerPressedEventArgs.Handled = true;
    }

    protected override void RefreshOnStateChanged()
    {
        if (Vm == null) return;
        if (Vm.Machine.Controller!.State != MachineControllerState.Paused) return;
        
        Vm.Disassembler.CurrentRangeFrom = Vm.Cpu!.PC;
        Vm.Disassembler.CurrentRangeTo = (ushort)(Vm.Disassembler.CurrentRangeFrom + 256);
        RefreshDisassembly();
        Dg.SelectedIndex = 0;
    }

    protected override void Refresh()
    {
        if (Vm == null) return;
        
        // --- Make sure to display the current execution point indicator
        Vm.Disassembler.ApplyNewPc(Vm.Cpu!.PC);

        if (!Vm.Disassembler.IsFollowPcMode) return;
        
        // --- Find the first item with the PC address
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
    
    
}