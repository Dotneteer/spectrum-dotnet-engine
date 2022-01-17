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
    /// This function reads a byte (8-bit) from the memory using the provided 16-bit address.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the memory
    /// read operation.
    /// </remarks>
    public Func<ushort, byte> ReadMemoryFunction;

    /// <summary>
    /// This function writes a byte (8-bit) to the 16-bit memory address provided in the first argument.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the memory
    /// write operation.
    /// </remarks>
    public Action<ushort, byte> WriteMemoryFunction;

    /// <summary>
    /// This function reads a byte (8-bit) from an I/O port using the provided 16-bit address.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port read operation.
    /// </remarks>
    public Func<ushort, byte> ReadPortFunction;

    /// <summary>
    /// This function writes a byte (8-bit) to the 16-bit I/O port address provided in the first argument.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port write operation.
    /// </remarks>
    public Action<ushort, byte> WritePortFunction;

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    /// <remarks>
    /// With this function, you can emulate hardware activities running simultaneously with the CPU. For example,
    /// rendering the screen or sound,  handling peripheral devices, and so on.
    /// </remarks>
    public Action TactIncrementedHandler;

    /// <summary>
    /// This function handles address-based memory read contention.
    /// </summary>
    public Action<ushort> ContendReadFunction;

    /// <summary>
    /// This function handles address-based memory write contention.
    /// </summary>
    public Action<ushort> ContendWriteFunction;
}
