using System.Runtime.CompilerServices;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition lists the methods that contribute to the CPU's execution cycle.
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// Executes a hard reset as if the machine and the CPU had just been turned on.
    /// </summary>
    public void HardReset()
    {
        Regs.AF = 0xffff;
        Regs._AF_ = 0xffff;
        Regs.BC = 0x0000;
        Regs._BC_ = 0x0000;
        Regs.DE = 0x0000;
        Regs._DE_ = 0x0000;
        Regs.HL = 0x0000;
        Regs._HL_ = 0x0000;
        Regs.IX = 0x0000;
        Regs.IY = 0x0000;
        Regs.IR = 0x0000;
        Regs.PC = 0x0000;
        Regs.SP = 0xffff;
        Regs.WZ = 0x0000;

        SignalFlags = Z80Signals.None;
        InterruptMode = 0;
        Iff1 = false;
        Iff2 = false;
        ClockMultiplier = 1;
        FrameCompleted = true;
        F53Updated = false;
        PrevF53Updated = false;

        OpCode = 0;
        Prefix = OpCodePrefix.None;
        EiBacklog = 0;
        RetExecuted = false;
    }

    /// <summary>
    /// Handles the active RESET signal of the CPU.
    /// </summary>
    public void Reset()
    {
        Regs.AF = 0xffff;
        Regs._AF_ = 0xffff;
        Regs.IR = 0x0000;
        Regs.PC = 0x0000;
        Regs.SP = 0xffff;
        Regs.WZ = 0x0000;

        SignalFlags = Z80Signals.None;
        InterruptMode = 0;
        Iff1 = false;
        Iff2 = false;
        ClockMultiplier = 1;
        FrameCompleted = true;
        F53Updated = false;
        PrevF53Updated = false;

        OpCode = 0;
        Prefix = OpCodePrefix.None;
        EiBacklog = 0;
        RetExecuted = false;
    }

    /// <summary>
    /// Reset the flag that indicates the machine frame completion.
    /// </summary>
    public void ResetFrameCompletedFlag()
    {
        FrameCompleted = false;
    }

    /// <summary>
    /// Call this method to execute a CPU instruction cycle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// First, it tests if any of the CPU signals is active, using this order: RESET, NMI, INT.
    /// </para>
    /// <param>
    /// Then, after processing the signal, the CPU executes the next instruction. When the instruction is completed,
    /// the method returns.
    /// </param>
    /// <para>
    /// To emulate that the CPU continuously runs, you must call this method in a loop. The execution cycle modifies
    /// the CPU state and (through the memory and I/O port operations) the state of the emulated machine.
    /// </para>
    /// </remarks>
    public void ExecuteCpuCycle()
    {
        // --- Modify the EI interrupt backlog value
        if (EiBacklog > 0)
        {
            EiBacklog--;
        }

        // --- Test CPU signals
        if (SignalFlags != Z80Signals.None)
        {
            // --- The CPU senses the RESET signal in any phase of the instruction execution
            if ((SignalFlags & Z80Signals.Reset) != 0)
            {
                // --- RESET is active. Process it and then inactivate the signal
                Reset();
                SignalFlags &= ~Z80Signals.Reset;
            }
            // --- The CPU does not test the NMI signal while an instruction is being executed
            else if ((SignalFlags & Z80Signals.Nmi) != 0 && Prefix == OpCodePrefix.None)
            {
                // --- NMI is active. Process the non-maskable interrupt
                ProcessNmi();
            }
            // --- The CPU does not test the INT signal while an instruction is being executed
            else if ((SignalFlags & Z80Signals.Int) != 0 && Prefix == OpCodePrefix.None)
            {
                // --- NMI is active. Check, if the interrupt is enabled
                if (Iff1 && EiBacklog == 0)
                {
                    // --- Yes, INT is enabled, and the CPU has already executed the first instruction after EI.
                    ProcessInt();
                }
            }
        }

        // --- Let's handle the halted state.
        if (Halted)
        {
            // --- While in halted state, the CPU does not execute any instructions. It just refreshes the memory
            // --- page pointed by R and waits for four T-states.
            RefreshMemory();
            TactPlus4();
            return;
        }

        // --- The CPU is about to execute the subsequent instruction. First, let's store the previous value of
        // --- F53Updated, as we will use this value in the SCF and CCF instructions.
        PrevF53Updated = F53Updated;

        // --- Second, let's execute the M1 machine cycle that reads the next opcode from the memory.
        OpCode = ReadMemory(Regs.PC);
        Regs.PC++;

        // --- Third, let's refresh the memory by updating the value of Register R. It takes one T-state.
        if (Prefix != OpCodePrefix.DDCB && Prefix != OpCodePrefix.FDCB)
        {
            // --- Indexed bit operation consider the third byte as an address offset, so no memory refresh occurs.
            RefreshMemory();
            TactPlus1();
        }

        // --- It's time to execute the fetched instruction
        switch (Prefix)
        {
            // --- Standard Z80 instructions
            case OpCodePrefix.None:
                switch (OpCode)
                {
                    case 0xcb:
                        Prefix = OpCodePrefix.CB;
                        break;
                    case 0xed:
                        Prefix = OpCodePrefix.ED;
                        break;
                    case 0xdd:
                        Prefix = OpCodePrefix.DD;
                        break;
                    case 0xfd:
                        Prefix = OpCodePrefix.FD;
                        break;
                    default:
                        _standardInstrs![OpCode]?.Invoke();
                        Prefix = OpCodePrefix.None;
                        break;
                }
                break;

            // --- Bit instructions
            case OpCodePrefix.CB:
                _bitInstrs![OpCode]?.Invoke();
                Prefix = OpCodePrefix.None;
                break;

            // --- Extended instructions
            case OpCodePrefix.ED:
                _extendedInstrs![OpCode]?.Invoke();
                Prefix = OpCodePrefix.None;
                break;

            // --- IX- or IY-indexed instructions
            case OpCodePrefix.DD:
            case OpCodePrefix.FD:
                if (OpCode == 0xdd)
                {
                    Prefix = OpCodePrefix.DD;
                }
                else if (OpCode == 0xfd)
                {
                    Prefix = OpCodePrefix.FD;
                }
                else if (OpCode == 0xcb)
                {
                    Prefix = Prefix == OpCodePrefix.DD
                        ? OpCodePrefix.DDCB
                        : OpCodePrefix.FDCB;
                }
                else
                {
                    _indexedInstrs![OpCode]?.Invoke();
                    Prefix = OpCodePrefix.None;
                    break;
                }
                break;

            // --- IX- or IY-indexed bit instructions
            case OpCodePrefix.DDCB:
            case OpCodePrefix.FDCB:
                // --- OpCode is the distance
                Regs.WZ = (ushort)(IndexReg + (sbyte)OpCode);
                OpCode = ReadMemory(Regs.PC);
                TactPlus2(Regs.PC);
                Regs.PC++;
                _indexedBitInstrs![OpCode]?.Invoke();
                Prefix = OpCodePrefix.None;
                break;
        }
    }

    /// <summary>
    /// Remove the CPU from its HALTED state.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveFromHaltedState()
    {
        // --- Remove the CPU from its HALTED state.
        if (Halted)
        {
            Regs.PC++;
            Halted = false;
        }
    }

    /// <summary>
    /// This method processes the active non-maskable interrupt.
    /// </summary>
    private void ProcessNmi()
    {
        // --- Acknowledge the NMI
        TactPlus4();

        RemoveFromHaltedState();

        // --- Update the interrupt flip-flops: The purpose of IFF2 is to save the status of IFF1 when a non-maskable
        // --- interrupt occurs. When a non-maskable interrupt is accepted, IFF1 resets to prevent further interrupts
        // --- until reenabled by the programmer. Therefore, after a non-maskable interrupt is accepted, maskable
        // --- interrupts are disabled, but the previous state of IFF1 is saved so that the complete state of the CPU
        // --- just prior to the non-maskable interrupt can be restored at any time. 
        Iff2 = Iff1;
        Iff1 = false;

        // --- Push the return address to the stack
        PushPC();
        RefreshMemory();

        // --- Carry on the execution at the NMI handler routine address, $0066.
        Regs.PC = 0x0066;
    }

    /// <summary>
    /// This method executes an active and enabled maskable interrupt using the current Interrupt Mode.
    /// </summary>
    private void ProcessInt()
    {
        // --- It takes six T-states to acknowledge the interrupt
        TactPlus6();

        RemoveFromHaltedState();

        // --- Disable the maskable interrupt unless it is enabled again with the EI instruction.
        Iff2 = false;
        Iff2 = false;

        // --- Push the return address to the stack
        PushPC();
        RefreshMemory();

        if (InterruptMode == 2)
        {
            // --- The official Zilog documentation states this:
            // --- "The programmer maintains a table of 16-bit starting addresses for every interrupt service routine.
            // --- This table can be located anywhere in memory. When an interrupt is accepted, a 16-bit pointer must
            // --- be formed to obtain the required interrupt service routine starting address from the table. The
            // --- upper eight bits of this pointer is formed from the contents of the I register. The I register must
            // --- be loaded with the applicable value by the programmer. A CPU reset clears the I register so that it
            // --- is initialized to 0. 
            // --- The lower eight bits of the pointer must be supplied by the interrupting device. Only seven bits are
            // --- required from the interrupting device because the least-significant bit must be a 0.This process is
            // --- required because the pointer must receive two adjacent bytes to form a complete 16 - bit service
            // --- routine starting address; addresses must always start in even locations."
            // --- However, this article shows that we need to reset the least significant bit of:
            // --- http://www.z80.info/interrup2.htm
            var addr = (Regs.I << 8) + 0xff;
            Regs.WL = ReadMemory((ushort)addr++);
            Regs.WH = ReadMemory((ushort)addr);
        }
        else
        {
            // --- On ZX Spectrum, Interrupt Mode 0 and 1 result in the same behavior, as no peripheral device would put
            // --- an instruction on the data bus. In Interrupt Mode 0, the CPU would read a $FF value from the bus, the
            // --- opcode for the RST $38 instruction. In Interrupt Mode 1, the CPU responds to an interrupt by executing
            // --- an RST $38 instruction.
            Regs.WZ = 0x0038;
        }

        // --- Observe that the interrupt handler routine address is first assembled in WZ and moved to PC.
        Regs.PC = Regs.WZ;
    }

    /// <summary>
    /// Calculate the new value of Register F.
    /// </summary>
    /// <remarks>
    /// Seven bits of this 8-bit register are automatically incremented after each instruction fetch. The eighth bit
    /// remains as programmed, resulting from an LD R, A instruction.
    /// </remarks>
    private void RefreshMemory()
    {
        Regs.R = (byte)((Regs.R + 1) & 0x7f | (Regs.R & 0x80));
    }
}