// ReSharper disable VirtualMemberCallInConstructor

namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents the emulator of a ZX Spectrum 128 machine.
/// </summary>
public class ZxSpectrum128Machine: ZxSpectrumBase
{
    #region Private members

    private readonly byte[][] _romPages;
    private readonly byte[][] _ramBanks;
    private int _selectedRom;
    private int _selectedBank;
    private bool _pagingEnabled;
    private bool _useShadowScreen;

    #endregion

    #region Initialization and Properties

    /// <summary>
    /// The unique identifier of the machine type
    /// </summary>
    public override string MachineId => "sp128";

    /// <summary>
    /// The name of the machine type to display
    /// </summary>
    public override string DisplayName => "ZX Spectrum 128K";

    /// <summary>
    /// Represents the PSG device of ZX Spectrum 128
    /// </summary>
    public IPsgDevice PsgDevice { get; }
    
    /// <summary>
    /// Initialize the machine
    /// </summary>
    public ZxSpectrum128Machine()
    {
        // --- Set up machine attributes
        BaseClockFrequency = 3_546_900;
        ClockMultiplier = 1;
        DelayedAddressBus = true;
        
        // --- Initialize the memory contents
        _romPages = new byte[2][];
        _romPages[0] = new byte[0x4000];
        _romPages[1] = new byte[0x4000];
        _ramBanks = new byte[8][];
        for (var i = 0; i < 8; i++)
        {
            _ramBanks[i] = new byte[0x4000];
        }

        // --- Create and initialize devices
        KeyboardDevice = new KeyboardDevice(this);
        ScreenDevice = new CommonScreenDevice(this, CommonScreenDevice.ZxSpectrum128ScreenConfiguration);
        BeeperDevice = new BeeperDevice(this);
        PsgDevice = new ZxSpectrum128PsgDevice(this);
        FloatingBusDevice = new ZxSpectrum128FloatingBusDevice(this);
        TapeDevice = new TapeDevice(this);
        Reset();

        // --- Initialize the machine's ROM (Roms/ZxSpectrum48/ZxSpectrum48.rom)
        UploadRomBytes(0, LoadRomFromResource(MachineId, 0));
        UploadRomBytes(1, LoadRomFromResource(MachineId, 1));
    }

    /// <summary>
    /// Gets the ULA issue number of the ZX Spectrum model (2 or 3)
    /// </summary>
    public override int UlaIssue { get; set; } = 3;

    /// <summary>
    /// Emulates turning on a machine (after it has been turned off).
    /// </summary>
    public override void HardReset()
    {
        base.HardReset();
        for (var bank = 0; bank < 8; bank++)
        {
            for (var i = 0; i < 0x4000; i++)
            {
                _ramBanks[bank][i] = 0;
            }
        }        
        Reset();
    }

    /// <summary>
    /// This method emulates resetting a machine with a hardware reset button.
    /// </summary>
    public override void Reset()
    {
        // --- Reset the CPU
        base.Reset();

        // --- Reset the ROM page and the RAM bank
        _selectedRom = 0;
        _selectedBank = 0;
        
        // --- Enable memory paging
        _pagingEnabled = true;
        
        // --- Shadow screen is disabled
        _useShadowScreen = false;

        // --- Reset and setup devices
        KeyboardDevice.Reset();
        ScreenDevice.Reset();
        BeeperDevice.Reset();
        BeeperDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
        PsgDevice.Reset();
        PsgDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
        FloatingBusDevice.Reset();
        TapeDevice.Reset();
        
        // --- Set default property values
        SetMachineProperty(MachinePropNames.TAPE_MODE, TapeMode.Passive);
        SetMachineProperty(MachinePropNames.REWIND_REQUESTED, null);
        SetMachineProperty(MachinePropNames.KBTYPE_48, false);

        // --- Unknown clock multiplier in the previous frame
        OldClockMultiplier = -1;

        // --- Prepare for running a new machine loop
        ClockMultiplier = TargetClockMultiplier;
        ExecutionContext.LastTerminationReason = null;
        LastRenderedFrameTact = -0;

        // --- Empty the queue of emulated keystrokes
        lock (EmulatedKeyStrokes) { EmulatedKeyStrokes.Clear(); }
    }

    /// <summary>
    /// Indicates if the currently selected ROM is the ZX Spectrum 48 ROM
    /// </summary>
    public override bool IsSpectrum48RomSelected => _selectedRom == 1;

