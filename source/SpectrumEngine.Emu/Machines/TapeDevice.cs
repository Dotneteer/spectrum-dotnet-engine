namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum tape device.
/// </summary>
public sealed class TapeDevice: ITapeDevice
{
    /// <summary>
    /// Initialize the tape device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public TapeDevice(IZxSpectrum48Machine machine)
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