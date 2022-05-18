namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public interface IToolBar
    {
        string? Title { get; }

        bool Visibility { get; }

        bool BackButtonVisibility { get; }
    }
}