    /// <summary>
    /// Reads the screen memory byte
    /// </summary>
    /// <param name="offset">Offset from the beginning of the screen memory</param>
    /// <returns>The byte at the specified screen memory location</returns>
    public override byte ReadScreenMemory(ushort offset) 
        => _ramBanks[_useShadowScreen ? 7 : 5][offset & 0x3fff];

    /// <summary>
    /// Every time the CPU clock is incremented, this function is executed.
    /// </summary>
    /// <param name="increment">The tact increment value</param>
    public override void OnTactIncremented(int increment)
    {
        base.OnTactIncremented(increment);
        PsgDevice.SetNextAudioSample();
    }

    /// <summary>
    /// Check for current tape mode
    /// </summary>
    protected override void AfterInstructionExecuted()
    {
        base.AfterInstructionExecuted();
        PsgDevice.CalculateCurrentAudioValue();
    }

    /// <summary>
    /// The machine's execution loop calls this method when it is about to initialize a new frame.
    /// </summary>
    /// <param name="clockMultiplierChanged">
    /// Indicates if the clock multiplier has been changed since the execution of the previous frame.
    /// </param>
    protected override void OnInitNewFrame(bool clockMultiplierChanged)
    {
        // --- No screen tact rendered in this frame
        LastRenderedFrameTact = 0;

        // --- Prepare the screen device for the new machine frame
        ScreenDevice.OnNewFrame();

        // --- Handle audio sample recalculations when the actual clock frequency changes
        if (OldClockMultiplier != ClockMultiplier)
        {
            BeeperDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
            PsgDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
            OldClockMultiplier = ClockMultiplier;
        }

        // --- Prepare the beeper device for the new frame
        BeeperDevice.OnNewFrame();
        PsgDevice.OnNewFrame();
    }

    /// <summary>
    /// Get the 64K of addressable memory of the ZX Spectrum computer
    /// </summary>
    /// <returns>Bytes of the flat memory</returns>
    public override byte[] Get64KFlatMemory()
    {
        var memory = new byte[0x1_0000];
        _romPages[_selectedRom].CopyTo(memory, 0);
        _ramBanks[5].CopyTo(memory, 0x4000);
        _ramBanks[2].CopyTo(memory, 0x8000);
        _ramBanks[_selectedBank].CopyTo(memory, 0xc000);
        return memory;
    }

    /// <summary>
    /// Get the specified 16K partition (page or bank) of the ZX Spectrum computer
    /// </summary>
    /// <param name="index">Partition index</param>
    /// <returns>Bytes of the partition</returns>
    /// <remarks>
    /// Less than zero: ROM pages
    /// 0..7: RAM bank with the specified index
    /// </remarks>
    public override byte[] Get16KPartition(int index)
    {
        return index switch
        {
            -1 => _romPages[0],
            -2 => _romPages[1],
            >= 0 and <= 7 => _ramBanks[index],
            _ => throw new InvalidOperationException($"Invalid 16K partition index: {index}")
        };
    }

    /// <summary>
    /// Gets the audio sample rate
    /// </summary>
    public override int GetAudioSampleRate() => BeeperDevice.GetAudioSampleRate();

    /// <summary>
    /// Gets the audio samples rendered in the current frame
    /// </summary>
    /// <returns>Array with the audio samples</returns>
    public override float[] GetAudioSamples()
    {
        var beeperSamples = BeeperDevice.GetAudioSamples();
        var psgSamples = PsgDevice.GetAudioSamples();
        var samplesCount = Math.Min(beeperSamples.Length, psgSamples.Length);
        var sumSamples = new float[samplesCount];
        for (var i = 0; i < samplesCount; i++)
        {
            sumSamples[i] = beeperSamples[i] + psgSamples[i];
        }
        return sumSamples;
    }

    /// <summary>
    /// Get the number of T-states in a display line (use -1, if this info is not available)
    /// </summary>
    public override int TactsInDisplayLine => ScreenDevice.ScreenWidth;

    #endregion

    #region Memory Device

    /// <summary>
    /// Read the byte at the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <returns>The byte read from the memory</returns>
    public override byte DoReadMemory(ushort address)
    {
        var memIndex = address & 0x3FFF;
        return (address & 0xC000) switch
        {
            0x0000 => _romPages[_selectedRom][memIndex],
            0x4000 => _ramBanks[5][memIndex],
            0x8000 => _ramBanks[2][memIndex],
            _ => _ramBanks[_selectedBank][memIndex]
        };
    }

