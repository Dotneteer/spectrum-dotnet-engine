// ReSharper disable VirtualMemberCallInConstructor
namespace SpectrumEngine.Emu.ZxSpectrum128;

/// <summary>
/// This class represents the emulator of a ZX Spectrum 128 machine.
/// </summary>
public class ZxSpectrum128Machine: ZxSpectrumBase
{
    #region Private members

    // --- This byte array represents the 64K memory, including the 16K ROM and 48K RAM.
    private readonly byte[] _memory = new byte[0x1_0000];

    // --- Stores the memory information of the ROM pages
    private readonly byte[][] _romPages;
    private readonly byte[][] _ramBanks;
    private int _selectedRom = 0;
    private int _selectedBank = 0;

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

        // --- Select the default ROM page and RAM bank
        _selectedRom = 0;
        _selectedBank = 0;
         
        // --- Create and initialize devices
        KeyboardDevice = new KeyboardDevice(this);
        ScreenDevice = new CommonScreenDevice(this, CommonScreenDevice.ZxSpectrum128ScreenConfiguration);
        BeeperDevice = new BeeperDevice(this);
        FloatingBusDevice = new ZxSpectrum128FloatingBusDevice(this);
        TapeDevice = new TapeDevice(this);
        Reset();

        // --- Initialize the machine's ROM (Roms/ZxSpectrum48/ZxSpectrum48.rom)
        UploadRomBytes(LoadRomFromResource(DefaultRomResource));
        
        // --- Allow access to the 64Kbyte of memory
        SetMachineProperty(MachinePropNames.MemoryFlat, _memory);
    }

    /// <summary>
    /// Specify the name of the default ROM's resource file within this assembly.
    /// </summary>
    protected override string DefaultRomResource => "ZxSpectrum48";

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
        
        // --- Reset and setup devices
        KeyboardDevice.Reset();
        ScreenDevice.Reset();
        BeeperDevice.Reset();
        BeeperDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
        FloatingBusDevice.Reset();
        TapeDevice.Reset();
        
        // --- Set default property values
        SetMachineProperty(MachinePropNames.TapeMode, TapeMode.Passive);
        SetMachineProperty(MachinePropNames.RewindRequested, null);

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
    /// This function implements the memory read delay of the CPU.
    /// </summary>
    /// <param name="address">Memory address to read</param>
    /// <remarks>
    /// Normally, it is exactly 3 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 3-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 3 T-states!
    /// </remarks>
    public override void DelayMemoryRead(ushort address)
    {
        DelayAddressBusAccess(address);
        TactPlus3();
        TotalContentionDelaySinceStart += 3;
        ContentionDelaySincePause += 3;
    }

    /// <summary>
    /// Write the given byte to the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    public override void DoWriteMemory(ushort address, byte value)
    {
        if ((address & 0xc000) != 0x0000)
        {
            _memory[address] = value;
        }
    }

    /// <summary>
    /// This function implements the memory write delay of the CPU.
    /// </summary>
    /// <param name="address">Memory address to write</param>
    /// <remarks>
    /// Normally, it is exactly 3 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 3-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 3 T-states!
    /// </remarks>
    public override void DelayMemoryWrite(ushort address)
    {
        DelayAddressBusAccess(address);
        TactPlus3();
        TotalContentionDelaySinceStart += 3;
        ContentionDelaySincePause += 3;
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
        var delay = GetContentionValue(CurrentFrameTact / ClockMultiplier);
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
        return (address & 0x0001) == 0 
            ? ReadPort0Xfe(address)
            : FloatingBusDevice.ReadFloatingBus();
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
        if ((address & 0x0001) == 0)
        {
            WritePort0xFE(value);
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
    /// Uploades the specified ROM information to the ZX Spectrum 48 ROM memory
    /// </summary>
    /// <param name="data">ROM contents</param>
    private void UploadRomBytes(byte[] data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            _memory[i] = data[i];
        }
    }
}