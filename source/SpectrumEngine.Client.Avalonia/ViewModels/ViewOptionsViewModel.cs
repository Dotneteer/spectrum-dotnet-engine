namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class ViewOptionsViewModel : ViewModelBase
{
    private bool _showToolbar;
    private bool _showStatusBar;
    private bool _isMuted;
    private bool _showKeyboard; 

    public bool ShowToolbar
    {
        get => _showToolbar;
        set => SetProperty(ref _showToolbar, value);
    }

    public bool ShowStatusBar
    {
        get => _showStatusBar;
        set => SetProperty(ref _showStatusBar, value);
    }

    public bool ShowKeyboard
    {
        get => _showKeyboard;
        set => SetProperty(ref _showKeyboard, value);
    }

    public bool IsMuted
    {
        get => _isMuted;
        set => SetProperty(ref _isMuted, value);
    }
}