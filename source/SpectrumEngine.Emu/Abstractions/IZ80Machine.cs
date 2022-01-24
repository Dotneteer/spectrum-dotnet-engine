namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the behavior of a virtual machine that integrates the emulator from separate hardware
/// components, including the Z80 CPU, the memory, screen, keyboard, and many other devices.
/// </summary>
public interface IZ80Machine
{
    /// <summary>
    /// This property stores the execution context where the emulated machine runs its execution loop.
    /// </summary>
    ExecutionContext ExecutionContext { get; }

    /// <summary>
    /// Get the Z80 instance associated with the machine.
    /// </summary>
    IZ80Cpu Cpu { get; }

    /// <summary>
    /// This method provides a way to configure (or reconfigure) the emulated machine after changing the properties
    /// of its components.
    /// </summary>
    void Configure();

    /// <summary>
    /// Emulates turning on a machine (after it has been turned off).
    /// </summary>
    void HardReset();

    /// <summary>
    /// This method emulates resetting a machine with a hardware reset button.
    /// </summary>
    void Reset();

    /// <summary>
    /// Executes the machine loop using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    LoopTerminationMode ExecuteMachineLoop();
}