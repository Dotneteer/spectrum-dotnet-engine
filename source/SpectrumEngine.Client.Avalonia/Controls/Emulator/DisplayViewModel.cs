using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.Emulators;

public class DisplayViewModel: ViewModelBase
{
    private int _screenWidth;
    private int _screenHeight;
    private int _zoomFactor;
    private bool _isDebugging;
    private string? _overlayMessage;

    public DisplayViewModel()
    {
        ScreenWidth = 800;
        ScreenHeight = 400;
    }

    /// <summary>
    /// Indicates if the machine runs in debug mode
    /// </summary>
    public bool IsDebugging
    {
        get => _isDebugging;
        set => SetProperty(ref _isDebugging, value);
    }

    /// <summary>
    /// The machine status overlay to display
    /// </summary>
    public string? OverlayMessage
    {
        get => _overlayMessage;
        set => SetProperty(ref _overlayMessage, value);
    }
    public int ZoomFactor
    {
        get => _zoomFactor;
        set => SetProperty(ref _zoomFactor, value);
    }

    public int ScreenWidth
    {
        get => _screenWidth;
        set => SetProperty(ref _screenWidth, value);
    }

    public int ScreenHeight
    {
        get => _screenHeight;
        set => SetProperty(ref _screenHeight, value);
    }
}
