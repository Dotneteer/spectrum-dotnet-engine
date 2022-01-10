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
    public readonly Registers Regs = new ();

    /// <summary>
    /// The current state of the Z80 signal flags
    /// </summary>
    public Z80Signals SignalFlags = Z80Signals.None;

    /// <summary>
    /// The current maskable interrupt mode (0, 1, or 2)
    /// </summary>
    public int InterruptMode = 0;

    /// <summary>
    /// Interrupt Enable Flip-Flop
    /// </summary>
    /// <remarks>
    /// If set, the CPU accepts maskable interrupt; otherwise, not.
    /// </remarks>
    public bool Iff1 = false;

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
    public bool Iff2 = false;

    /// <summary>
    /// The number of T-states (clock cycles) elapsed since the last reset
    /// </summary>
    public ulong Tacts = 0;

    /// <summary>
    /// This flag indicates if bit 3 or 5 of Register F has been updated. We need to keep this value, as we utilize
    /// it within the `SCF` and `CCF` instructions to calculate the new values of bit 3 and 5 of F.
    /// </summary>
    public bool F53Updated = false;

    /// <summary>
    /// When calculating the value of bit 3 and 5 of Register F within the `SCF` and `CCF` instructions, we must know
    /// whether the last executed instruction has updated these flags. This field stores this information.
    /// </summary>
    public bool PrevF53Updated = false;

    /// <summary>
    /// The last fetched opcode. If an instruction is prefixed, it contains the prefix or the opcode following the
    /// prefix, depending on which was fetched last.
    /// </summary>
    public byte OpCode;

    /// <summary>
    /// The current prefix to consider when processing the subsequent opcode.
    /// </summary>
    public OpCodePrefix Prefix;

    /// <summary>
    /// We use this variable to handle the EI instruction properly.
    /// </summary>
    /// <remarks>
    /// When an EI instruction is executed, any pending interrupt request is not accepted until after the instruction
    /// following EI is executed. This single instruction delay is necessary when the next instruction is a return
    /// instruction. Interrupts are not allowed until a return is completed.
    /// </remarks>
    public int EiBacklog;

    /// <summary>
    /// We need this flag to implement the step-over debugger function that continues the execution and stops when the
    /// current subroutine returns to its caller. The debugger will observe the change of this flag and manage its
    /// internal tracking of the call stack accordingly.
    /// </summary>
    public bool RetExecuted;
}
