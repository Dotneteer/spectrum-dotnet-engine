namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class ViewsPanelViewModel: ViewModelBase
{
    private bool _showMemory;
    private bool _showDisassembly;
    private bool _showWatch;

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

    public void ToggleShowMemory() => ShowMemory = !ShowMemory;
    
    public void ToggleShowDisassembly() => ShowDisassembly = !ShowDisassembly;
    
    public void ToggleShowWatch() => ShowWatch = !ShowWatch;
}