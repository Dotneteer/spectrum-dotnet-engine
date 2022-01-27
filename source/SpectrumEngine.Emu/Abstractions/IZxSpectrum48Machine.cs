namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the behavior of a ZX Spectrum 48K virtual machine that integrates the emulator built from
/// the standard components of a ZX Spectrum.
/// </summary>
public interface IZxSpectrum48Machine : IZ80Machine
{
    /// <summary>
    /// Gets the ULA issue number of the ZX Spectrum model (2 or 3)
    /// </summary>
    int UlaIssue { get; }

    /// <summary>
    /// Represents the memory device of ZX Spectrum 48K
    /// </summary>
    IMemoryDevice MemoryDevice { get; }

    /// <summary>
    /// Represents the I/O handler of ZX Spectrum 48K
    /// </summary>
    IIoHandler<IZxSpectrum48Machine> IoHandler { get; }

    /// <summary>
    /// Represents the keyboard device of ZX Spectrum 48K
    /// </summary>
    IKeyboardDevice KeyboardDevice { get; }

    /// <summary>
    /// Represents the screen device of ZX Spectrum 48K
    /// </summary>
    IScreenDevice ScreenDevice { get; }

    /// <summary>
    /// Represents the beeper device of ZX Spectrum 48K
    /// </summary>
    IBeeperDevice BeeperDevice { get; }

    /// <summary>
    /// Represents the floating port device of ZX Spectrum 48K
    /// </summary>
    IFloatingBusDevice FloatingBusDevice { get; }

    /// <summary>
    /// Represents the tape device of ZX Spectrum 48K
    /// </summary>
    ITapeDevice TapeDevice { get; }
}

