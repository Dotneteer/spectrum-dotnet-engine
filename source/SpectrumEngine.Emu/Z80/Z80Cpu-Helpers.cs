using System.Runtime.CompilerServices;

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
    /// Reads the specified memory address.
    /// </summary>
    /// <param name="address">Memory address to read</param>
    /// <returns>The byte the CPU has read from the memory</returns>
    /// <remarks>
    /// If the emulated hardware uses any delay when reading the memory, increment the CPU tacts accordingly.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadMemory(ushort address)
    {
        TactPlus3();
        return ReadMemoryFunction(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadCodeMemory()
    {
        TactPlus3();
        return ReadMemoryFunction(Regs.PC++);
    }

    /// <summary>
    /// Writes a byte to the specfied memory address.
    /// </summary>
    /// <param name="address">Memory address</param>
    /// <param name="data">Data byte to write</param>
    /// <remarks>
    /// If the emulated hardware uses any delay when writing the memory, increment the CPU tacts accordingly.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteMemory(ushort address, byte data)
    {
        TactPlus3();
        WriteMemoryFunction(address, data);
    }

    /// <summary>
    /// Reads the specified I/O port.
    /// </summary>
    /// <param name="address">I/O port address to read</param>
    /// <returns>The byte the CPU has read from the I/O port</returns>
    /// <remarks>
    /// The I/O port handler must increase the CPU tacts to emulate the behavior of the concrete hardware. If the
    /// peripheral device does not delay the execution, invoke the TactP4 method to apply the default 4 T-state
    /// I/O delays.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadPort(ushort address)
    {
        TactPlus3();
        return ReadMemoryFunction(address);
    }

    /// <summary>
    /// Writes a byte to the specified I/O port.
    /// </summary>
    /// <param name="address">I/O port address</param>
    /// <param name="data">Data byte to write</param>
    /// <remarks>
    /// The I/O port handler must increase the CPU tacts to emulate the behavior of the concrete hardware. If the
    /// peripheral device does not delay the execution, invoke the TactP4 method to apply the default 4 T-state
    /// I/O delays.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WritePort(ushort address, byte data)
    {
        WriteMemoryFunction(address, data);
    }

    /// <summary>
    /// Push the current value of PC to the stack.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PushPC()
    {
        Regs.SP--;
        TactPlus1();
        WriteMemory(Regs.SP, (byte)(Regs.PC >> 8));
        Regs.SP--;
        WriteMemory(Regs.SP, (byte)(Regs.PC & 0xff));
    }

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

    /// <summary>
    /// The core of the CALL instruction 
    /// </summary>
    private void CallCore()
    {
        TactPlus1(Regs.IR);
        Regs.SP--;
        WriteMemory(Regs.SP, (byte)(Regs.PC >> 8));
        Regs.SP--;
        WriteMemory(Regs.SP, (byte)Regs.PC);
        Regs.PC = Regs.WZ;
    }

    // 
    /// <summary>
    /// The core of the RST instruction 
    /// </summary>
    /// <param name="addr">Restart address to call</param>
    private void RstCore(ushort addr)
    {
        TactPlus1(Regs.IR);
        Regs.SP--;
        WriteMemory(Regs.SP, (byte)(Regs.PC >> 8));
        Regs.SP--;
        WriteMemory(Regs.SP, (byte)Regs.PC);
        Regs.PC = Regs.WZ;
        Regs.PC = Regs.WZ = addr;
    }
}