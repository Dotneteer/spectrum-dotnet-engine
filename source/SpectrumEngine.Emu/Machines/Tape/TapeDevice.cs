using System.Text;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum tape device.
/// </summary>
public sealed class TapeDevice: ITapeDevice
{
    /// <summary>
    /// Pilot pulses in the header blcok 
    /// </summary>
    private const int HEADER_PILOT_COUNT = 8063;

    /// <summary>
    /// Pilot pulses in the data block 
    /// </summary>
    private const int  DATA_PILOT_COUNT = 3223;
    
    /// <summary>
    /// Minimum number of pilot pulses before SYNC1 (while saving)
    /// </summary>
    private const int MIN_PILOT_PULSE_COUNT = 3000;

    /// <summary>
    /// This value is the LD_START address of the ZX Spectrum ROM. This routine checks the tape input for a pilot 
    /// pulse. When we reach this address, a LOAD operation has just been started.
    /// </summary>
    private const ushort TAPE_LOAD_BYTES_ROUTINE = 0x056C;

    /// <summary>
    /// This value is the address in the ZX Spectrum ROM at which the LOAD routine tests if the tape header is
    /// correct. The address points to a RET NZ instruction that returns in case of an incorrect header. We use this
    /// address to emulate an error in fast tape mode.
    /// </summary>
    private const ushort TAPE_LOAD_BYTES_INVALID_HEADER_ROUTINE = 0x05B6;

    /// <summary>
    /// This value is the address in the ZX Spectrum ROM at which the LOAD routine returns after successfully loading
    /// a tape block.
    /// </summary>
    private const ushort TAPE_LOAD_BYTES_RESUME = 0x05E2;

    /// <summary>
    /// This value is the address of the SA-BYTES routine in the ZX Spectrum ROM, which indicates that a SAVE
    /// operation has just been started.
    /// </summary>
    private const ushort TAPE_SAVE_BYTES_ROUTINE = 0x04C2;

    /// <summary>
    /// Represents the minimum length of a too long pause in CPU tacts
    /// </summary>
    private const ulong TOO_LONG_PAUSE = 3_500_000;

    /// <summary>
    /// The width tolerance of save pulses
    /// </summary>
    private const int SAVE_PULSE_TOLERANCE = 24;

    /// <summary>
    /// Lenght of the data buffer to allocate for the SAVE operation
    /// </summary>
    private const int DATA_BUFFER_LENGTH = 0x1_0000;

    // --- The current tape mode
    private TapeMode _tapeMode;

    // --- The tape blocks to play
    private List<TapeDataBlock>? _blocks;
    
    // --- Signs that we reached the end of the tape
    private bool _tapeEof;

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

    // --- Index of byte to load within the current data block
    private int _dataIndex;
    
    // --- Bit mask of the current bit beign read
    private int _tapeBitMask;

    // --- Tape termination tact
    private ulong _tapeTermEndPos;
    
    // --- Tape pause position
    private ulong _tapePauseEndPos;

    // --- Object that knows how to save the tape information
    private ITapeSaver? _tapeSaver;
    
    // --- The tact when detecting the last MIC bit
    private ulong _tapeLastMicBitTact;

    // --- The last MIC bit value

    // --- Current save phase
    private SavePhase _savePhase;

    // --- Pilot pulse counter used during a SAVE operation
    private int _pilotPulseCount;

    // --- Value of the last data pulse detected
    private MicPulseType _prevDataPulse;

    // --- Offset of the last bit being saved
    private int _bitOffset;
    
    // --- Data byte being saved
    private byte _dataByte;
    
    // --- Length of data being saved
    private int _dataLength;
    
    // --- Buffer collecting the date saved
    private byte[] _dataBuffer = Array.Empty<byte>();

    // --- Number of data blocks beign saved
    private int _dataBlockCount;
    
    /// <summary>
    /// Initialize the tape device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public TapeDevice(IZxSpectrumMachine machine)
    {
        Machine = machine;
        Machine.MachinePropertyChanged += OnMachinePropertiesChanged;
        Reset();
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
            Machine.SetMachineProperty(MachinePropNames.TAPE_MODE, value);
        }
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrumMachine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        _tapeMode = TapeMode.Passive;
        _currentBlockIndex = -1;
        _tapeEof = false;
        _playPhase = PlayPhase.None;
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
            switch (Machine.Regs.PC)
            {
                case TAPE_LOAD_BYTES_ROUTINE:
                {
                    // --- We just entered into the LOAD routine
                    TapeMode = TapeMode.Load;
                    NextTapeBlock();
                
                    // --- Do we allow fast loading mode?
                    var allowFastLoad = Machine.GetMachineProperty(MachinePropNames.FAST_LOAD);
                    if (allowFastLoad is not true) return;
                
                    // --- Emulate loading the current block in fast mode.
                    FastLoad();
                    TapeMode = TapeMode.Passive;
                    return;
                }
                
                case TAPE_SAVE_BYTES_ROUTINE:
                    // --- Turn on SAVE mode
                    TapeMode = TapeMode.Save;
                    _tapeLastMicBitTact = Machine.Tacts;
                    MicBit = true;
                    _savePhase = SavePhase.None;
                    _pilotPulseCount = 0;
                    _dataBlockCount = 0;
                    _prevDataPulse = MicPulseType.None;
                    _dataLength = 0;
                    break;
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

            // --- We terminated the data, it's pause time (1 second)
            _playPhase = PlayPhase.Pause;
            _tapePauseEndPos = _tapeTermEndPos + (ulong)Machine.BaseClockFrequency;
            return true; // => High EAR bit
        }