    /// <summary>
    /// Write the given byte to the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    public override void DoWriteMemory(ushort address, byte value)
    {
        var memIndex = address & 0x3FFF;
        switch (address & 0xc000)
        {
            case 0x0000:
                return;
            case 0x4000:
                _ramBanks[5][memIndex] = value;
                return;
            case 0x8000:
                _ramBanks[2][memIndex] = value;
                return;
            default:
                _ramBanks[_selectedBank][memIndex] = value;
                return;
        }
    }

    /// <summary>
    /// This method implements memory operation delays.
    /// </summary>
    /// <param name="address"></param>
    /// <remarks>
    /// Whenever the CPU accesses the 0x4000-0x7fff memory range, it contends with the ULA. We keep the contention
    /// delay values for a particular machine frame tact in _contentionValues.Independently of the memory address, 
    /// the Z80 CPU takes 3 T-states to read or write the memory contents.
    /// </remarks>
    public override void DelayAddressBusAccess(ushort address)
    {
        var page = address & 0xc000;
        if (page != 0x4000 && (page != 0xc000 || (_selectedBank & 0x01) != 1)) return;
        
        // --- We read from contended memory
        var delay = GetContentionValue(CurrentFrameTact);
        TactPlusN(delay);
        TotalContentionDelaySinceStart += delay;
        ContentionDelaySincePause += delay;
    }

    #endregion

    #region I/O port handling

    /// <summary>
    /// This function reads a byte (8-bit) from an I/O port using the provided 16-bit address.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port read operation.
    /// </remarks>
    public override byte DoReadPort(ushort address)
    {
        if ((address & 0x0001) == 0)
        {
            // --- Standard ZX Spectrum 48 I/O read
            return ReadPort0Xfe(address);
        } 
        
        // --- Handle the Kempston port
        if ((address & 0x00e0) != 0) {
            // TODO: Implement Kempston port handling
            return 0xff;
        }
        
        // --- Handle the PSG register index port
        if ((address & 0xc002) == 0xc000)
        {
            return PsgDevice.ReadPsgRegisterValue();
        }

        return FloatingBusDevice.ReadFloatingBus();
    }

    /// <summary>
    /// This function implements the I/O port read delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    public override void DelayPortRead(ushort address) => DelayContendedIo(address);

    /// <summary>
    /// This function writes a byte (8-bit) to the 16-bit I/O port address provided in the first argument.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port write operation.
    /// </remarks>
    public override void DoWritePort(ushort address, byte value)
    {
        // --- Standard ZX Spectrum 48 port
        if ((address & 0x0001) == 0)
        {
            WritePort0xFE(value);
            return;
        }

        // --- Memory paging port
        if ((address & 0xc002) == 0x4000)
        {
            // --- Abort if paging is not enabled
            if (!_pagingEnabled) return;
            
            // --- Choose the RAM bank for Slot 3 (0xc000-0xffff)
            _selectedBank = value & 0x07;

            // --- Choose screen (Bank 5 or 7)
            _useShadowScreen = ((value >> 3) & 0x01) == 0x01;

            // --- Choose ROM bank for Slot 0 (0x0000-0x3fff)
            _selectedRom = (value >> 4) & 0x01;

            // --- Enable/disable paging
            _pagingEnabled = (value & 0x20) == 0x00;

            return;
        }
        
        // --- Test for PSG register index port
        if ((address & 0xc002) == 0xc000) {
            PsgDevice.SetPsgRegisterIndex((byte)(value & 0x0f));
            return;
        }
        
        // --- Test for PSG register value port
        if ((address & 0xc002) == 0x8000) {
            PsgDevice.WritePsgRegisterValue(value);
        }
    }

    /// <summary>
    /// This function implements the I/O port write delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    public override void DelayPortWrite(ushort address) => DelayContendedIo(address);

    #endregion

    #region Display

    /// <summary>
    /// Width of the screen in native machine screen pixels
    /// </summary>
    public override int ScreenWidthInPixels => ScreenDevice.ScreenWidth;

    /// <summary>
    /// Height of the screen in native machine screen pixels
    /// </summary>
    public override int ScreenHeightInPixels => ScreenDevice.ScreenLines;

    /// <summary>
    /// Gets the buffer that stores the rendered pixels
    /// </summary>
    public override uint[] GetPixelBuffer() => ScreenDevice.GetPixelBuffer();

    #endregion

    /// <summary>
    /// Uploades the specified ROM information to the ZX Spectrum 128 ROM memory
    /// </summary>
    /// <param name="pageIndex">ROM page index</param>
    /// <param name="data">ROM contents</param>
    private void UploadRomBytes(int pageIndex, byte[] data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            _romPages[pageIndex][i] = data[i];
        }
    }
}