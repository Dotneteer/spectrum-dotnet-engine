namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the properties and operations of the ZX Spectrum's beeper device.
/// </summary>
public interface IBeeperDevice: IAudioDevice
{
    /// <summary>
    /// The current value of the EAR bit
    /// </summary>
    bool EarBit { get; }
    
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
