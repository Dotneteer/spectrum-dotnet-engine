using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using SpectrumEngine.Emu;
using SpectrumEngine.Tools.Disassembler;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

/// <summary>
/// Represents the Z80 Disassembly view model
/// </summary>
public class DisassemblyViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _parent;
    private ushort? _lastPc;

    private ObservableCollection<DisassemblyItemViewModel>? _disassItems;

    public DisassemblyViewModel(MainWindowViewModel parent)
    {
        _parent = parent;
    }

    public ObservableCollection<DisassemblyItemViewModel>? DisassItems
    {
        get => _disassItems;
        set => SetProperty(ref _disassItems, value);
    }

    public void RefreshDisassembly(byte[] opCodes)
    {
        var map = new MemoryMap
        {
            new(0x0000, 0x3fff)
        };
        var disassembler = new Z80Disassembler(map, opCodes);
        DisassItems = new ObservableCollection<DisassemblyItemViewModel>(disassembler.Disassemble().OutputItems
            .Select(oi => new DisassemblyItemViewModel {Item = oi, Parent = _parent}).ToList());
    }

    // --- Invoke this method when PC changes so the UI get refreshed
    public void ApplyNewPc(ushort pc)
    {
        RefreshItemAtAddress(_lastPc);
        RefreshItemAtAddress(pc);
        _lastPc = pc;
    }

    public void ApplyBreakpointChanges(List<BreakpointInfo> oldBps, List<BreakpointInfo> newBps)
    {
        foreach (var oldBp in oldBps)
        {
            RefreshItemAtAddress(oldBp.Address);
        }
        foreach (var newBp in newBps)
        {
            RefreshItemAtAddress(newBp.Address);
        }
    }
    
    /// <summary>
    /// Refreshes the disassembly item at the specified address
    /// </summary>
    /// <param name="address">Address to refresh</param>
    private void RefreshItemAtAddress(ushort? address)
    {
        if (DisassItems == null  || address == null) return;
        var index = GetItemIndexForAddress(address.Value);
        if (index != null)
        {
            DisassItems[index.Value] = DisassItems[index.Value].Clone();
        }
    }
    
    /// <summary>
    /// Gets the index of the disassembly item with the specified address
    /// </summary>
    /// <param name="address">Item address</param>
    /// <returns>Item, if found; otherwise, null</returns>
    private int? GetItemIndexForAddress(ushort address)
    {
        if (DisassItems == null) return null;
        for (var i = 0; i < DisassItems.Count; i++)
        {
            if (DisassItems[i].Item?.Address == address) return i;
        }
        return null;
    }
}