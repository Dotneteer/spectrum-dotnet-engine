using SpectrumEngine.Emu;
// ReSharper disable InconsistentNaming

namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class CpuPanelViewModel: ViewModelBase
{
    private readonly IZ80Machine _machine;

    public CpuPanelViewModel(MachineController controller)
    {
        _machine = controller.Machine;
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
        RaisePropertyChanged(nameof(HFlag));
        RaisePropertyChanged(nameof(NFlag));
        RaisePropertyChanged(nameof(CFlag));
    }
    
    public ushort AF => _machine.Regs.AF;

    public ushort BC => _machine.Regs.BC;

    public ushort DE => _machine.Regs.DE;
    
    public ushort HL => _machine.Regs.HL;

    public ushort AF_ => _machine.Regs._AF_;

    public ushort BC_ => _machine.Regs._BC_;

    public ushort DE_ => _machine.Regs._DE_;
    
    public ushort HL_ => _machine.Regs._HL_;

    public ushort PC => _machine.Regs.PC;

    public ushort SP => _machine.Regs.SP;

    public ushort IX => _machine.Regs.IX;
    
    public ushort IY => _machine.Regs.IY;

    public byte I => _machine.Regs.I;

    public byte R => _machine.Regs.R;

    public ushort WZ => _machine.Regs.WZ;

    public bool IntRequested => (_machine.SignalFlags & Z80Cpu.Z80Signals.Int) != 0;

    public bool Halted => _machine.Halted;

    public int InterruptMode => _machine.InterruptMode;

    public bool Iff1 => _machine.Iff1;
    
    public bool Iff2 => _machine.Iff2;

    public ulong Tacts => _machine.Tacts;

    public bool SFlag => _machine.Regs.IsSFlagSet;

    public bool ZFlag => _machine.Regs.IsZFlagSet;

    public bool R5Flag => _machine.Regs.IsR5FlagSet;

    public bool PFlag => _machine.Regs.IsPFlagSet;

    public bool R3Flag => _machine.Regs.IsR3FlagSet;

    public bool HFlag => _machine.Regs.IsHFlagSet;

    public bool NFlag => _machine.Regs.IsNFlagSet;
    
    public bool CFlag => _machine.Regs.IsCFlagSet;
}