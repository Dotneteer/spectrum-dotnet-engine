namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum beeper device.
/// </summary>
public sealed class BeeperDevice : AudioDeviceBase, IBeeperDevice
{
    /// <summary>
    /// Initialize the beeper device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public BeeperDevice(IZxSpectrumMachine machine): base(machine)
    {
    }

    /// <summary>
    /// The current value of the EAR bit
    /// </summary>
    public bool EarBit { get; private set; }

    /// <summary>
    /// This method sets the EAR bit value to generate sound with the beeper.
    /// </summary>
    /// <param name="value">EAR bit value to set</param>
    public void SetEarBit(bool value)
    {
        EarBit = value;
    }

    /// <summary>
    /// Gets the current sound sample (according to the current CPU tact)
    /// </summary>
    /// <returns>Sound sample value</returns>
    protected override float GetCurrentSampleValue()
    {
        return EarBit ? 1.0f : 0.0f;
    }
}