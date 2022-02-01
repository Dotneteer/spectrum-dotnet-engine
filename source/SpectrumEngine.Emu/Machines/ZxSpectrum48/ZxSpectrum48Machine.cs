namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents the emulator of a ZX Spectrum 48 machine.
/// </summary>
public sealed class ZxSpectrum48Machine :
    Z80MachineBase,
    IZxSpectrum48Machine
{
    #region Private members

    /// <summary>
    /// This byte array represents the 64K memory, including the 16K ROM and 48K RAM.
    /// </summary>
    private readonly byte[] _memory = new byte[0x1_0000];

    /// <summary>
    /// This byte array stores the contention values associated with a particular machine frame tact.
    /// </summary>
    private byte[] _contentionValues = Array.Empty<byte>();

    /// <summary>
    /// Stores the last rendered machine frame tact.
    /// </summary>
    private int _lastRenderedFrameTact;

    #endregion

    #region Initialization and Properties

    /// <summary>
    /// Initialize the machine
    /// </summary>
    public ZxSpectrum48Machine()
    {
        // --- Set up machine attributes
        BaseClockFrequency = 3_500_000;
        ClockMultiplier = 1;

        // --- Create and initialize devices
        IoHandler = new ZxSpectrum48IoHandler(this);
        KeyboardDevice = new KeyboardDevice(this);
        ScreenDevice = new ScreenDevice(this);
        BeeperDevice = new BeeperDevice(this);
        FloatingBusDevice = new ZxSpectrum48FloatingBusDevice(this);
        TapeDevice = new TapeDevice(this);
        Reset();

        // --- Bind the CPU and I/O
        ReadPortFunction = IoHandler.ReadPort;
        WritePortFunction = IoHandler.WritePort;

        // --- Set up devices
        ScreenDevice.SetMemoryScreenOffset(0x4000);

        // --- Initialize the machine's ROM
        UploadRomBytes(LoadRomFromResource(DefaultRomResource));
    }

    /// <summary>
    /// Specify the name of the default ROM's resource file within this assembly.
    /// </summary>
    protected override string DefaultRomResource => "ZxSpectrum48";

    /// <summary>
    /// Gets the ULA issue number of the ZX Spectrum model (2 or 3)
    /// </summary>
    public int UlaIssue { get; set; } = 3;

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
    public IFloatingBusDevice FloatingBusDevice { get; }

    /// <summary>
    /// Represents the tape device of ZX Spectrum 48K
    /// </summary>
    public ITapeDevice TapeDevice { get; }

    /// <summary>
    /// Represents the CPU's I/O handler to read and write I/O ports.
    /// </summary>
    public IIoHandler<IZxSpectrum48Machine> IoHandler { get; }

    /// <summary>
    /// Emulates turning on a machine (after it has been turned off).
    /// </summary>
    public override void HardReset()
    {
        base.HardReset();
        Reset();
    }

    /// <summary>
    /// This method emulates resetting a machine with a hardware reset button.
    /// </summary>
    public override void Reset()
    {
        // --- Reset the CPU
        base.Reset();

        // --- Reset memory
        for (var i = 0x4000; i < _memory.Length; i++) _memory[i] = 0;

        // --- Reset devices
        IoHandler.Reset();
        KeyboardDevice.Reset();
        ScreenDevice.Reset();
        BeeperDevice.Reset();
        FloatingBusDevice.Reset();
        TapeDevice.Reset();

        // --- Prepare for running a new machine loop
        ClockMultiplier = TargetClockMultiplier;
        ExecutionContext.LastTerminationReason = null;
        _lastRenderedFrameTact = -1;
    }

    #endregion

    #region Memory Device

    /// <summary>
    /// Read the byte at the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <returns>The byte read from the memory</returns>
    public override byte DoReadMemory(ushort address)
        => _memory[address];

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
        if ((address & 0xc000) == 0x4000)
        {
            // --- We read from contended memory
            var delay = _contentionValues[CurrentFrameTact / ClockMultiplier];
            TactPlusN(delay);
        }
    }

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
    public void AllocateContentionValues(int tactsInFrame)
    {
        _contentionValues = new byte[tactsInFrame];
    }

    /// <summary>
    /// This method sets the contention value associated with the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <param name="value">Contention value</param>
    public void SetContentionValue(int tact, byte value)
    {
        _contentionValues[tact] = value;
    }

    /// <summary>
    /// This method gets the contention value for the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <returns>The contention value associated with the specified tact.</returns>
    public byte GetContentionValue(int tact)
    {
        return _contentionValues[tact];
    }

    #endregion

    /// <summary>
    /// The machine's execution loop calls this method when it is about to initialize a new frame.
    /// </summary>
    /// <param name="clockMultiplierChanged">
    /// Indicates if the clock multiplier has been changed since the execution of the previous frame.
    /// </param>
    protected override void OnInitNewFrame(bool clockMultiplierChanged)
    {
    }

    /// <summary>
    /// Tests if the machine should raise a Z80 maskable interrupt
    /// </summary>
    /// <returns>
    /// True, if the INT signal should be active; otherwise, false.
    /// </returns>
    protected override bool ShouldRaiseInterrupt() => CurrentFrameTact / ClockMultiplier < 32;

    /// <summary>
    /// 
    /// </summary>
    protected override void AfterInstructionExecuted()
    {
    }

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    public override void OnTactIncremented(ulong oldTact)
    {
        var machineTact = CurrentFrameTact / ClockMultiplier;
        if (_lastRenderedFrameTact != machineTact)
        {
            // --- Render the current frame tact
        }
        _lastRenderedFrameTact = machineTact;
    }

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
