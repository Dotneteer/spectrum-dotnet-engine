using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;
using SpectrumEngine.Tools.Disassembler;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class BreakpointsPanel : MachineStatusUserControl
{
    private object? _prevDc;
    public BreakpointsPanel()
    {
        InitializeComponent();
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_prevDc is MainWindowViewModel prevVm)
        {
            prevVm.Debugger.BreakpointsChanged -= DebuggerOnBreakpointsChanged;
        }

        if (DataContext is MainWindowViewModel newVm)
        {
            newVm.Debugger.BreakpointsChanged += DebuggerOnBreakpointsChanged;
        }

        _prevDc = DataContext;
    }

    private async void DebuggerOnBreakpointsChanged(object? sender, 
        (List<BreakpointInfo> OldBps, List<BreakpointInfo> NewBps) e)
    {
        await Refresh();
    }

    protected override async Task Refresh()
    {
        var vm = Vm;
        if (vm == null) return;
        await Task.Run(() =>
        {
            var memory = (vm.Machine.Controller?.Machine as ZxSpectrumBase)?.Get64KFlatMemory();
            if (memory == null) return;

            // --- Disassemble the contents of each breakpoint
            foreach (var bpItem in vm.Debugger.Breakpoints)
            {
                var map = new MemoryMap
                {
                    new(bpItem.Address, (ushort)(bpItem.Address + 4))
                };
                var disassembler = new Z80Disassembler(map, memory);
                var disassViewItems = disassembler.Disassemble().OutputItems;
                if (disassViewItems.Count > 0)
                {
                    bpItem.Disassembly = disassViewItems[0].Instruction;
                }
            }
            vm.Debugger.SignStateChanged();
        });
    }
}