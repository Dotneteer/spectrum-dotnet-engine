namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum tape device.
/// </summary>
public sealed class TapeDevice: ITapeDevice
{
    /// <summary>
    /// This value is the LD_START address of the ZX Spectrum ROM. This routine checks the tape input for a pilot 
    /// pulse. When we reach this address, a LOAD operation has just been started.
    /// </summary>
    const ushort TAPE_LOAD_BYTES_ROUTINE = 0x056C;

    /// <summary>
    /// This value is the address in the ZX Spectrum ROM at which the LOAD routine tests if the tape header is
    /// correct. The address points to a RET NZ instruction that returns in case of an incorrect header. We use this
    /// address to emulate an error in fast tape mode.
    /// </summary>
    const ushort TAPE_LOAD_BYTES_INVALID_HEADER_ROUTINE = 0x05B6;

    /// <summary>
    /// This value is the address in the ZX Spectrum ROM at which the LOAD routine returns after successfully loading
    /// a tape block.
    /// </summary>
    const ushort TAPE_LOAD_BYTES_RESUME = 0x05E2;

    /// <summary>
    /// This value is the address of the SA-BYTES routine in the ZX Spectrum ROM, which indicates that a SAVE
    /// operation has just been started.
    /// </summary>
    const ushort TAPE_SAVE_BYTES_ROUTINE = 0x04C2;

    /// <summary>
    /// Represents the minimum length of a too long pause in CPU tacts
    /// </summary>
    const ulong TOO_LONG_PAUSE = 10_500_000;

    // --- Signs that we reached the end of the tape
#pragma warning disable CS0649
    private bool _tapeEof;
#pragma warning restore CS0649

    // --- The tact when detecting the last MIC bit
    private ulong _tapeLastMicBitTact;

    // --- The last MIC bit value
    private bool _tapeLastMicBit;

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
    /// This method updates the current tape mode according to the current ROM index and PC value
    /// </summary>
    public void UpdateTapeMode()
    {
        // --- Handle passive mode
        if (TapeMode == TapeMode.Passive)
        {
            if (!Machine.IsSpectrum48RomSelected) 
            {
                // --- Not in ZX Spectrum 48 ROM, nothing to do
                return;
            }
            if (Machine.Regs.PC == TAPE_LOAD_BYTES_ROUTINE)
            {
                // --- We just entered into the LOAD routine
                TapeMode = TapeMode.Load;
                NextTapeBlock();
                if (Machine.UseFastLoad)
                {
                    // --- Emulate loading the current block in fast mode.
                    FastLoad();
                    TapeMode = TapeMode.Passive;
                }
                return;
            }

            if (Machine.Regs.PC == TAPE_SAVE_BYTES_ROUTINE)
            {
                // --- Turn on SAVE mode
                TapeMode = TapeMode.Save;
                _tapeLastMicBitTact = Machine.Tacts;
                _tapeLastMicBit = true;
                //tapeSavePhase = SP_NONE;
                //tapePilotPulseCount = 0;
                //tapeDataBlockCount = 0;
                //tapePrevDataPulse = 0;
                //tapeSaveDataLen = 0;
            }
            return;
        }

        // --- Handle LOAD mode
        if (TapeMode == TapeMode.Load)
        {
            // --- Move to passive mode when tape ends or a tape error occurs
            if (_tapeEof || (Machine.IsSpectrum48RomSelected && Machine.Regs.PC == 0x0008))
            {
                TapeMode = TapeMode.Passive;
            }
            return;
        }

        // --- Handle SAVE Mode. Error or too long pause?
        if ((Machine.IsSpectrum48RomSelected && Machine.Regs.PC == 0x0008) || (Machine.Tacts - _tapeLastMicBitTact) > TOO_LONG_PAUSE)
        {
            // --- Leave the SAVE mode
            TapeMode = TapeMode.Passive;
            //saveModeLeft(tapeSaveDataLen);
        }
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

    /// <summary>
    /// Sets the data to be loaded from the tape when emulating a LOAD
    /// </summary>
    /// <param name="dataBlocks">List of data blocks to play</param>
    public void SetTapeData(IEnumerable<TapeDataBlock> dataBlocks)
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// Rewinds the tape, sets the first block as the beginning to play
    /// </summary>
    public void RewindTape()
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// Moves to the next tape block to play
    /// </summary>
    private void NextTapeBlock()
    {
        // TODO
    }

    /// <summary>
    /// Emulates loading the current block in fast mode.
    /// </summary>
    private void FastLoad()
    {

    }
}