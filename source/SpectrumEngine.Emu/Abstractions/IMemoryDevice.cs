namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the properties and operations of the ZX Spectrum's memory device.
/// </summary>
public interface IMemoryDevice: IGenericDevice<IZxSpectrum48Machine>
{
    /// <summary>
    /// Read the byte at the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <returns>The byte read from the memory</returns>
    byte ReadMemory(ushort address);

    /// <summary>
    /// Write the given byte to the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    /// <remarks>
    /// This method does not write into ROM.
    /// </remarks>
    void WriteMemory(ushort address, byte value);

    /// <summary>
    /// Write the given byte to the specified memory address
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    /// <remarks>
    /// This method allows writing into ROM.
    /// </remarks>
    void DirectWrite(ushort address, byte value);

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
    void AllocateContentionValues(int tactsInFrame);

    /// <summary>
    /// This method sets the contention value associated with the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <param name="value">Contention value</param>
    void SetContentionValue(int tact, byte value);

    /// <summary>
    /// This method gets the contention value for the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <returns>The contention value associated with the specified tact.</returns>
    byte GetContentionValue(int tact);

    void DelayContendedMemory(ushort address);
}

