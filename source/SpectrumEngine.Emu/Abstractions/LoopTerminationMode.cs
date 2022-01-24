namespace SpectrumEngine.Emu;

/// <summary>
/// This enum defines the termination condition for the machine execution loop.
/// </summary>
public enum LoopTerminationMode
{
    /// <summary>
    /// Normal mode: the execution loop terminates when the current frame completes.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// The execution completes when a debugger event occurs (e.g., stopping at a breakpoint).
    /// </summary>
    DebugEvent,

    /// <summary>
    /// The execution loop completes when the CPU gets halted.
    /// </summary>
    UntilHalt,

    /// <summary>
    /// The execution loop completes when the current PC address (and an optional memory partition) reaches a specified termination point.
    /// </summary>
    UntilExecutionPoint,
}
