namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum beeper device.
/// </summary>
public sealed class BeeperDevice : IBeeperDevice
{
    /// <summary>
    /// Gets the last EAR bit value.
    /// </summary>
    public bool EarBitValue { get; private set; }

    /// <summary>
    /// This method sets the EAR bit value to generate sound with the beeper.
    /// </summary>
    /// <param name="value">EAR bit value to set</param>
    public void SetEarBit(bool value)
    {
        EarBitValue = value;
        // TODO: Generate the beeper sound sample
    }

    /// <summary>
    /// Initialize the beeper device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public BeeperDevice(IZxSpectrum48Machine machine)
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
}