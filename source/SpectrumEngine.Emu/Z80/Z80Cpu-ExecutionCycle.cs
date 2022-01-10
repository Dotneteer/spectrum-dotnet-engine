namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition defines the state information we use while emulating the behavior of the CPU.
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// Executes a hard reset as if the machine and the CPU had just been turned on.
    /// </summary>
    public void HardReset()
    {
        Regs.AF = 0xffff;
        Regs._AF_ = 0xffff;
        Regs.BC = 0x0000;
        Regs._BC_ = 0x0000;
        Regs.DE = 0x0000;
        Regs._DE_ = 0x0000;
        Regs.HL = 0x0000;
        Regs._HL_ = 0x0000;
        Regs.IX = 0x0000;
        Regs.IY = 0x0000;
        Regs.IR = 0x0000;
        Regs.PC = 0x0000;
        Regs.SP = 0xffff;
        Regs.WZ = 0x0000;

        SignalFlags = Z80Signals.None;
        InterruptMode = 0;
        Iff1 = false;
        Iff2 = false;
        Tacts = 0;
        F53Updated = false;
        PrevF53Updated = false;

        OpCode = 0;
        Prefix = OpCodePrefix.None;
        EiBacklog = 0;
        RetExecuted = false;
    }

    /// <summary>
    /// Handles the active RESET signal of the CPU
    /// </summary>
    public void Reset()
    {
        Regs.AF = 0xffff;
        Regs._AF_ = 0xffff;
        Regs.IR = 0x0000;
        Regs.PC = 0x0000;
        Regs.SP = 0xffff;
        Regs.WZ = 0x0000;

        SignalFlags = Z80Signals.None;
        InterruptMode = 0;
        Iff1 = false;
        Iff2 = false;
        Tacts = 0;
        F53Updated = false;
        PrevF53Updated = false;

        OpCode = 0;
        Prefix = OpCodePrefix.None;
        EiBacklog = 0;
        RetExecuted = false;
    }
}