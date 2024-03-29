﻿namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the properties and operations of the ZX Spectrum's tape device.
/// </summary>
public interface ITapeDevice: IGenericDevice<IZxSpectrumMachine>
{
    /// <summary>
    /// Get the current operation mode of the tape device.
    /// </summary>
    TapeMode TapeMode { get; }

    /// <summary>
    /// This mehtod updates the current tape mode according to the current ROM index and PC value
    /// </summary>
    void UpdateTapeMode();

    /// <summary>
    /// This method returns the value of the EAR bit read from the tape.
    /// </summary>
    bool GetTapeEarBit();

    /// <summary>
    /// The current value of the MIC bit
    /// </summary>
    bool MicBit { get; }
    
    /// <summary>
    /// Process the specified MIC bit value.
    /// </summary>
    /// <param name="micBit">MIC bit to process</param>
    void ProcessMicBit(bool micBit);
}

/// <summary>
/// This enum indicates the current mode of the tape device.
/// </summary>
public enum TapeMode
{
    /// <summary>
    /// The tape device is passive.
    /// </summary>
    Passive,

    /// <summary>
    /// The tape device is in LOAD mode, affecting the read operation of the EAR bit.
    /// </summary>
    Load,

    /// <summary>
    /// The tape device is in SAVE mode, affecting the write operation of the MIC bit.
    /// </summary>
    Save
}

