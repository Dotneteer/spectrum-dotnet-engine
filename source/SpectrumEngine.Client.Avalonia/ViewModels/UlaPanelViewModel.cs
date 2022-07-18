using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class UlaPanelViewModel: ViewModelBase
{
    private readonly ZxSpectrum48Machine? _machine;

    public UlaPanelViewModel(MachineController controller)
    {
        _machine = controller.Machine as ZxSpectrum48Machine;
    }

    public void SignStateChanged()
    {
        RaisePropertyChanged(nameof(CurrentFrameTact));
        RaisePropertyChanged(nameof(Frames));
        RaisePropertyChanged(nameof(RasterLine));
        RaisePropertyChanged(nameof(PixelInLine));
        RaisePropertyChanged(nameof(PixelOp));
        RaisePropertyChanged(nameof(TotalContention));
        RaisePropertyChanged(nameof(LastContention));
    }
    
    public int CurrentFrameTact => _machine!.CurrentFrameTact;

    public int Frames => _machine!.Frames;

    public int RasterLine => _machine!.CurrentFrameTact / _machine.TactsInDisplayLine;
    
    public int PixelInLine => _machine!.CurrentFrameTact % _machine.TactsInDisplayLine;

    public string PixelOp => _machine!.ScreenDevice.RenderingTactTable[_machine.CurrentFrameTact].Phase.ToString();

    public int TotalContention => _machine!.TotalContentionDelaySinceStart;

    public int LastContention => _machine!.ContentionDelaySincePause;
}