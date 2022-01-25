namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the memory device of ZX Spectrum 48K
/// </summary>
public sealed class ZxSpectrum48MemoryDevice: IMemoryDevice
{
    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        throw new NotImplementedException();
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
}
