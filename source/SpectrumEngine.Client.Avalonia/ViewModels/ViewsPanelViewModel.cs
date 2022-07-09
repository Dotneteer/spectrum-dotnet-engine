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
        set => SetProperty(ref _showMemory, value);
    }
    
    public bool ShowDisassembly
    {
        get => _showDisassembly;
        set => SetProperty(ref _showDisassembly, value);
    }
    
    public bool ShowWatch
    {
        get => _showWatch;
        set => SetProperty(ref _showWatch, value);
    }
    
    public bool MemoryOnLeft
    {
        get => _memoryOnLeft;
        set => SetProperty(ref _memoryOnLeft, value);
    }
    
    public bool DisassemblyOnLeft
    {
        get => _disassemblyOnLeft;
        set => SetProperty(ref _disassemblyOnLeft, value);
    }
    
    public bool WatchOnLeft
    {
        get => _watchOnLeft;
        set => SetProperty(ref _watchOnLeft, value);
    }

    public void ToggleShowMemory() => ShowMemory = !ShowMemory;
    
    public void ToggleShowDisassembly() => ShowDisassembly = !ShowDisassembly;
    
    public void ToggleShowWatch() => ShowWatch = !ShowWatch;
    
    public void ToggleMemoryOnLeft() => MemoryOnLeft = !MemoryOnLeft;
    
    public void ToggleDisAssemblyOnLeft() => DisassemblyOnLeft = !DisassemblyOnLeft;
    
    public void ToggleWatchOnLeft() => WatchOnLeft = !WatchOnLeft;
}