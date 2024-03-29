using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Represents the Z80 Disassembly view model
/// </summary>
public class DisassemblyViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _parent;
    private ushort? _lastPc;
    private ObservableCollection<DisassemblyItemViewModel>? _disassItems;
    private DisassemblyMode _disassemblyMode = DisassemblyMode.Normal;
    private ushort _fullRangeFrom;
    private ushort _fullRangeTo;
    private ushort _currentRangeFrom;
    private ushort _currentRangeTo;

    /// <summary>
    /// Initializes the view model with the specified parent
    /// </summary>
    /// <param name="parent">Parent view model</param>
    public DisassemblyViewModel(MainWindowViewModel parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Disassembly items to show
    /// </summary>
    public ObservableCollection<DisassemblyItemViewModel>? DisassItems
    {
        get => _disassItems;
        set => SetProperty(ref _disassItems, value);
    }

    /// <summary>
    /// Items created during background disassembly
    /// </summary>
    public List<DisassemblyItemViewModel>? BackgroundDisassemblyItems { get; set; }
    
    /// <summary>
    /// Current disassembly mode
    /// </summary>
    public DisassemblyMode DisassemblyMode
    {
        get => _disassemblyMode;
        set
        {
            SetProperty(ref _disassemblyMode, value);
            RefreshRangeFlags();
            DisassemblyModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Fired when the disassembly mode changes
    /// </summary>
    public event EventHandler? DisassemblyModeChanged;

    /// <summary>
    /// Start of the disassembly range when full range is displayed
    /// </summary>
    public ushort FullRangeFrom
    {
        get => _fullRangeFrom;
        set
        {
            SetProperty(ref _fullRangeFrom, value);
            RefreshRangeValues();
        }
    }

    /// <summary>
    /// End of the disassembly range (inclusinve) when full range is displayed
    /// </summary>
    public ushort FullRangeTo
    {
        get => _fullRangeTo;
        set
        {
            SetProperty(ref _fullRangeTo, value);
            RefreshRangeValues();
        }
    }

    /// <summary>
    /// Start of the disassembly range when in StartFromPC mode
    /// </summary>
    public ushort CurrentRangeFrom
    {
        get => _currentRangeFrom;
        set
        {
            SetProperty(ref _currentRangeFrom, value);
            RefreshRangeValues();
        }
    }

    /// <summary>
    /// End of the disassembly range (inclusive) when in StartFromPC mode
    /// </summary>
    public ushort CurrentRangeTo
    {
        get => _currentRangeTo;
        set
        {
            SetProperty(ref _currentRangeTo, value);
            RefreshRangeValues();
        }
    }

    /// <summary>
    /// The effective start address of the range
    /// </summary>
    public ushort RangeFrom => _disassemblyMode == DisassemblyMode.StartFromPc ? _currentRangeFrom : _fullRangeFrom;
    
    /// <summary>
    /// The effective end address of the range
    /// </summary>
    public ushort RangeTo => _disassemblyMode == DisassemblyMode.StartFromPc ? _currentRangeTo : _fullRangeTo;

    /// <summary>
    /// Raised when the disassembly range changes
    /// </summary>
    public event EventHandler? RangeChanged;
    
    /// <summary>
    /// Sign that disassembly range has been changed
    /// </summary>
    public void RaiseRangeChanged()
    {
        RangeChanged?.Invoke(this, EventArgs.Empty);
        RefreshRangeValues();
    }

    /// <summary>
    /// Indicates if disassembly is in Flat mode
    /// </summary>
    public bool IsFlatMode => _disassemblyMode == DisassemblyMode.Normal;

    /// <summary>
    /// Indicates if disassembly is in Follow PC mode
    /// </summary>
    public bool IsFollowPcMode => _disassemblyMode == DisassemblyMode.FollowPc;
    
    /// <summary>
    /// Indicates if disassembly is in Start from PC mode
    /// </summary>

    public bool IsStartFromPcMode => _disassemblyMode == DisassemblyMode.StartFromPc;
    
    /// <summary>
    /// Indicates if disassembly info should be displayed
    /// </summary>
    public bool CanDisplayDisassembly 
        => _disassemblyMode != DisassemblyMode.StartFromPc 
           || _parent.Machine.Controller?.State != MachineControllerState.Running;

    /// <summary>
    /// Refreshes the disassembly
    /// </summary>
    /// <param name="top">Top disassembly line</param>
    /// <param name="height">Number of lines</param>
    public void RefreshDisassembly(int top, int height)
    {
        var disassViewItems = BackgroundDisassemblyItems;
        if (disassViewItems == null) return;
        
        if (!IsStartFromPcMode)
        {
            // --- Ensure we have a valid collection for DisassItems
            DisassItems ??= new ObservableCollection<DisassemblyItemViewModel>();
            if (DisassItems.Count > disassViewItems.Count)
            {
                // --- Remove unnecessary tail items
                for (var i = 0; i < DisassItems.Count - disassViewItems.Count; i++)
                {
                    DisassItems.RemoveAt(DisassItems.Count);
                }
            }
        
            // --- Copy the items into the new collection
            for (var i = 0; i < disassViewItems.Count; i++)
            {
                if (i >= DisassItems.Count)
                {
                    DisassItems.Add(disassViewItems[i]);
                }
                else if (i >= top && i <= top + height)
                {
                    DisassItems[i] = disassViewItems[i];
                }
                else
                {
                    var item = DisassItems[i];
                    item.Item = disassViewItems[i].Item;
                    item.Parent = disassViewItems[i].Parent;
                }
            }
        }
        else
        {
            DisassItems = new ObservableCollection<DisassemblyItemViewModel>(disassViewItems);
        }
    }

    public void Refresh() => RaiseRangeChanged();

    public void SetFlatMode()
    {
        var oldMode = DisassemblyMode;
        DisassemblyMode = DisassemblyMode.Normal;
        if (oldMode is DisassemblyMode.Normal or DisassemblyMode.FollowPc) return;
        Refresh();
    }

    public void SetFollowPcMode()
    {
        var oldMode = DisassemblyMode;
        DisassemblyMode = DisassemblyMode.FollowPc;
        if (oldMode is DisassemblyMode.Normal or DisassemblyMode.FollowPc) return;
        Refresh();
    }

    public void SetStartFromPcMode()
    {
        DisassemblyMode = DisassemblyMode.StartFromPc;
        CurrentRangeFrom = _parent.Cpu!.PC;
        CurrentRangeTo = (ushort)(CurrentRangeFrom + 256);
        Refresh();
    }

    // --- Invoke this method when PC changes so the UI get refreshed
    public void ApplyNewPc(ushort pc)
    {
        RefreshItemAtAddress(_lastPc);
        RefreshItemAtAddress(pc);
        _lastPc = pc;
        RefreshRangeFlags();
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

    private void RefreshRangeFlags()
    {
        RaisePropertyChanged(nameof(IsFlatMode));
        RaisePropertyChanged(nameof(IsFollowPcMode));
        RaisePropertyChanged(nameof(IsStartFromPcMode));
        RaisePropertyChanged(nameof(CanDisplayDisassembly));
    }
    
    private void RefreshRangeValues()
    {
        RaisePropertyChanged(nameof(RangeFrom));
        RaisePropertyChanged(nameof(RangeTo));
    }
}

/// <summary>
/// Defines the current disassembly view mode
/// </summary>
public enum DisassemblyMode
{
    Normal, 
    FollowPc,
    StartFromPc
}