namespace SpectrumEngine.Emu;

/// <summary>
/// This enum defines how the machine execution loop should behave in debug mode.
/// </summary>
public enum DebugStepMode
{
    /// <summary>
    /// The debug mode is turned off, the machine should ignore breakpoints.
    /// </summary>
    NoDebug = 0,

    /// <summary>
    /// The execution loop should terminate as soon as it reaches an active breakpoint.
    /// </summary>
    StopAtBreakpoint,

    /// <summary>
    /// The execution loop should stop after completing the subsequent CPU instruction.
    /// </summary>
    StepInto,

    /// <summary>
    /// The execution loop should stop after the PC reaches the address of subsequent CPU instruction. If the current
    /// instruction is a subroutine call, the execution stops when the subroutine returns. If the instruction is a
    /// block instruction, the execution stops when the block completes. Should the instruction be a HALT, the loop
    /// terminates as the CPU gets out of the halted mode.
    /// </summary>
    StepOver,

    /// <summary>
    /// The execution loop stops after the first RET instruction (conditional or unconditional) when it returns to its caller.
    /// </summary>
    StepOut
}