namespace SpectrumEngine.Emu;

/// <summary>
/// Represents the AY-3-8910 PSG chip as a device
/// </summary>
public interface IPsgDevice: IAudioDevice
{
    /// <summary>
    /// Sets the PSG register index to read or write
    /// </summary>
    /// <param name="index">PSG register index</param>
    void SetPsgRegisterIndex(int index);

    /// <summary>
    /// Reads the value of the PSG register addressed with the las SetPsgRegisterIndex operation
    /// </summary>
    /// <returns>The value of the PSG register</returns>
    byte ReadPsgRegister();

    /// <summary>
    /// Writes the value of the PSG register addressed with the las SetPsgRegisterIndex operation
    /// </summary>
    /// <param name="value">Value to set for the PSG register</param>
    void WritePsgRegisterValue(byte value);
}