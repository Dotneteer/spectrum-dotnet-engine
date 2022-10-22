namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum +3E floating bus device.
/// </summary>
public class ZxSpectrum3eFloatingBusDevice: IFloatingBusDevice
{
    /// <summary>
    /// Initialize the floating port device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum3eFloatingBusDevice(IZxSpectrumMachine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Release resources
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrumMachine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        // --- Intentionally empty
    }

    /// <summary>
    /// Reads the current floating bus value.
    /// </summary>
    /// <returns></returns>
    public byte ReadFloatingBus() => 0xff;
}