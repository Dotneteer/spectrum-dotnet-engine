using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Represents the view model of a single activity
/// </summary>
public class ActivityViewModel: ViewModelBase
{
    private string _id;
    private string _icon;
    private string _description;
    private bool _isSystem;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    
    public string Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }
    
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
    
    public bool IsSystem
    {
        get => _isSystem;
        set => SetProperty(ref _isSystem, value);
    }
}