namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum 48 floating bus device.
/// </summary>
public sealed class ZxSpectrum48FloatingBusDevice : IFloatingBusDevice
{
    /// <summary>
    /// Initialize the floating port device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum48FloatingBusDevice(IZxSpectrum48Machine machine)
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads the current floating bus value.
    /// </summary>
    /// <returns></returns>
    public byte ReadFloatingPort()
    {
        // TODO: Implement reading from the floating bus
        return 0xff;
    }

}

