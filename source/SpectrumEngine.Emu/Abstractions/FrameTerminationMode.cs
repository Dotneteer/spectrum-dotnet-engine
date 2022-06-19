namespace SpectrumEngine.Emu;

/// <summary>
/// This enum defines the termination condition for the machine frame.
/// </summary>
public enum FrameTerminationMode
{
    /// <summary>
    /// Normal mode: the frame terminates when the current frame completes.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// The execution completes when a debugger event occurs (e.g., stopping at a breakpoint).
    /// </summary>
    DebugEvent,

    /// <summary>
    /// The execution completes when the current PC address (and an optional memory partition) reaches a specified termination point.
    /// </summary>
    UntilExecutionPoint,
}
