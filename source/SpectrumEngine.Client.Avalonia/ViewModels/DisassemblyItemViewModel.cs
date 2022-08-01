using System.Linq;
using SpectrumEngine.Tools.Disassembler;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

/// <summary>
/// This class represents the view model of a single disassembly item
/// </summary>
public class DisassemblyItemViewModel : ViewModelBase
{
    private MainWindowViewModel? _parent;
    private DisassemblyItem? _item;

    /// <summary>
    /// Ths raw disassembly item
    /// </summary>
    public DisassemblyItem? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    public MainWindowViewModel? Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
    }

    public bool HasNoBreakpoint => 
        Parent?.Cpu?.PC != Item!.Address 
        && (Parent?.Debugger.Breakpoints.All(bp => bp.Address != Item!.Address) ?? true); 

    public bool HasActiveBreakpoint => Parent?.Cpu?.PC == Item!.Address; 

    public bool HasDefinedBreakpoint => 
        Parent?.Cpu?.PC != Item!.Address 
        && (Parent?.Debugger.Breakpoints.Any(bp => bp.Address == Item!.Address) ?? false);
    
    public string OpCodes => Item!.OpCodes;
    
    public string Address => $"{Item!.Address:X4}";
    
    /// <summary>
    /// Label formatted for output
    /// </summary>
    public string Label => Item!.HasLabel ? $"L{Item.Address:X4}:" : "";

    /// <summary>
    /// Instruction formatted for output;
    /// </summary>
    public string Instruction => Item!.Instruction!;
}
