namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum 48 floating port device.
/// </summary>
public sealed class ZxSpectrum48FloatingPortDevice : IFloatingPortDevice
{
    /// <summary>
    /// Initialize the floating port device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum48FloatingPortDevice(IZxSpectrum48Machine machine)
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
}

