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

    protected override void Refresh()
    {
        Vm?.Disassembler.ApplyNewPc(Vm.Cpu!.PC);
    }
}