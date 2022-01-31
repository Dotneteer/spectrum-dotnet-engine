namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents the emulator of a ZX Spectrum 48 machine.
/// </summary>
public sealed class ZxSpectrum48Machine :
    Z80MachineBase,
    IZxSpectrum48Machine
{
    private int _lastRenderedFrameTact;

    /// <summary>
    /// Specify the name of the default ROM's resource file within this assembly.
    /// </summary>
    protected override string DefaultRomResource => "ZxSpectrum48";

    /// <summary>
    /// Initialize the machine
    /// </summary>
    public ZxSpectrum48Machine()
    {
        // --- Set up machine attributes
        BaseClockFrequency = 3_500_000;
        ClockMultiplier = 1;

        // --- Create and initialize devices
        MemoryDevice = new ZxSpectrum48MemoryDevice(this);
        IoHandler = new ZxSpectrum48IoHandler(this);
        KeyboardDevice = new KeyboardDevice(this);
        ScreenDevice = new ScreenDevice(this);
        BeeperDevice = new BeeperDevice(this);
        FloatingBusDevice = new ZxSpectrum48FloatingBusDevice(this);
        TapeDevice = new TapeDevice(this);
        Reset();

        // --- Bind the CPU, memory, and I/O
        ReadMemoryFunction = MemoryDevice.ReadMemory;
        WriteMemoryFunction = MemoryDevice.WriteMemory;
        ReadPortFunction = IoHandler.ReadPort;
        WritePortFunction = IoHandler.WritePort;
        TactIncrementedHandler = OnTactIncremented;

        // --- Set up devices
        ScreenDevice.SetMemoryScreenOffset(0x4000);

        // --- Initialize the machine's ROM
        UploadRomBytes(LoadRomFromResource(DefaultRomResource));
    }

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
    /// Represents the CPU's memory handler to read and write the memory contents.
    /// </summary>
    public IMemoryDevice MemoryDevice { get; }

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
        base.Reset();
        MemoryDevice.Reset();
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
    protected override void OnTactIncremented(ulong oldTact)
    {
        var machineTact = CurrentFrameTact / ClockMultiplier;
        if (_lastRenderedFrameTact != machineTact)
        {
            // --- Render the current frame tact
        }
        _lastRenderedFrameTact = machineTact;
        //TODO: Implement this method
    }

    /// <summary>
    /// Uploades the specified ROM information to the ZX Spectrum 48 ROM memory
    /// </summary>
    /// <param name="data">ROM contents</param>
    private void UploadRomBytes(byte[] data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            MemoryDevice.DirectWrite((ushort)i, data[i]);
        }
    }
}
