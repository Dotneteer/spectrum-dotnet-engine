namespace SpectrumEngine.Client.Avalonia.ViewModels;

/// <summary>
/// The view model that stores application preferences
/// </summary>
public class PreferencesViewModel: ViewModelBase
{
    private string _saveFolder;

    /// <summary>
    /// The folder to persist files resulting from SAVE operations
    /// </summary>
    public string SaveFolder
    {
        get => _saveFolder;
        set => SetProperty(ref _saveFolder, value);
    }
}