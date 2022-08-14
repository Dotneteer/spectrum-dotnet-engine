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
    /// Refreshes the memory contents dump
    /// </summary>
    /// <param name="memory">Contents of the memory</param>
    public void RefreshMemory(byte[] memory)
    {
        // --- Calculate the visible range
        var rangeFrom = RangeFrom & 0xfff8;
        var rangeTo = (RangeTo + 8) & 0xffff8;

        // --- Ensure memory items
        MemoryItems ??= new ObservableCollection<MemoryItemViewModel>();

        var index = 0;
        for (var addr = rangeFrom; addr < rangeTo; addr += 8, index++)
        {
            var memItem = new MemoryItemViewModel { Address = (ushort)addr };
            memItem.RefreshFrom(memory, _parent.Cpu!);
            if (index >= MemoryItems.Count)
            {
              MemoryItems.Add(memItem);  
            }
            else
            {
                MemoryItems[index] = memItem;
            }
        }
    }
    
    public void Refresh() => RaiseRangeChanged();
}