﻿namespace SpectrumEngine.Emu;

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
    /// This method allocates storage for the memory contention values.
    /// </summary>
    /// <param name="tactsInFrame">Number of tacts in a machine frame</param>
    /// <remarks>
    /// Each machine frame tact that renders a display pixel may have a contention delay. If the CPU reads or writes
    /// data or uses an I/O port in that particular frame tact, the memory operation may be delayed. When the machine's
    /// screen device is initialized, it calculates the number of tacts in a frame and calls this method to allocate
    /// storage for the contention values.
    /// </remarks>
    void AllocateContentionValues(int tactsInFrame);

    /// <summary>
    /// This method sets the contention value associated with the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <param name="value">Contention value</param>
    void SetContentionValue(int tact, byte value);

    /// <summary>
    /// This method gets the contention value for the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <returns>The contention value associated with the specified tact.</returns>
    byte GetContentionValue(int tact);

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

    /// <summary>
    /// Indicates if the currently selected ROM is the ZX Spectrum 48 ROM
    /// </summary>
    bool IsSpectrum48RomSelected { get; }

    /// <summary>
    /// Indicates that the machine can use the FAST LOAD mode
    /// </summary>
    bool UseFastLoad { get; }
}

