namespace SpectrumEngine.Emu;

/// <summary>
/// This class manages the ZX Spectrum devices when the CPU reads or writes I/O ports.
/// </summary>
public class ZxSpectrum48IoHandler : IIoHandler<IZxSpectrum48Machine>
{
    // --- Last value of bit 3 on port $FE
    private bool _portBit3LastValue;

    // --- Last value of bit 4 on port $FE
    private bool _portBit4LastValue;

    // --- Tacts value when last time bit 4 of $fe changed from 0 to 1
    private int _portBit4ChangedFrom0Tacts;

    // --- Tacts value when last time bit 4 of $fe changed from 1 to 0
    private int _portBit4ChangedFrom1Tacts;

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
    /// Reset the I/O to its initial state.
    /// </summary>
    public void Reset()
    {
    }

    /// <summary>
    /// Read a byte from the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <returns>The byte read from the port</returns>
    public byte ReadPort(ushort address)
    {
        return (address & 0x0001) == 0
            ? ReadPort0xFE(address)
            : Machine.FloatingBusDevice.ReadFloatingPort();
    }

    /// <summary>
    /// Write the given byte to the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <param name="value">Byte to write to the I/O port</param>
    public void WritePort(ushort address, byte value)
    {
        if ((address & 0x0001) == 0)
        {
            WritePort0xFE(address, value);
        }
    }

    /// <summary>
    /// Reads a byte from the ZX Spectrum generic input port.
    /// </summary>
    /// <param name="address">Port address</param>
    /// <returns>Byte value read from the generic port</returns>
    private byte ReadPort0xFE(ushort address)
    {
        return 0xff;
    }

    private void WritePort0xFE(ushort address, byte value)
    {
        // TODO: Implement this method
    }
}
