namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class ViewsPanelViewModel: ViewModelBase
{
    private bool _showMemory;
    private bool _showDisassembly;
    private bool _showWatch;
    private bool _memoryOnLeft;
    private bool _disassemblyOnLeft;
    private bool _watchOnLeft;

    public bool ShowMemory
    {
        get => _showMemory;
        set
        {
            SetProperty(ref _showMemory, value);
            RefreshPanelVisibility();
        }
    }

    public bool ShowDisassembly
    {
        get => _showDisassembly;
        set
        {
            SetProperty(ref _showDisassembly, value);
            RefreshPanelVisibility();
        }
    }

    public bool ShowWatch
    {
        get => _showWatch;
        set
        {
            SetProperty(ref _showWatch, value);
            RefreshPanelVisibility();
        }
    }

    public bool MemoryOnLeft
    {
        get => _memoryOnLeft;
        set
        {
            SetProperty(ref _memoryOnLeft, value);
            RefreshPanelVisibility();
        }
    }

    public bool DisassemblyOnLeft
    {
        get => _disassemblyOnLeft;
        set
        {
            SetProperty(ref _disassemblyOnLeft, value);
            RefreshPanelVisibility();
        }
    }

    public bool WatchOnLeft
    {
        get => _watchOnLeft;
        set
        {
            SetProperty(ref _watchOnLeft, value);
            RefreshPanelVisibility();
        }
    }

    public bool MemoryVisibleOnLeft => ShowMemory && MemoryOnLeft;
    
    public bool DisassemblyVisibleOnLeft => ShowDisassembly && DisassemblyOnLeft;
    
    public bool WatchVisibleOnLeft => ShowWatch && WatchOnLeft;

    public bool MemoryVisibleOnRight => ShowMemory && !MemoryOnLeft;
    
    public bool DisassemblyVisibleOnRight => ShowDisassembly && !DisassemblyOnLeft;
    
    public bool WatchVisibleOnRight => ShowWatch && !WatchOnLeft;

    public bool LeftVisible =>
        (ShowMemory && MemoryOnLeft)
        || (ShowDisassembly && DisassemblyOnLeft) 
        || (ShowWatch && WatchOnLeft);
    
    public bool RightVisible =>
        (ShowMemory && !MemoryOnLeft)
        || (ShowDisassembly && !DisassemblyOnLeft) 
        || (ShowWatch && !WatchOnLeft);

    public bool BothVisible => LeftVisible && RightVisible;
    
    private void RefreshPanelVisibility()
    {
        RaisePropertyChanged(nameof(LeftVisible));
        RaisePropertyChanged(nameof(RightVisible));
        RaisePropertyChanged(nameof(BothVisible));
        RaisePropertyChanged(nameof(MemoryVisibleOnLeft));
        RaisePropertyChanged(nameof(DisassemblyVisibleOnLeft));
        RaisePropertyChanged(nameof(WatchVisibleOnLeft));
        RaisePropertyChanged(nameof(MemoryVisibleOnRight));
        RaisePropertyChanged(nameof(DisassemblyVisibleOnRight));
        RaisePropertyChanged(nameof(WatchVisibleOnRight));
    }
    
    public void ToggleShowMemory() => ShowMemory = !ShowMemory;
    
    public void ToggleShowDisassembly() => ShowDisassembly = !ShowDisassembly;
    
    public void ToggleShowWatch() => ShowWatch = !ShowWatch;
    
    public void ToggleMemoryOnLeft() => MemoryOnLeft = !MemoryOnLeft;
    
    public void ToggleDismOnLeft() => DisassemblyOnLeft = !DisassemblyOnLeft;
    
    public void ToggleWatchOnLeft() => WatchOnLeft = !WatchOnLeft;
}