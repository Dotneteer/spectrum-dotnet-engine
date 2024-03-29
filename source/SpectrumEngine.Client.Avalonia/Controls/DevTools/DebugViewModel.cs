using System;
using System.Collections.Generic;
using System.Linq;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

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

    /// <summary>
    /// The last breakpoint we stopped in the frame
    /// </summary>
    public ushort? LastBreakpoint { get; set; }

    /// <summary>
    /// Breakpoint used for step-out debugging mode
    /// </summary>
    public ushort? ImminentBreakpoint { get; set; }

    public List<BreakpointInfo> BreakpointsOrdered => Breakpoints.OrderBy(bp => bp.Address).ToList();

    /// <summary>
    /// Raise this event when the collection of breakpoints has changed.
    /// </summary>
    public event EventHandler<(List<BreakpointInfo> OldBps, List<BreakpointInfo> NewBps)>? BreakpointsChanged;
    
    /// <summary>
    /// Erases all breakpoints
    /// </summary>
    public void EraseAllBreakpoints()
    {
        var oldBps = new List<BreakpointInfo>(Breakpoints);
        Breakpoints.Clear();
        SignStateChanged(oldBps);
    }

    /// <summary>
    /// Adds a breakpoint to the list of existing ones
    /// </summary>
    /// <param name="address">Breakpoint address</param>
    /// <returns>True, if a new breakpoint was added; otherwise, if an existing breakpoint was updates, false</returns>
    public bool AddBreakpoint(ushort address)
    {
        var oldBps = new List<BreakpointInfo>(Breakpoints);
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
        SignStateChanged(oldBps);
        return newBp;
    }

    /// <summary>
    /// Removes a breakpoint
    /// </summary>
    /// <param name="address">Breakpoint address</param>
    /// <returns>True, if the breakpoint has just been removed; otherwise, false</returns>
    public bool RemoveBreakpoint(ushort address)
    {
        var oldBps = new List<BreakpointInfo>(Breakpoints);
        var existingBp = Breakpoints.FirstOrDefault(bp => bp.Address == address);
        if (existingBp == null) return false;
        Breakpoints.Remove(existingBp);
        SignStateChanged(oldBps);
        return true;
    }

    public void SignStateChanged(List<BreakpointInfo>? oldBps = null)
    {
        RaisePropertyChanged(nameof(Breakpoints));
        RaisePropertyChanged(nameof(BreakpointsOrdered));
        if (oldBps != null)
        {
            BreakpointsChanged?.Invoke(this, (oldBps, Breakpoints));
        }
    }
}