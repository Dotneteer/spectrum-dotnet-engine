namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the responsibilities of reading from and writing to the Z80 I/O ports.
/// </summary>
/// <remarks>
/// An I/O handler should recognize if the emulated machine manages a particular input and output port and behave
/// accordingly.
/// </remarks>
public interface IIoHandler<TMachine>: IGenericDevice<TMachine> where TMachine : IZ80Machine
{
    /// <summary>
    /// Read a byte from the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <returns>The byte read from the port</returns>
    byte ReadPort(ushort address);

    /// <summary>
    /// Write the given byte to the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <param name="value">Byte to write to the I/O port</param>
    void WritePort(ushort address, byte value);
}
