namespace SpectrumEngine.Emu;

/// <summary>
/// This class is intended to be a reusable base class for emulators using the Z80 CPU.
/// </summary>
public abstract class Z80MachineBase : IZ80Machine
{
    /// <summary>
    /// This property stores the execution context where the emulated machine runs its execution loop.
    /// </summary>
    public ExecutionContext ExecutionContext { get; } = new();

    /// <summary>
    /// Get the Z80 instance associated with the machine.
    /// </summary>
    public IZ80Cpu Cpu { get; } = new Z80Cpu();

    /// <summary>
    /// This property gets or sets the value of the current clock multiplier.
    /// </summary>
    /// <remarks>
    /// By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock
    /// frequency multiplier to emulate a faster CPU.
    /// </remarks>
    public int ClockMultiplier { get; set; }

    /// <summary>
    /// Represents the CPU's memory handler to read and write the memory contents.
    /// </summary>
    public abstract IMemoryDevice MemoryDevice { get; }

    /// <summary>
    /// Represents the CPU's I/O handler to read and write I/O ports.
    /// </summary>
    public abstract IIoHandler IoHandler { get; }

    /// <summary>
    /// This method provides a way to configure (or reconfigure) the emulated machine after changing the properties
    /// of its components.
    /// </summary>
    public virtual void Configure()
    {
        // --- Implement this method in derived classes
    }

    /// <summary>
    /// Executes the machine loop using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    public abstract LoopTerminationMode ExecuteMachineLoop();

    /// <summary>
    /// Emulates turning on a machine (after it has been turned off).
    /// </summary>
    public virtual void HardReset()
    {
        Cpu.HardReset();
    }

    /// <summary>
    /// This method emulates resetting a machine with a hardware reset button.
    /// </summary>
    public virtual void Reset()
    {
        Cpu.Reset();
    }

    /// <summary>
    /// Initialize the machine by connecting the CPU with the memory device and the I/O handler.
    /// </summary>
    protected Z80MachineBase()
    {
        Cpu.ReadMemoryFunction = MemoryDevice.ReadMemory;
        Cpu.WriteMemoryFunction = MemoryDevice.WriteMemory;
        Cpu.ReadPortFunction = IoHandler.ReadPort;
        Cpu.WritePortFunction = IoHandler.WritePort;
        Cpu.TactIncrementedHandler = OnTactImcremented;
    }

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    /// <remarks>
    /// Override in derived classes to implement hardware components running parallel with the CPU.
    /// </remarks>
    protected abstract void OnTactImcremented();
}
