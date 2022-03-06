namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the properties and operations of the ZX Spectrum's beeper device.
/// </summary>
public interface IBeeperDevice: IAudioDevice
{
    /// <summary>
    /// Gets the last EAR bit value.
    /// </summary>
    bool EarBitValue { get; }

    /// <summary>
    /// This method sets the EAR bit value to generate sound with the beeper.
    /// </summary>
    /// <param name="value">EAR bit value to set</param>
    void SetEarBit(bool value);

    /// <summary>
    /// Renders the subsequent beeper sample according to the current EAR bit value
    /// </summary>
    void RenderBeeperSample();
}
