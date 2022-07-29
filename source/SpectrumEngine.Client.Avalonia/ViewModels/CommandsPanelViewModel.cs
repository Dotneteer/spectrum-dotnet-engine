using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class CommandsPanelViewModel: ViewModelBase
{
    private OutputBuffer? _buffer;

    public OutputBuffer? Buffer
    {
        get => _buffer;
        set => SetProperty(ref _buffer, value);
    }
}