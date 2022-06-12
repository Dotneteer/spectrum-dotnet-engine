using System.Diagnostics;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum tape device.
/// </summary>
public sealed class TapeDevice: ITapeDevice, IDisposable
{
    /// <summary>
    /// Pilot pulses in the header blcok 
    /// </summary>
    const int HEADER_PILOT_COUNT = 8063;

    /// <summary>
    /// Pilot pulses in the data block 
    /// </summary>
    const int  DATA_PILOT_COUNT = 3223;
    
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

    // --- The current tape mode
    private TapeMode _tapeMode;

    // --- The tape blocks to play
    private List<TapeDataBlock>? _blocks;
    
    // --- Signs that we reached the end of the tape
    private bool _tapeEof;

    // --- The tact when detecting the last MIC bit
    private ulong _tapeLastMicBitTact;

    // --- The last MIC bit value
    private bool _tapeLastMicBit;
    
    // --- The index of the block to read
    private int _currentBlockIndex;

    // --- The current phase of playing a tape block
    private PlayPhase _playPhase;

    // --- The CPU tact when the tape load started
    private ulong _tapeStartTact;

    // --- End tact of the current pilot
    private ulong _tapePilotEndPos;

    // --- End tact of the SYNC1 pulse
    private ulong _tapeSync1EndPos;
    
    // --- End tact of the SYNC2 pulse
    private ulong _tapeSync2EndPos;

    // --- Start tact of the current bit
    private ulong _tapeBitStartPos;
    
    // --- Length of the current bit pulse
    private ulong _tapeBitPulseLen;

    private int _dataIndex;
    
    // --- Bit mask of the current bit beign read
    private int _tapeBitMask;

    // --- Tape termination tact
    private ulong _tapeTermEndPos;
    
    // --- Tape pause position
    private ulong _tapePauseEndPos;
    
    /// <summary>
    /// Initialize the tape device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public TapeDevice(IZxSpectrum48Machine machine)
    {
        Machine = machine;
        Machine.MachinePropertyChanged += OnMachinePropertiesChanged;
        _currentBlockIndex = -1;
    }

    /// <summary>
    /// Release resources
    /// </summary>
    public void Dispose()
    {
        Machine.MachinePropertyChanged -= OnMachinePropertiesChanged;
    }
    
    /// <summary>
    /// Get the current operation mode of the tape device.
    /// </summary>
    public TapeMode TapeMode
    {
        get => _tapeMode;
        private set
        {
            if (_tapeMode == value) return;
            _tapeMode = value;
            Machine.SetMachineProperty(MachinePropNames.TapeMode, value);
        }
    }

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
        // --- Calculate the current position
        var pos = Machine.Tacts - _tapeStartTact;
        var block = _blocks![_currentBlockIndex];
        
        // --- PILOT or SYNC phase?
        if (_playPhase is PlayPhase.Pilot or PlayPhase.Sync)
        {
            // --- Generate appropriate pilot or sync EAR bit
            if (pos <= _tapePilotEndPos) {
                // --- Alternating pilot pulses
                return pos / block.PilotPulseLength % 2 == 0;
            }
            
            // --- Test SYNC1 position
            if (pos <= _tapeSync1EndPos) {
                // --- Turn to SYNC phase
                _playPhase = PlayPhase.Sync;
                return false; // => Low EAR bit
            }

            // --- Test SYNC_2 position
            if (pos <= _tapeSync2EndPos) {
                _playPhase = PlayPhase.Sync;
                return true; // => High EAR bit
            }
            
            // --- Now, we're ready to change to Data phase
            _playPhase = PlayPhase.Data;
            _tapeBitStartPos = _tapeSync2EndPos;

            // --- Select the bit pulse length of the first bit of the data byte
            _tapeBitPulseLen = (block.Data[_dataIndex] & _tapeBitMask) != 0
                ? block.OneBitPulseLength
                : block.ZeroBitPulseLength;
        }
        
