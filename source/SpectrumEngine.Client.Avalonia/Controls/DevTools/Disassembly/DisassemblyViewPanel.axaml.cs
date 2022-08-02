using System;
using Avalonia.Controls;
using Avalonia.Input;
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
    }

    private void OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        RefreshDisassembly();
    }

    private void RefreshDisassembly()
    {
        var machine = Vm?.Machine.Controller?.Machine as ZxSpectrum48Machine;
        if (machine?.GetMachineProperty(MachinePropNames.MemoryFlat) is not byte[] memory) return;
        Vm?.Disassembler.RefreshDisassembly(memory.AsSpan(0x0000, 0x4000).ToArray());
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