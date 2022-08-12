using System.Collections.Generic;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class CommandsPanelViewModel: ViewModelBase
{
    private List<List<OutputSection>>? _buffer = new();

    public List<List<OutputSection>>? Buffer
    {
        get => _buffer;
        set => SetProperty(ref _buffer, value);
    }
}