namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents the emulator of a ZX Spectrum 48 machine.
/// </summary>
public sealed class ZxSpectrum48Machine : Z80MachineBase, IZxSpectrum48Machine
{
    /// <summary>
    /// Initialize the machine
    /// </summary>
    public ZxSpectrum48Machine()
    {
        MemoryDevice = new ZxSpectrum48MemoryDevice(this);
        IoHandler = new ZxSpectrum48IoHandler(this);
        KeyboardDevice = new KeyboardDevice(this);
        ScreenDevice = new ScreenDevice(this);
        BeeperDevice = new BeeperDevice(this);
        FloatingBusDevice = new ZxSpectrum48FloatingBusDevice(this);
        TapeDevice = new TapeDevice(this);
        Reset();

        // --- Bind the CPU, memory, and I/O
        Cpu.ReadMemoryFunction = MemoryDevice.ReadMemory;
        Cpu.WriteMemoryFunction = MemoryDevice.WriteMemory;
        Cpu.ReadPortFunction = IoHandler.ReadPort;
        Cpu.WritePortFunction = IoHandler.WritePort;
        Cpu.TactIncrementedHandler = OnTactIncremented;
    }

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
    }

    /// <summary>
    /// Executes the machine loop using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    public override LoopTerminationMode ExecuteMachineLoop()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    protected override void OnTactIncremented()
    {
        throw new NotImplementedException();
    }
}
