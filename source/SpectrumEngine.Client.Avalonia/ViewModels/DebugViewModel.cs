using System.Collections.Generic;
using System.Linq;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

/// <summary>
/// Represents the viewmodel used for debugging
/// </summary>
public class DebugViewModel: ViewModelBase, IDebugSupport
{
    private ushort? _lastStartupBreakpoint;

    /// <summary>
    /// This member stores the last startup breakpoint to check. It allows setting a breakpoint to the first
    /// instruction of a program.
    /// </summary>
    public ushort? LastStartupBreakpoint
    {
        get => _lastStartupBreakpoint; 
        set => SetProperty(ref _lastStartupBreakpoint, value);
    }
    
    /// <summary>
    /// Gets the list of breakpoints
    /// </summary>
    public List<BreakpointInfo> Breakpoints { get; } = new();

    public List<BreakpointInfo> BreakpointsOrdered => Breakpoints.OrderBy(bp => bp.Address).ToList();  
    
    /// <summary>
    /// Erases all breakpoints
    /// </summary>
    public void EraseAllBreakpoints()
    {
        Breakpoints.Clear();
        SignStateChanged();
    }

    /// <summary>
    /// Adds a breakpoint to the list of existing ones
    /// </summary>
    /// <param name="address">Breakpoint address</param>
    /// <returns>True, if a new breakpoint was added; otherwise, if an existing breakpoint was updates, false</returns>
    public bool AddBreakpoint(ushort address)
    {
        var existingBp = Breakpoints.FirstOrDefault(bp => bp.Address == address);
        var newBp = true;
        if (existingBp != null)
        {
            Breakpoints.Remove(existingBp);
            newBp = false;
        }
        Breakpoints.Add(new BreakpointInfo()
        {
            Address = address
        });
        SignStateChanged();
        return newBp;
    }

    /// <summary>
    /// Removes a breakpoint
    /// </summary>
    /// <param name="address">Breakpoint address</param>
    /// <returns>True, if the breakpoint has just been removed; otherwise, false</returns>
    public bool RemoveBreakpoint(ushort address)
    {
        var existingBp = Breakpoints.FirstOrDefault(bp => bp.Address == address);
        if (existingBp == null) return false;
        Breakpoints.Remove(existingBp);
        SignStateChanged();
        return true;
    }

    public void SignStateChanged()
    {
        RaisePropertyChanged(nameof(Breakpoints));
        RaisePropertyChanged(nameof(BreakpointsOrdered));
    }
}