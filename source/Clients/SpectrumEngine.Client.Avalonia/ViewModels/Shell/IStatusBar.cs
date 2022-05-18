namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public interface IStatusBar
    {
        string? Title { get; }

        bool Visibility { get; }
    }
}
