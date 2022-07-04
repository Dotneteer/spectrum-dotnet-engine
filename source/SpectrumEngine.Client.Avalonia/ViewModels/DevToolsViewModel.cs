namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class DevToolsViewModel: ViewModelBase
{
    private bool _showDevTools;

    public bool ShowDevTools
    {
        get => _showDevTools;
        set => SetProperty(ref _showDevTools, value);
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
}