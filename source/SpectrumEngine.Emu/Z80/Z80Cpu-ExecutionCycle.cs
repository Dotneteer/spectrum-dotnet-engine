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
        Tacts = 0;
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
        Tacts = 0;
        F53Updated = false;
        PrevF53Updated = false;

        OpCode = 0;
        Prefix = OpCodePrefix.None;
        EiBacklog = 0;
        RetExecuted = false;
    }

    /// <summary>
    /// Call this method to execute a CPU instruction cycle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// First, it tests if any of the CPU signals is active, using this order: RESET, NMI, INT, HLT.
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
            if ((SignalFlags & Z80Signals.Reset) != 0)
            {
                // --- RESET is active. Process it and then inactivate the signal
                Reset();
                SignalFlags &= ~Z80Signals.Reset;
            }
            else if ((SignalFlags & Z80Signals.Nmi) != 0)
            {
                // --- NMI is active. Process the non-maskable interrupt
                ProcessNmi();
            }
            else if ((SignalFlags & Z80Signals.Int) != 0)
            {
                // --- NMI is active. Check, if the interrupt is enabled
                if (Iff1 && EiBacklog == 0)
                {
                    // --- Yes, INT is enabled, and the CPU has already executed the first instruction after EI.
                    ProcessInt();
                }
            }

            // --- Let's check for the HALTED signal. If any of RESET, NMI, or INT was active and handled, HALTED is
            // --- passive. We could reach this point only if none were handled, including an active but disabled INT.
            if ((SignalFlags & Z80Signals.Halted) != 0)
            {
                ProcessHalted();
                // --- While in HALTED state, the CPU does not execute any instructions.
                return;
            }
        }

        // --- The CPU is about to execute the subsequent instruction. First, let's store the previous value of
        // --- F53Updated, as we will use this value in the SCF and CCF instructions.
        PrevF53Updated = F53Updated;

        // --- Second, let's execute the M1 machine cycle that reads the next opcode from the memory.
        OpCode = ReadMemory(Regs.PC);
        Regs.PC++;

        // --- Third, let's refresh the memory by updating the value of Register R. It takes one T-state.
        RefreshMemory();

        // --- It's time to execute the fetched instruction

    }

    /// <summary>
    /// Reads the specified memory address.
    /// </summary>
    /// <param name="address">Memory address to read</param>
    /// <returns>The byte the CPU has read from the memory</returns>
    /// <remarks>
    /// If the emulated hardware uses any delay when reading the memory, increment the CPU tacts accordingly.
    /// </remarks>
    private byte ReadMemory(ushort address)
    {
        TactPlus3();
        return ReadMemoryFunction(address);
    }

    /// <summary>
    /// Writes a byte to the specfied memory address.
    /// </summary>
    /// <param name="address">Memory address</param>
    /// <param name="data">Data byte to write</param>
    /// <remarks>
    /// If the emulated hardware uses any delay when writing the memory, increment the CPU tacts accordingly.
    /// </remarks>
    private void WriteMemory(ushort address, byte data)
    {
        TactPlus3();
        WriteMemoryFunction(address, data);
    }

    /// <summary>
    /// Reads the specified I/O port.
    /// </summary>
    /// <param name="address">I/O port address to read</param>
    /// <returns>The byte the CPU has read from the I/O port</returns>
    /// <remarks>
    /// The I/O port handler must increase the CPU tacts to emulate the behavior of the concrete hardware. If the
    /// peripheral device does not delay the execution, invoke the TactP4 method to apply the default 4 T-state
    /// I/O delays.
    /// </remarks>
    private byte ReadPort(ushort address)
    {
        TactPlus3();
        return ReadMemoryFunction(address);
    }

    /// <summary>
    /// Writes a byte to the specified I/O port.
    /// </summary>
    /// <param name="address">I/O port address</param>
    /// <param name="data">Data byte to write</param>
    /// <remarks>
    /// The I/O port handler must increase the CPU tacts to emulate the behavior of the concrete hardware. If the
    /// peripheral device does not delay the execution, invoke the TactP4 method to apply the default 4 T-state
    /// I/O delays.
    /// </remarks>
    private void WritePort(ushort address, byte data)
    {
        WriteMemoryFunction(address, data);
    }

    /// <summary>
    /// This method processes the active non-maskable interrupt.
    /// </summary>
    private void ProcessNmi()
    {

    }

    /// <summary>
    /// This method executes an active and enabled maskable interrupt using the current Interrupt Mode.
    /// </summary>
    private void ProcessInt()
    {

    }

    private void ProcessHalted()
    {

    }

    private void RefreshMemory()
    {
        TactPlus1();
    }
}