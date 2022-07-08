namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class DevToolsViewModel: ViewModelBase
{
    private bool _showDevTools;
    private bool _showToolbar;
    private bool _showStatusBar;
    private bool _showSiteBar;
    private bool _showPanels;
    private bool _siteBarOnLeft;
    private bool _panelsAtBottom;

    public DevToolsViewModel()
    {
        SiteBar = new SiteBarViewModel
        {
            ShowCpu = true,
            ShowUla = true,
            ShowBreakpoints = true
        };
        SiteBar.OnPanelVisibilityChanged += (_, _) =>
        {
            if (!SiteBar.ShowCpu && !SiteBar.ShowUla && !SiteBar.ShowBreakpoints)
            {
                ShowSiteBar = false;
                return;
            }
            if (!ShowSiteBar) ShowSiteBar = true;
        };
    }
    public SiteBarViewModel SiteBar { get; }
    
    public bool ShowDevTools
    {
        get => _showDevTools;
        set => SetProperty(ref _showDevTools, value);
    }

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

    public bool ShowSiteBar
    {
        get => _showSiteBar;
        set => SetProperty(ref _showSiteBar, value);
    }

    public bool ShowPanels
    {
        get => _showPanels;
        set => SetProperty(ref _showPanels, value);
    }

    public bool SiteBarOnLeft
    {
        get => _siteBarOnLeft;
        set => SetProperty(ref _siteBarOnLeft, value);
    }

    public bool PanelsAtBottom
    {
        get => _panelsAtBottom;
        set => SetProperty(ref _panelsAtBottom, value);
    }
    
    public void ToggleDevTools()
    {
        ShowDevTools = !ShowDevTools;
        if (ShowDevTools)
        {
            App.ShowDevToolsWindow();
        }
        else
        {
            App.HideDevToolsWindow();
        }
    }
    
    public void ToggleShowToolbar() => ShowToolbar = !ShowToolbar;

    public void ToggleShowStatusBar() => ShowStatusBar = !ShowStatusBar;

    public void ToggleShowSiteBar() => ShowSiteBar = !ShowSiteBar;

    public void ToggleShowPanels() => ShowPanels = !ShowPanels;
    
    public void ToggleSiteBarOnLeft() => SiteBarOnLeft = !SiteBarOnLeft;

    public void TogglePanelsAtBottom() => PanelsAtBottom = !PanelsAtBottom;
}