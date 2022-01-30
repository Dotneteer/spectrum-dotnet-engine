namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition defines the state information we use while emulating the behavior of the CPU.
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// The registers of the CPU
    /// </summary>
    public Registers Regs { get; private set; } = new();

    /// <summary>
    /// The current state of the Z80 signal flags
    /// </summary>
    public Z80Signals SignalFlags { get; set; } = Z80Signals.None;

    /// <summary>
    /// The current maskable interrupt mode (0, 1, or 2)
    /// </summary>
    public int InterruptMode { get; private set; } = 0;

    /// <summary>
    /// Interrupt Enable Flip-Flop
    /// </summary>
    /// <remarks>
    /// If set, the CPU accepts maskable interrupt; otherwise, not.
    /// </remarks>
    public bool Iff1 { get; set; } = false;

    /// <summary>
    /// Temporary storage for Iff1.
    /// </summary>
    /// <remarks>
    /// The purpose of IFF2 is to save the status of IFF1 when a non-maskable interrupt occurs. When a non-maskable
    /// interrupt is accepted, IFF1 resets to prevent further interrupts until reenabled by the programmer. Therefore,
    /// after a non-maskable interrupt is accepted, maskable interrupts are disabled, but the previous state of IFF1
    /// is saved so that the complete state of the CPU just prior to the non-maskable interrupt can be restored at any
    /// time.
    /// </remarks>
    public bool Iff2 { get; set; } = false;

    /// <summary>
    /// This flag indicates if the CPU is in a halted state.
    /// </summary>
    public bool Halted { get; private set; }

    /// <summary>
    /// The number of T-states (clock cycles) elapsed since the last reset
    /// </summary>
    public ulong Tacts { get; set; }

    /// <summary>
    /// Show the number of machine frames completed since the CPU started.
    /// </summary>
    public int Frames => (int)(Tacts / ((ulong)TactsInFrame * (ulong)ClockMultiplier));

    /// <summary>
    /// Get the current frame tact within the machine frame being executed.
    /// </summary>
    public int CurrentFrameTact => (int)(Tacts / ((ulong)TactsInFrame * (ulong)ClockMultiplier));

    /// <summary>
    /// Get the number of T-states in a machine frame.
    /// </summary>
    public int TactsInFrame { get; private set; } = 100_000_000;

    /// <summary>
    /// This property gets or sets the value of the current clock multiplier.
    /// </summary>
    /// <remarks>
    /// By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock
    /// frequency multiplier to emulate a faster CPU.
    /// </remarks>
    public int ClockMultiplier { get; set; }

    /// <summary>
    /// This flag indicates that the current CPU frame has been completed since the last reset of the flag.
    /// </summary>
    public bool FrameCompleted { get; private set; }

    /// <summary>
    /// This flag indicates if bit 3 or 5 of Register F has been updated. We need to keep this value, as we utilize
    /// it within the `SCF` and `CCF` instructions to calculate the new values of bit 3 and 5 of F.
    /// </summary>
    public bool F53Updated { get; private set; } = false;

    /// <summary>
    /// When calculating the value of bit 3 and 5 of Register F within the `SCF` and `CCF` instructions, we must know
    /// whether the last executed instruction has updated these flags. This field stores this information.
    /// </summary>
    public bool PrevF53Updated { get; private set; } = false;

    /// <summary>
    /// The last fetched opcode. If an instruction is prefixed, it contains the prefix or the opcode following the
    /// prefix, depending on which was fetched last.
    /// </summary>
    public byte OpCode { get; private set; }

    /// <summary>
    /// The current prefix to consider when processing the subsequent opcode.
    /// </summary>
    public OpCodePrefix Prefix { get; private set; }

    /// <summary>
    /// We use this variable to handle the EI instruction properly.
    /// </summary>
    /// <remarks>
    /// When an EI instruction is executed, any pending interrupt request is not accepted until after the instruction
    /// following EI is executed. This single instruction delay is necessary when the next instruction is a return
    /// instruction. Interrupts are not allowed until a return is completed.
    /// </remarks>
    public int EiBacklog { get; private set; }

    /// <summary>
    /// We need this flag to implement the step-over debugger function that continues the execution and stops when the
    /// current subroutine returns to its caller. The debugger will observe the change of this flag and manage its
    /// internal tracking of the call stack accordingly.
    /// </summary>
    public bool RetExecuted { get; private set; } = false;

    /// <summary>
    /// This flag is reserved for future extension. The ZX Spectrum Next computer uses additional Z80 instructions.
    /// This flag indicates if those are allowed.
    /// </summary>
    public bool AllowExtendedInstructions { get; set; } = false;

    /// <summary>
    /// This flag indicates if the Z80 CPU works in hardware with memory contention between the CPU and other hardware
    /// components.
    /// </summary>
    public bool MemoryContended = false;

    /// <summary>
    /// This flag indicates whether the Z80 CPU works in hardware with a gate array controlled memory contention
    /// between the CPU and other components.
    /// </summary>
    public bool GateArray = false;
}
