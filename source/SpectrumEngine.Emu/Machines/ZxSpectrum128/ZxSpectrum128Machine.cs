namespace SpectrumEngine.Emu.ZxSpectrum128;

/// <summary>
/// This class represents the emulator of a ZX Spectrum 128 machine.
/// </summary>
public class ZxSpectrum128Machine: ZxSpectrum48Machine
{
    /// <summary>
    /// The unique identifier of the machine type
    /// </summary>
    public override string MachineId => "sp128";

    /// <summary>
    /// The name of the machine type to display
    /// </summary>
    public override string DisplayName => "ZX Spectrum 128K";
}