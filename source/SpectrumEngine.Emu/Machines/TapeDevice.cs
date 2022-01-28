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
    /// Get the current operation mode of the tape device.
    /// </summary>
    public TapeMode TapeMode { get; private set; }

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

    /// <summary>
    /// This method returns the value of the EAR bit read from the tape.
    /// </summary>
    public bool GetTapeEarBit()
    {
        // TODO: Implement this method
        return false;
    }

    /// <summary>
    /// Process the specified MIC bit value.
    /// </summary>
    /// <param name="micBit">MIC bit to process</param>
    public void ProcessMicBit(bool micBit)
    {
        // TODO: Implement this method
    }
}