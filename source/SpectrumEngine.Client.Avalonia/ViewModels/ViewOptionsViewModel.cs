namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class ViewOptionsViewModel : ViewModelBase
{
    private readonly bool _useNativeMenu;
    private bool _showMenuBar;
    private bool _showToolbar;
    private bool _showStatusBar;
    private bool _isMuted;
    private bool _showKeyboard;

    public ViewOptionsViewModel(EnvironmentViewModel env)
    {
        _useNativeMenu = env.UseNativeMenu;
    }
    
    public bool ShowMenuBar
    {
        get => _showMenuBar;
        set
        {
            SetProperty(ref _showMenuBar, value);
            RaisePropertyChanged(nameof(ShouldDisplayMenu));
        }
    }

    public bool ShouldDisplayMenu => !_useNativeMenu || ShowMenuBar;
    
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

    public void ToggleShowMenuBar() => ShowMenuBar = !ShowMenuBar;

    public void ToggleShowToolbar() => ShowToolbar = !ShowToolbar;

    public void ToggleShowStatusBar() => ShowStatusBar = !ShowStatusBar;

    public void ToggleShowKeyboard() => ShowKeyboard = !ShowKeyboard;

    public void ToggleMuted() => IsMuted = !IsMuted;


}