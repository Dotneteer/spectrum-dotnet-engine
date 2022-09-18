using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Represents the Z80 Memory view model
/// </summary>
public class MemoryViewModel: ViewModelBase
{
    private readonly MainWindowViewModel _parent;
    private MemoryDisplayMode _displayMode;
    private int _romPage;
    private int _ramBank;
    private ushort _rangeFrom;
    private ushort _rangeTo;
    
    private ObservableCollection<MemoryItemViewModel>? _memoryItems;

    /// <summary>
    /// Initializes the view model with the specified parent
    /// </summary>
    /// <param name="parent">Parent view model</param>
    public MemoryViewModel(MainWindowViewModel parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Memory items to show
    /// </summary>
    public ObservableCollection<MemoryItemViewModel>? MemoryItems
    {
        get => _memoryItems;
        set => SetProperty(ref _memoryItems, value);
    }

    /// <summary>
    /// Items created during background refresh
    /// </summary>
    public List<MemoryItemViewModel>? BackgroundMemoryItems { get; set; }

    /// <summary>
    /// The current display mode
    /// </summary>
    public MemoryDisplayMode DisplayMode
    {
        get => _displayMode;
        set => SetProperty(ref _displayMode, value);
    }

    /// <summary>
    /// Indicates that the current mode is ROM page
    /// </summary>
    public bool InRomPageMode => _displayMode == MemoryDisplayMode.RomPage;

    /// <summary>
    /// Indicates that the current mode is RAM bank
    /// </summary>
    public bool InRamBankMode => _displayMode == MemoryDisplayMode.RamBank;

    /// <summary>
    /// Indicates that the current mode is full view
    /// </summary>
    public bool InFullMode => _displayMode == MemoryDisplayMode.Full;
    
    /// <summary>
    /// The current ROM page to display
    /// </summary>
    public int RomPage
    {
        get => _romPage;
        set => SetProperty(ref _romPage, value);
    }

    /// <summary>
    /// The current RAM bank to display
    /// </summary>
    public int RamBank
    {
        get => _ramBank;
        set => SetProperty(ref _ramBank, value);
    }
    
    /// <summary>
    /// The start address of the memory range
    /// </summary>
    public ushort RangeFrom
    {
        get => _rangeFrom;
        set => SetProperty(ref _rangeFrom, value);
    }

    /// <summary>
    /// The end address of the memory range
    /// </summary>
    public ushort RangeTo
    {
        get => _rangeTo;
        set => SetProperty(ref _rangeTo, value);
    }

    public event EventHandler? ModeChanged;
    
    /// <summary>
    /// Raised when the memory range changes
    /// </summary>
    public event EventHandler? RangeChanged;

    /// <summary>
    /// Sign that disassembly range has been changed
    /// </summary>
    public void RaiseRangeChanged()
    {
        RangeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sign that the current display mode has changed
    /// </summary>
    public void RaiseModeChanged()
    {
        RaisePropertyChanged(nameof(InRomPageMode));
        RaisePropertyChanged(nameof(InRamBankMode));
        RaisePropertyChanged(nameof(InFullMode));
        ModeChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Raised when the address to show at the top has changed
    /// </summary>
    public event EventHandler<ushort>? TopAddressChanged;

    /// <summary>
    /// Signs that the address to show at the top has changed
    /// </summary>
    public void RaiseTopAddressChanged(ushort topAddress)
    {
        TopAddressChanged?.Invoke(this, topAddress);
    }

    /// <summary>
    /// Refreshes the memory contents dump
    /// </summary>
    /// <param name="top">Top memory line</param>
    /// <param name="height">Number of lines</param>
    public void RefreshMemory(int top, int height)
    {
        if (BackgroundMemoryItems == null || MemoryItems == null) return;
        for (var i = top; i <= top + height; i++)
        {
            if (!MemoryItems[i].Equals(BackgroundMemoryItems[i]))
            {
                MemoryItems[i] = BackgroundMemoryItems[i];
            }
        }
    }

    public void Refresh()
    {
        RaiseRangeChanged();
    }
}

public enum MemoryDisplayMode
{
    /// <summary>
    /// Display the full 64K memory (according to the range set)
    /// </summary>
    Full,
    
    /// <summary>
    /// Display only the specified ROM page
    /// </summary>
    RomPage,
    
    /// <summary>
    /// Display only the specified RAM bank
    /// </summary>
    RamBank
}