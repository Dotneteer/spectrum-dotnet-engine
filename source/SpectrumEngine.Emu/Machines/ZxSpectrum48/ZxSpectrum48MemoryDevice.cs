namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the memory device of ZX Spectrum 48K
/// </summary>
public sealed class ZxSpectrum48MemoryDevice: IMemoryDevice
{
    /// <summary>
    /// Initialize the floating port device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum48MemoryDevice(IZxSpectrum48Machine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrum48Machine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// Read the byte at the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <returns>The byte read from the memory</returns>
    public byte ReadMemory(ushort address)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Write the given byte to the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    public void WriteMemory(ushort address, byte value)
    {
        throw new NotImplementedException();
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
        // TODO: Implement this method
    }

    /// <summary>
    /// This method sets the contention value associated with the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <param name="value">Contention value</param>
    public void SetContentionValue(int tact, byte value)
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// This method gets the contention value for the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <returns>The contention value associated with the specified tact.</returns>
    public byte GetContentionValue(int tact)
    {
        // TODO: Implement this method
        return 0;
    }
}
