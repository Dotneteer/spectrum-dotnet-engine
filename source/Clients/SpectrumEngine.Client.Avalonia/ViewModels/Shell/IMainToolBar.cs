namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public interface IMainToolBar
    {
        string? Title { get; }

        bool BackButtonVisibility { get; }

        bool ToolBarVisibility { get; }
    }
}
