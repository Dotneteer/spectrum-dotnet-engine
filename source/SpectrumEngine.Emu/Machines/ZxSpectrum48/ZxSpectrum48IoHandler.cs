namespace SpectrumEngine.Emu;

/// <summary>
/// This class manages the ZX Spectrum devices when the CPU reads or writes I/O ports.
/// </summary>
public class ZxSpectrum48IoHandler : IIoHandler<IZxSpectrum48Machine>
{
    /// <summary>
    /// Initialize the I/O handler and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum48IoHandler(IZxSpectrum48Machine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Get the machine that hosts the I/O handler
    /// </summary>
    public IZxSpectrum48Machine Machine { get; }

    /// <summary>
    /// Read a byte from the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <returns>The byte read from the port</returns>
    public byte ReadPort(ushort address)
    {
        throw new NotImplementedException();
    }

    public void Reset()
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
