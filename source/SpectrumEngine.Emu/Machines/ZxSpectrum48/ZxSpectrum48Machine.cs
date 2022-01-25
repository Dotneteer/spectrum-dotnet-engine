namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents the emulator of a ZX Spectrum 48 machine.
/// </summary>
public sealed class ZxSpectrum48Machine : Z80MachineBase, IZxSpectrum48Machine
{
    /// <summary>
    /// Represents the keyboard device of ZX Spectrum 48K
    /// </summary>
    public IKeyboardDevice KeyboardDevice { get; } = new KeyboardDevice();

    /// <summary>
    /// Represents the screen device of ZX Spectrum 48K
    /// </summary>
    public IScreenDevice ScreenDevice { get; } = new ScreenDevice();

    /// <summary>
    /// Represents the beeper device of ZX Spectrum 48K
    /// </summary>
    public IBeeperDevice BeeperDevice { get; } = new BeeperDevice();

    /// <summary>
    /// Represents the floating port device of ZX Spectrum 48K
    /// </summary>
    public IFloatingPortDevice FloatingPortDevice { get; } = new ZxSpectrum48FloatingPortDevice();

    /// <summary>
    /// Represents the tape device of ZX Spectrum 48K
    /// </summary>
    public ITapeDevice TapeDevice { get; } = new TapeDevice();

    /// <summary>
    /// Represents the memory device of ZX Spectrum 48K
    /// </summary>
    public override IMemoryDevice MemoryDevice { get; } = new ZxSpectrum48MemoryDevice();

    /// <summary>
    /// Represents the I/O handler of ZX Spectrum 48K
    /// </summary>
    public override IIoHandler IoHandler { get; } = new ZxSpectrum48IoHandler();

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
    protected override void OnTactImcremented()
    {
        throw new NotImplementedException();
    }
}
