namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition contains the definition of dependencies that you need to define to allow the Z80 to work in an
/// emulated hardware environment.
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// Read the byte at the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <returns>The byte read from the memory</returns>
    public abstract byte DoReadMemory(ushort address);

    /// <summary>
    /// This function implements the memory read delay of the CPU.
    /// </summary>
    /// <param name="address">Memory address to read</param>
    /// <remarks>
    /// Normally, it is exactly 3 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 3-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 3 T-states!
    /// </remarks>
    public virtual void DelayMemoryRead(ushort address) => TactPlus3();

    /// <summary>
    /// Write the given byte to the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    public abstract void DoWriteMemory(ushort address, byte value);

    /// <summary>
    /// This function implements the memory write delay of the CPU.
    /// </summary>
    /// <param name="address">Memory address to write</param>
    /// <remarks>
    /// Normally, it is exactly 3 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 3-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 3 T-states!
    /// </remarks>
    public virtual void DelayMemoryWrite(ushort address) => TactPlus3();

    /// <summary>
    /// This function handles address-based memory read contention.
    /// </summary>
    public abstract void DelayAddressBusAccess(ushort address);

    /// <summary>
    /// This function reads a byte (8-bit) from an I/O port using the provided 16-bit address.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port read operation.
    /// </remarks>
    public abstract byte DoReadPort(ushort address);

    /// <summary>
    /// This function implements the I/O port read delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    public virtual void DelayPortRead(ushort address) => TactPlus4();

    /// <summary>
    /// This function writes a byte (8-bit) to the 16-bit I/O port address provided in the first argument.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port write operation.
    /// </remarks>
    public abstract void DoWritePort(ushort address, byte value);

    /// <summary>
    /// This function implements the I/O port write delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    public virtual void DelayPortWrite(ushort address) => TactPlus4();

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    /// <param name="oldTacts">Number of tacts before the increment event</param>
    /// <remarks>
    /// With this function, you can emulate hardware activities running simultaneously with the CPU. For example,
    /// rendering the screen or sound,  handling peripheral devices, and so on.
    /// </remarks>
    public virtual void OnTactIncremented(ulong oldTacts)
    {
    }
}
