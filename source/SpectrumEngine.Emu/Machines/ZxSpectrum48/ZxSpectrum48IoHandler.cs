namespace SpectrumEngine.Emu;

/// <summary>
/// This class manages the ZX Spectrum devices when the CPU reads or writes I/O ports.
/// </summary>
public class ZxSpectrum48IoHandler : IIoHandler
{
    /// <summary>
    /// Read a byte from the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <returns>The byte read from the port</returns>
    public byte ReadPort(ushort address)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Write the given byte to the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <param name="value">Byte to write to the I/O port</param>
    public void WritePort(ushort address, byte value)
    {
        throw new NotImplementedException();
    }
}
