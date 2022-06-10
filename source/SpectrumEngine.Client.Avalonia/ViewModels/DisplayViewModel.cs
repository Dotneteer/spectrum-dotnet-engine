using Avalonia.Media;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class DisplayViewModel: ViewModelBase
{
    private int _screenWidth;
    private int _screenHeight;
    private int _zoomFactor;
    private Brush? _background;

    // ReSharper disable once UnusedMember.Global
    public DisplayViewModel()
    {
    }

    public DisplayViewModel(MachineController controller)
    {
        Controller = controller;
        Machine = controller.Machine;
        ScreenWidth = 800;
        ScreenHeight = 400;
        Background = new SolidColorBrush(Color.FromArgb(0xff, 0xdb, 0xc1, 0x0e));
    }

    public MachineController? Controller { get; }

    public IZ80Machine? Machine { get; }

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

    public Brush? Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }
}