        // --- Data phase?
        if (_playPhase == PlayPhase.Data) {
            // --- Generate current bit pulse
            var bitPos = pos - _tapeBitStartPos;
            
            // --- First pulse?
            if (bitPos < _tapeBitPulseLen) {
                return false; // => Low EAR bit
            }
            if (bitPos < _tapeBitPulseLen * 2) {
                return true; // => High EAR bit
            }

            // --- Move to the next bit
            _tapeBitMask >>= 1;
            if (_tapeBitMask == 0) {
                // --- Move to the next byte
                _tapeBitMask = 0x80;
                _dataIndex++;
            }

            // --- Do we have more bits to play?
            if (_dataIndex < block.Data.Length) {
                // --- Prepare to the next bit
                _tapeBitStartPos += 2 * _tapeBitPulseLen;

                // --- Select the bit pulse length of the next bit
                _tapeBitPulseLen = (block.Data[_dataIndex] & _tapeBitMask) != 0
                    ? block.OneBitPulseLength
                    : block.ZeroBitPulseLength;

                // --- We're in the first pulse of the next bit
                return false; // => Low EAR bit
            }

            // --- We've played all data bytes, let's send the terminating pulse
            _playPhase = PlayPhase.TermSync;

            // --- Prepare to the terminating sync
            _tapeTermEndPos = _tapeBitStartPos + 2 * _tapeBitPulseLen + block.EndSyncPulseLenght;
            return false;
        }

        // --- Termination sync?
        if (_playPhase == PlayPhase.TermSync) {
            if (pos < _tapeTermEndPos) {
                return false; // => Low EAR bit
            }

            // --- We terminated the data, it's pause time 
            _playPhase = PlayPhase.Pause;
            _tapePauseEndPos = _tapeTermEndPos + (ulong)(Machine.BaseClockFrequency * Machine.ClockMultiplier);
            return true; // => High EAR bit
        }

        // --- Completion? Move to the next block
        if (pos > _tapePauseEndPos) {
            NextTapeBlock();
        }

        // --- Return with a high bit
        return true;
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
    /// Rewinds the tape, sets the first block as the beginning to play
    /// </summary>
    public void RewindTape()
    {
        if (TapeMode == TapeMode.Passive)
        {
            _currentBlockIndex = -1;
            _tapeEof = false;
        }
    }

    /// <summary>
    /// Moves to the next tape block to play
    /// </summary>
    private void NextTapeBlock()
    {
        // --- No next block situations
        if (_tapeEof) return;
        if (_blocks == null)
        {
            _tapeEof = true;
            return;
        }
        if (_currentBlockIndex >= _blocks.Count - 1)
        {
            _tapeEof = true;
            return;
        }
        
        // --- Current block completed?
        if (_playPhase == PlayPhase.Completed)
        {
            return;
        }
        
        // --- Ok, we have a current block to play
        var block = _blocks[++_currentBlockIndex];
        _playPhase = PlayPhase.Pilot;
        _tapeStartTact = Machine.Tacts;
        _tapePilotEndPos = block.PilotPulseLength * ((block.Data[0] & 0x80) != 0
            ? (ulong)DATA_PILOT_COUNT
            : HEADER_PILOT_COUNT);
        _tapeSync1EndPos = _tapePilotEndPos + block.Sync1PulseLength;
        _tapeSync2EndPos = _tapeSync1EndPos + block.Sync2PulseLength;
        _dataIndex = 0;
        _tapeBitMask = 0x80;
    }

    /// <summary>
    /// Emulates loading the current block in fast mode.
    /// </summary>
    private void FastLoad()
    {

    }

    /// <summary>
    /// Respond to the tape data changes and rewind requests
    /// </summary>
    private void OnMachinePropertiesChanged(object? sender, (string key, object? value) args)
    {
        switch (args.key)
        {
            case MachinePropNames.TapeData:
                if (args.value is List<TapeDataBlock> blocks)
                {
                    _blocks = blocks;
                    _currentBlockIndex = -1;
                    _tapeEof = false;
                }       
                break;
            case MachinePropNames.RewindRequested:
                if (args.value is true)
                {
                    // TODO: Implement the rewind operation
                    Machine.SetMachineProperty(MachinePropNames.RewindRequested, null);
                }
                break;
        }
    }
}