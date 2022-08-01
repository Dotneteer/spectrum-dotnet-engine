using System.Collections.Generic;
using System.Linq;
using SpectrumEngine.Tools.Disassembler;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

/// <summary>
/// Represents the Z80 Disassembly view model
/// </summary>
public class DisassemblyViewModel: ViewModelBase
{
    private readonly MainWindowViewModel _parent;
    private List<DisassemblyItemViewModel>? _disassItems;

    public DisassemblyViewModel(MainWindowViewModel parent)
    {
        _parent = parent;
    }
    
    public List<DisassemblyItemViewModel>? DisassItems
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
        DisassItems = disassembler.Disassemble().OutputItems
            .Select(oi => new DisassemblyItemViewModel { Item = oi, Parent = _parent}).ToList();
    }

    public void SignChanged()
    {
        RaisePropertyChanged(nameof(DisassItems));
    }
}