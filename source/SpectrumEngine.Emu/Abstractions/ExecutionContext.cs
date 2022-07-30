namespace SpectrumEngine.Emu;

/// <summary>
/// This class defines the execution context in which an emulated machine can run its execution loop.
/// </summary>
public sealed class ExecutionContext
{
    /// <summary>
    /// This property defines how the machine's execution loop completes.
    /// </summary>
    public FrameTerminationMode FrameTerminationMode { get; set; }

    /// <summary>
    /// This property defines how the machine execution loop should handle the debug mode.
    /// </summary>
    public DebugStepMode DebugStepMode { get; set; }

    /// <summary>
    /// The optional termination partition, at which the execution loop should stop when it is in the
    /// UntilExecutionPoint loop termination mode. For example, in the case of ZX Spectrum 48K, this property has no
    /// meaning. For ZX Spectrum 128K (and above), this value may be the current ROM index.
    /// </summary>
    public int? TerminationPartition { get; set; }

    /// <summary>
    /// This optional 16-bit value defines the PC value that is considered the termination point, provided the
    /// execution loop is in the UntilExecutionPoint loop termination mode.
    /// </summary>
    public ushort? TerminationPoint { get; set; }

    /// <summary>
    /// This property describes the termination reason of the last machine execution loop. It returns null if the
    /// execution loop has not been started at least once.
    /// </summary>
    public FrameTerminationMode? LastTerminationReason { get; set; }

    /// <summary>
    /// Has the last execution loop cancelled?
    /// </summary>
    public bool Canceled { get; set; }
    
    /// <summary>
    /// The object that provides debug support for the machone
    /// </summary>
    public IDebugSupport? DebugSupport { get; set; }
}
