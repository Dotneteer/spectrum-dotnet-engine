namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the memory device of ZX Spectrum 48K
/// </summary>
public sealed class ZxSpectrum48MemoryDevice : IMemoryDevice
{
    /// <summary>
    /// This byte array represents the 64K memory, including the 16K ROM and 48K RAM.
    /// </summary>
    private readonly byte[] _memory = new byte[0x1_0000];

    /// <summary>
    /// This byte array stores the contention values associated with a particular machine frame tact.
    /// </summary>
    private byte[] _contentionValues;

    /// <summary>
    /// Initialize the floating port device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum48MemoryDevice(IZxSpectrum48Machine machine)
    {
        Machine = machine;
        _contentionValues = Array.Empty<byte>();

        // --- Set up the contention methods
        machine.ContendReadFunction = machine.ContendWriteFunction =
            (ushort address) => DelayContendedMemory(address);
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrum48Machine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    /// <remarks>
    /// This method fills up the RAM with zeros; keeps the ROM unchanged.
    /// </remarks>
    public void Reset()
    {
        for (var i = 0x4000; i < _memory.Length; i++) _memory[i] = 0;
    }

    /// <summary>
    /// Read the byte at the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <returns>The byte read from the memory</returns>
    public byte ReadMemory(ushort address)
    {
        return _memory[address];
    }

    /// <summary>
    /// Write the given byte to the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    public void WriteMemory(ushort address, byte value)
    {
        if ((address & 0xc000) != 0x0000)
        {
            _memory[address] = value;
        }
    }

    /// <summary>
    /// Write the given byte to the specified memory address with no CPU delays
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    /// <remarks>
    /// This method allows writing into ROM.
    /// </remarks>
    public void DirectWrite(ushort address, byte value)
    {
        _memory[address] = value;
    }

    /// <summary>
    /// This method allocates storage for the memory contention values.
    /// </summary>
    /// <param name="tactsInFrame">Number of tacts in a machine frame</param>
    /// <remarks>
    /// Each machine frame tact that renders a display pixel may have a contention delay. If the CPU reads or writes
    /// data or uses an I/O port in that particular frame tact, the memory operation may be delayed. When the machine's
    /// screen device is initialized, it calculates the number of tacts in a frame and calls this method to allocate
    /// storage for the contention values.
    /// </remarks>
    public void AllocateContentionValues(int tactsInFrame)
    {
        _contentionValues = new byte[tactsInFrame];
    }

    /// <summary>
    /// This method sets the contention value associated with the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <param name="value">Contention value</param>
    public void SetContentionValue(int tact, byte value)
    {
        _contentionValues[tact] = value;
    }

    /// <summary>
    /// This method gets the contention value for the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <returns>The contention value associated with the specified tact.</returns>
    public byte GetContentionValue(int tact)
    {
        return _contentionValues[tact];
    }

    /// <summary>
    /// This method implements memory operation delays.
    /// </summary>
    /// <param name="address"></param>
    /// <remarks>
    /// Whenever the CPU accesses the 0x4000-0x7fff memory range, it contends with the ULA. We keep the contention
    /// delay values for a particular machine frame tact in _contentionValues.Independently of the memory address, 
    /// the Z80 CPU takes 3 T-states to read or write the memory contents.
    /// </remarks>
    public void DelayContendedMemory(ushort address)
    {
        if ((address & 0xc000) == 0x4000)
        {
            // --- We read from contended memory
            var delay = _contentionValues[Machine.CurrentFrameTact / Machine.ClockMultiplier];
            Machine.TactPlusN(delay);
        }
    }
}
