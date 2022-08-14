using System;
using System.Collections.ObjectModel;
using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Represents the Z80 Memory view model
/// </summary>
public class MemoryViewModel: ViewModelBase
{
    private readonly MainWindowViewModel _parent;
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
    /// The start address of the memory range
    /// </summary>
    public ushort RangeFrom
    {
        get => _rangeFrom;
        set
        {
            SetProperty(ref _rangeFrom, value);
            RaiseRangeChanged();
        }
    }

    /// <summary>
    /// The end address of the memory range
    /// </summary>
    public ushort RangeTo
    {
        get => _rangeTo;
        set
        {
            SetProperty(ref _rangeTo, value);
            RaiseRangeChanged();
        }
    }

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
    /// <param name="memory">Contents of the memory</param>
    /// <param name="top">Top memory line</param>
    /// <param name="height">Number of lines</param>
    public void RefreshMemory(byte[] memory, int top, int height)
    {
        // --- Calculate the visible range
        var rangeFrom = RangeFrom & 0xfff0;
        var rangeTo = (RangeTo + 15) & 0xffff0;

        // --- Ensure memory items
        MemoryItems ??= new ObservableCollection<MemoryItemViewModel>();

        var index = 0;
        for (var addr = rangeFrom; addr < rangeTo; addr += 16, index++)
        {
            var memItem = new MemoryItemViewModel {Address = (ushort) addr};
            memItem.RefreshFrom(memory, _parent.Cpu!);
            if (index >= MemoryItems.Count)
            {
                MemoryItems.Add(memItem);
            }
            else if (index >= top && index <= top + height)
            {
                MemoryItems[index] = memItem;
            }
            else
            {
                memItem.CopyInto(MemoryItems[index]);
            }
        }
    }

    public void Refresh() => RaiseRangeChanged();
}