        // --- Completion? Move to the next block
        if (pos > _tapePauseEndPos) {
            NextTapeBlock();
        }

        // --- Return with a high bit
        return true;
    }

    public bool MicBit { get; private set; }

    /// <summary>
    /// Process the specified MIC bit value.
    /// </summary>
    /// <param name="micBit">MIC bit to process</param>
    public void ProcessMicBit(bool micBit)
    {
        if (_tapeMode != TapeMode.Save || MicBit == micBit)
        {
            return;
        }

        var length = Machine.Tacts - _tapeLastMicBitTact;

            // --- Classify the pulse by its width
            var pulse = MicPulseType.None;
            if (length is >= TapeDataBlock.BIT_0_PL - SAVE_PULSE_TOLERANCE 
                and <= TapeDataBlock.BIT_0_PL + SAVE_PULSE_TOLERANCE)
            {
                pulse = MicPulseType.Bit0;
            }
            else if (length is >= TapeDataBlock.BIT_1_PL - SAVE_PULSE_TOLERANCE 
                and <= TapeDataBlock.BIT_1_PL + SAVE_PULSE_TOLERANCE)
            {
                pulse = MicPulseType.Bit1;
            }
            if (length is >= TapeDataBlock.PILOT_PL - SAVE_PULSE_TOLERANCE
                and <= TapeDataBlock.PILOT_PL + SAVE_PULSE_TOLERANCE)
            {
                pulse = MicPulseType.Pilot;
            }
            else if (length is >= TapeDataBlock.SYNC_1_PL - SAVE_PULSE_TOLERANCE
                and <= TapeDataBlock.SYNC_1_PL + SAVE_PULSE_TOLERANCE)
            {
                pulse = MicPulseType.Sync1;
            }
            else if (length is >= TapeDataBlock.SYNC_2_PL - SAVE_PULSE_TOLERANCE
                and <= TapeDataBlock.SYNC_2_PL + SAVE_PULSE_TOLERANCE)
            {
                pulse = MicPulseType.Sync2;
            }
            else if (length is >= TapeDataBlock.TERM_SYNC - SAVE_PULSE_TOLERANCE
                and <= TapeDataBlock.TERM_SYNC + SAVE_PULSE_TOLERANCE)
            {
                pulse = MicPulseType.TermSync;
            }
            else if (length < TapeDataBlock.SYNC_1_PL - SAVE_PULSE_TOLERANCE)
            {
                pulse = MicPulseType.TooShort;
            }
            else if (length > TapeDataBlock.PILOT_PL + 2 * SAVE_PULSE_TOLERANCE)
            {
                pulse = MicPulseType.TooLong;
            }

            MicBit = micBit;
            _tapeLastMicBitTact = Machine.Tacts;

            // --- Lets process the pulse according to the current SAVE phase and pulse width
            var nextPhase = SavePhase.Error;
            switch (_savePhase)
            {
                case SavePhase.None:
                    if (pulse == MicPulseType.TooShort || pulse == MicPulseType.TooLong)
                    {
                        nextPhase = SavePhase.None;
                    }
                    else if (pulse == MicPulseType.Pilot)
                    {
                        _pilotPulseCount = 1;
                        nextPhase = SavePhase.Pilot;
                    }
                    break;
                case SavePhase.Pilot:
                    if (pulse == MicPulseType.Pilot)
                    {
                        _pilotPulseCount++;
                        nextPhase = SavePhase.Pilot;
                    }
                    else if (pulse == MicPulseType.Sync1 && _pilotPulseCount >= MIN_PILOT_PULSE_COUNT)
                    {
                        nextPhase = SavePhase.Sync1;
                    }
                    break;
                case SavePhase.Sync1:
                    if (pulse == MicPulseType.Sync2)
                    {
                        nextPhase = SavePhase.Sync2;
                    }
                    break;
                case SavePhase.Sync2:
                    if (pulse == MicPulseType.Bit0 || pulse == MicPulseType.Bit1)
                    {
                        // --- Next pulse starts data, prepare for receiving it
                        _prevDataPulse = pulse;
                        nextPhase = SavePhase.Data;
                        _bitOffset = 0;
                        _dataByte = 0;
                        _dataLength = 0;
                        _dataBuffer = new byte[DATA_BUFFER_LENGTH];
                    }
                    break;
                case SavePhase.Data:
                    if (pulse == MicPulseType.Bit0 || pulse == MicPulseType.Bit1)
                    {
                        if (_prevDataPulse == MicPulseType.None)
                        {
                            // --- We are waiting for the second half of the bit pulse
                            _prevDataPulse = pulse;
                            nextPhase = SavePhase.Data;
                        }
                        else if (_prevDataPulse == pulse)
                        {
                            // --- We received a full valid bit pulse
                            nextPhase = SavePhase.Data;
                            _prevDataPulse = MicPulseType.None;

                            // --- Add this bit to the received data
                            _bitOffset++;
                            _dataByte = (byte)(_dataByte * 2 + (pulse == MicPulseType.Bit0 ? 0 : 1));
                            if (_bitOffset == 8)
                            {
                                // --- We received a full byte
                                _dataBuffer[_dataLength++] = _dataByte;
                                _dataByte = 0;
                                _bitOffset = 0;
                            }
                        }
                    }
                    else if (pulse == MicPulseType.TermSync)
                    {
                        // --- We received the terminating pulse, the datablock has been completed
                        nextPhase = SavePhase.None;
                        _dataBlockCount++;

                        // --- Create and save the data block
                        var dataBlock = new TzxStandardSpeedBlock
                        {
                            Data = _dataBuffer,
                            DataLength = (ushort) _dataLength
                        };

                        // --- If this is the first data block, extract the name from the header
                        if (_dataBlockCount == 1 && _dataLength == 0x13)
                        {
                            // --- It's a header!
                            var sb = new StringBuilder(16);
                            for (var i = 2; i <= 11; i++)
                            {
                                sb.Append((char) _dataBuffer[i]);
                            }
                            var name = sb.ToString().TrimEnd();
                            _tapeSaver?.SetName(name);
                        }
                        _tapeSaver?.SaveTapeBlock(dataBlock);
                    }
                    break;
            }
            _savePhase = nextPhase;    }

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
        // --- Stop playing if no more blocks
        if (_tapeEof)
        {
            return;
        }

        var block = _blocks![_currentBlockIndex];
        var dataIndex = 0;
        var regs = Machine.Regs;

        // -- Move AF' to AF
        regs.AF = regs._AF_;

        // -- Check if it is a VERIFY
        var isVerify = (regs.AF & 0xff01) == 0xff00;

        // --- At this point IX contains the address to load the data, 
        // --- DE shows the #of bytes to load. A contains 0x00 for header, 
        // --- 0xFF for data block
        if (block.Data[dataIndex] != regs.A)
        {
            // --- This block has a different type we're expecting
            regs.A ^= regs.L;
            // --- Reset Z and C
            regs.F &= 0xbe;
            regs.PC = TAPE_LOAD_BYTES_INVALID_HEADER_ROUTINE;
            NextTapeBlock();
            return;
        }

        // --- It is time to load the block
        regs.H = regs.A;

        // --- Skip the header byte
        dataIndex++;
        while (regs.DE > 0)
        {
            regs.L = block.Data[dataIndex];
            if (isVerify)
            {
                // -- VERIFY operation
                if (Machine.DoReadMemory(regs.IX) != regs.L)
                {
                    // --- We read a different byte, it's an error
                    // --- Reset Z and C
                    regs.F &= 0xbe;
                    regs.PC = TAPE_LOAD_BYTES_INVALID_HEADER_ROUTINE;
                    return;
                }
            }

            // --- Store the loaded byte
            Machine.DoWriteMemory(regs.IX, regs.L);

            // --- Calculate the checksum
            regs.H ^= regs.L;

            // --- Increment the data pointers
            dataIndex++;
            regs.IX++;

            // --- Decrement byte count
            regs.DE--;
        }

        // --- Check the end of the data stream
        if (dataIndex > block.Data.Length - 1)
        {
            // --- Read over the expected length
            // --- Reset Carry to sign error
            regs.F &= 0xfe;
        }
        else
        {
            // --- Verify checksum
            if (block.Data[dataIndex] != regs.H)
            {
                // --- Wrong checksum
                // --- Reset Carry to sign error
                regs.F &= 0xfe;
            }
            else
            {
                // --- Block read successfully, set Carry
                regs.F |= Z80Cpu.FlagsSetMask.C;
            }
        }

        regs.PC = TAPE_LOAD_BYTES_RESUME;

        // --- Sign completion of this block
        _playPhase = PlayPhase.Pause;

        // --- Imitate, we're over the pause period
        _tapePauseEndPos = 0;
    }

    /// <summary>
    /// Respond to the tape data changes and rewind requests
    /// </summary>
    private void OnMachinePropertiesChanged(object? sender, (string key, object? value) args)
    {
        switch (args.key)
        {
            case MachinePropNames.TAPE_SAVER:
                if (args.value is ITapeSaver tapeSaver)
                {
                    _tapeSaver = tapeSaver;
                }
                break;
            
            case MachinePropNames.TAPE_DATA:
                if (args.value is List<TapeDataBlock> blocks)
                {
                    _blocks = blocks;
                    _currentBlockIndex = -1;
                    _tapeEof = false;
                }       
                break;
            
            case MachinePropNames.REWIND_REQUESTED:
                if (args.value is true)
                {
                    _currentBlockIndex = -1;
                    _tapeEof = false;
                    _playPhase = PlayPhase.None;
                    Machine.SetMachineProperty(MachinePropNames.REWIND_REQUESTED, null);
                }
                break;
        }
    }
}