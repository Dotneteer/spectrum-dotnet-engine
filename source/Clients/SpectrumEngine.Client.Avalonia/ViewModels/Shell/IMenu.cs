namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell
{
    public interface IMenu
    {
        string? Title { get; }

        bool Visibility { get; }
    }
}
