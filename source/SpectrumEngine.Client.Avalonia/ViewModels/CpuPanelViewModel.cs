using SpectrumEngine.Emu;
// ReSharper disable InconsistentNaming

namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class CpuPanelViewModel: ViewModelBase
{
    private readonly IZ80Machine _controller;

    public CpuPanelViewModel(MachineController controller)
    {
        _controller = controller.Machine;
    }

    public void SignStateChanged()
    {
        RaisePropertyChanged(nameof(AF));
        RaisePropertyChanged(nameof(BC));
        RaisePropertyChanged(nameof(DE));
        RaisePropertyChanged(nameof(HL));
        RaisePropertyChanged(nameof(AF_));
        RaisePropertyChanged(nameof(BC_));
        RaisePropertyChanged(nameof(DE_));
        RaisePropertyChanged(nameof(HL_));
        RaisePropertyChanged(nameof(PC));
        RaisePropertyChanged(nameof(SP));
        RaisePropertyChanged(nameof(IX));
        RaisePropertyChanged(nameof(IY));
        RaisePropertyChanged(nameof(I));
        RaisePropertyChanged(nameof(R));
        RaisePropertyChanged(nameof(WZ));
        RaisePropertyChanged(nameof(IntRequested));
        RaisePropertyChanged(nameof(Halted));
        RaisePropertyChanged(nameof(InterruptMode));
        RaisePropertyChanged(nameof(Iff1));
        RaisePropertyChanged(nameof(Iff2));
        RaisePropertyChanged(nameof(Tacts));
        RaisePropertyChanged(nameof(SFlag));
        RaisePropertyChanged(nameof(ZFlag));
        RaisePropertyChanged(nameof(R5Flag));
        RaisePropertyChanged(nameof(PFlag));
        RaisePropertyChanged(nameof(R3Flag));
        RaisePropertyChanged(nameof(NFlag));
        RaisePropertyChanged(nameof(CFlag));
    }
    
    public ushort AF => _controller.Regs.AF;

    public ushort BC => _controller.Regs.BC;

    public ushort DE => _controller.Regs.DE;
    
    public ushort HL => _controller.Regs.HL;

    public ushort AF_ => _controller.Regs._AF_;

    public ushort BC_ => _controller.Regs._BC_;

    public ushort DE_ => _controller.Regs._DE_;
    
    public ushort HL_ => _controller.Regs._HL_;

    public ushort PC => _controller.Regs.PC;

    public ushort SP => _controller.Regs.SP;

    public ushort IX => _controller.Regs.IX;
    
    public ushort IY => _controller.Regs.IY;

    public byte I => _controller.Regs.I;

    public byte R => _controller.Regs.R;

    public ushort WZ => _controller.Regs.WZ;

    public bool IntRequested => (_controller.SignalFlags & Z80Cpu.Z80Signals.Int) != 0;

    public bool Halted => _controller.Halted;

    public int InterruptMode => _controller.InterruptMode;

    public bool Iff1 => _controller.Iff1;
    
    public bool Iff2 => _controller.Iff2;

    public ulong Tacts => _controller.Tacts;

    public bool SFlag => _controller.Regs.IsSFlagSet;

    public bool ZFlag => _controller.Regs.IsZFlagSet;

    public bool R5Flag => _controller.Regs.IsR5FlagSet;

    public bool PFlag => _controller.Regs.IsPFlagSet;

    public bool R3Flag => _controller.Regs.IsR3FlagSet;

    public bool NFlag => _controller.Regs.IsNFlagSet;
    
    public bool CFlag => _controller.Regs.IsCFlagSet;
}