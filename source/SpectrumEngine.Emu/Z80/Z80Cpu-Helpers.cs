namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition contains miscellaneous helpers for instruction execution.
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// Execute a relative jump with the specified distance.
    /// </summary>
    /// <param name="e">8-bit signed distance</param>
    private void RelativeJump(byte e)
    {
        TactPlus5(Regs.PC);
        Regs.PC = Regs.WZ = (ushort)(Regs.PC + (ushort)(sbyte)e);
    }

    /// <summary>
    /// Set the R5 and R3 flags of F after SCF or CCF.
    /// </summary>
    private void SetR5R3ForScfAndCcf()
    {
        if (PrevF53Updated)
        {
            Regs.F = (byte)((Regs.F & ~FlagsSetMask.R3R5) | (Regs.A & FlagsSetMask.R3R5));
        }
        else
        {
            Regs.A |= (byte)(Regs.A & FlagsSetMask.R3R5);
        }
        F53Updated = true;
    }

    /// <summary>
    /// Store two 8-bit values to the address in the code 
    /// </summary>
    /// <param name="low">LSB value to store</param>
    /// <param name="high">MSB value to store</param>
    private void Store16(byte low, byte high)
    {
        ushort tmp = ReadCodeMemory();
        tmp += (ushort)(ReadCodeMemory() << 8);
        WriteMemory(tmp, low);
        tmp += 1;
        Regs.WZ = tmp;
        WriteMemory(tmp, high);
    }
}