// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DisplayViewModel? _display;

    public MainWindowViewModel()
    {
        Environment = new EnvironmentViewModel();
        ViewOptions = new ViewOptionsViewModel();
        Machine = new MachineViewModel(this);
        ViewOptions = new ViewOptionsViewModel
        {
            ShowToolbar = true,
            ShowStatusBar = true,
            ShowKeyboard = false,
            IsMuted = false
        };
        DevTools = new DevToolsViewModel
        {
            ShowDevTools = false,
            ShowToolbar = true,
            ShowStatusBar = true,
            ShowSiteBar = true,
            ShowPanels = true
        };
        Preferences = new PreferencesViewModel();
    }
    
    /// <summary>
    /// The environment partition of the view model
    /// </summary>
    public EnvironmentViewModel Environment { get; }
    
    /// <summary>
    /// The machine state partition of the view model
    /// </summary>
    public MachineViewModel Machine { get; }

    /// <summary>
    /// The view options part of the view model
    /// </summary>
    public ViewOptionsViewModel ViewOptions { get; }

    /// <summary>
    /// The DevTools partition of the view model
    /// </summary>
    public DevToolsViewModel DevTools { get; }
    
    /// <summary>
    /// The display view model created by the machine
    /// </summary>
    public DisplayViewModel? Display
    {
        get => _display;
        set => SetProperty(ref _display, value);
    }
    
    /// <summary>
    /// Application preferences
    /// </summary>
    public PreferencesViewModel Preferences { get; }
    
}
