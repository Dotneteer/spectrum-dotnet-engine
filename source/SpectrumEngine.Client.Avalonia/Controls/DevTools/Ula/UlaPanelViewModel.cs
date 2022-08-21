using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class UlaPanelViewModel: ViewModelBase
{
    private readonly ZxSpectrumBase? _machine;

    public UlaPanelViewModel(MachineController controller)
    {
        _machine = controller.Machine as ZxSpectrumBase;
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
        RaisePropertyChanged(nameof(KeyboardLine0));
        RaisePropertyChanged(nameof(KeyboardLine1));
        RaisePropertyChanged(nameof(KeyboardLine2));
        RaisePropertyChanged(nameof(KeyboardLine3));
        RaisePropertyChanged(nameof(KeyboardLine4));
        RaisePropertyChanged(nameof(KeyboardLine5));
        RaisePropertyChanged(nameof(KeyboardLine6));
        RaisePropertyChanged(nameof(KeyboardLine7));
        RaisePropertyChanged(nameof(BorderValue));
        RaisePropertyChanged(nameof(EarBit));
        RaisePropertyChanged(nameof(MicBit));
        RaisePropertyChanged(nameof(FloatingBusValue));
    }

    public int CurrentFrameTact => _machine!.CurrentFrameTact;

    public int Frames => _machine!.Frames;

    public int RasterLine => _machine!.CurrentFrameTact / _machine.TactsInDisplayLine;
    
    public int PixelInLine => _machine!.CurrentFrameTact % _machine.TactsInDisplayLine;

    public string PixelOp => _machine!.ScreenDevice.RenderingTactTable[_machine.CurrentFrameTact].Phase.ToString();

    public int TotalContention => _machine!.TotalContentionDelaySinceStart;

    public int LastContention => _machine!.ContentionDelaySincePause;

    public byte KeyboardLine0 => _machine!.KeyboardDevice.GetKeyLineValue(0);
    
    public byte KeyboardLine1 => _machine!.KeyboardDevice.GetKeyLineValue(1);

    public byte KeyboardLine2 => _machine!.KeyboardDevice.GetKeyLineValue(2);
    
    public byte KeyboardLine3 => _machine!.KeyboardDevice.GetKeyLineValue(3);
    
    public byte KeyboardLine4 => _machine!.KeyboardDevice.GetKeyLineValue(4);
    
    public byte KeyboardLine5 => _machine!.KeyboardDevice.GetKeyLineValue(5);
    
    public byte KeyboardLine6 => _machine!.KeyboardDevice.GetKeyLineValue(6);
    
    public byte KeyboardLine7 => _machine!.KeyboardDevice.GetKeyLineValue(7);

    public byte BorderValue => (byte)_machine!.ScreenDevice.BorderColor;

    public bool EarBit => _machine!.BeeperDevice.EarBit;

    public bool MicBit => _machine!.TapeDevice.MicBit;

    public byte FloatingBusValue => _machine!.FloatingBusDevice.ReadFloatingBus();
}