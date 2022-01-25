namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the behavior of a ZX Spectrum 48K virtual machine that integrates the emulator built from
/// the standard components of a ZX Spectrum.
/// </summary>
public interface IZxSpectrum48Machine : IZ80Machine
{
    /// <summary>
    /// Represents the keyboard device of ZX Spectrum 48K
    /// </summary>
    public IKeyboardDevice KeyboardDevice { get; }

    /// <summary>
    /// Represents the screen device of ZX Spectrum 48K
    /// </summary>
    public IScreenDevice ScreenDevice { get; }

    /// <summary>
    /// Represents the beeper device of ZX Spectrum 48K
    /// </summary>
    public IBeeperDevice BeeperDevice { get; }

    /// <summary>
    /// Represents the floating port device of ZX Spectrum 48K
    /// </summary>
    public IFloatingPortDevice FloatingPortDevice { get; }

    /// <summary>
    /// Represents the tape device of ZX Spectrum 48K
    /// </summary>
    public ITapeDevice TapeDevice { get; }

}

