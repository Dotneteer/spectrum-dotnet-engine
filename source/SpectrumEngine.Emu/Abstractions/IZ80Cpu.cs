﻿namespace SpectrumEngine.Emu;

/// <summary>
/// This interface represents the behavior and state of the Z80 CPU that is available from outside by other components.
/// </summary>
public interface IZ80Cpu
{
    /// <summary>
    /// The registers of the CPU
    /// </summary>
    Z80Cpu.Registers Regs { get; }

    /// <summary>
    /// The current state of the Z80 signal flags
    /// </summary>
    Z80Cpu.Z80Signals SignalFlags { get; set; }

    /// <summary>
    /// The current maskable interrupt mode (0, 1, or 2)
    /// </summary>
    int InterruptMode { get; }

    /// <summary>
    /// Interrupt Enable Flip-Flop
    /// </summary>
    bool Iff1 { get; set; }

    /// <summary>
    /// Temporary storage for Iff1.
    /// </summary>
    bool Iff2 { get; set; }
    
    /// <summary>
    /// This flag indicates if the CPU is in a halted state.
    /// </summary>
    bool Halted { get; }

    /// <summary>
    /// Get the base clock frequency of the CPU. We use this value to calculate the machine frame rate.
    /// </summary>
    int BaseClockFrequency { get; }

    /// <summary>
    /// This property gets or sets the value of the current clock multiplier.
    /// </summary>
    /// <remarks>
    /// By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock
    /// frequency multiplier to emulate a faster CPU.
    /// </remarks>
    int ClockMultiplier { get; set; }

    /// <summary>
    /// The number of T-states (clock cycles) elapsed since the last reset
    /// </summary>
    ulong Tacts { get; }

    /// <summary>
    /// Show the number of machine frames completed since the CPU started.
    /// </summary>
    int Frames { get; }

    /// <summary>
    /// Get the current frame tact within the machine frame being executed.
    /// </summary>
    int CurrentFrameTact { get; }

    /// <summary>
    /// Get the number of T-states in a machine frame.
    /// </summary>
    int TactsInFrame { get; }
    
    /// <summary>
    /// Get the number of T-states in a display line (use -1, if this info is not available)
    /// </summary>
    int TactsInDisplayLine { get; }

    /// <summary>
    /// Set the number of tacts in a machine frame.
    /// </summary>
    /// <param name="tacts">Number of tacts in a machine frame</param>
    void SetTactsInFrame(int tacts);

    /// <summary>
    /// This flag indicates if bit 3 or 5 of Register F has been updated. We need to keep this value, as we utilize
    /// it within the `SCF` and `CCF` instructions to calculate the new values of bit 3 and 5 of F.
    /// </summary>
    bool F53Updated { get; }

    /// <summary>
    /// When calculating the value of bit 3 and 5 of Register F within the `SCF` and `CCF` instructions, we must know
    /// whether the last executed instruction has updated these flags. This field stores this information.
    /// </summary>
    bool PrevF53Updated { get; }

    /// <summary>
    /// The last fetched opcode. If an instruction is prefixed, it contains the prefix or the opcode following the
    /// prefix, depending on which was fetched last.
    /// </summary>
    byte OpCode { get; }

    /// <summary>
    /// The current prefix to consider when processing the subsequent opcode.
    /// </summary>
    Z80Cpu.OpCodePrefix Prefix { get; }

    /// <summary>
    /// We use this variable to handle the EI instruction properly.
    /// </summary>
    int EiBacklog { get; }

    /// <summary>
    /// We need this flag to implement the step-over debugger function that continues the execution and stops when the
    /// current subroutine returns to its caller. The debugger will observe the change of this flag and manage its
    /// internal tracking of the call stack accordingly.
    /// </summary>
    bool RetExecuted { get; }

    /// <summary>
    /// This flag is reserved for future extension. The ZX Spectrum Next computer uses additional Z80 instructions.
    /// This flag indicates if those are allowed.
    /// </summary>
    bool AllowExtendedInstructions { get; set; }

    /// <summary>
    /// Accumulates the total contention value since the last start
    /// </summary>
    int TotalContentionDelaySinceStart { get; set; }

    /// <summary>
    /// Accumulates the contention since the last pause
    /// </summary>
    int ContentionDelaySincePause { get; set; }

    /// <summary>
    /// Executes a hard reset as if the machine and the CPU had just been turned on.
    /// </summary>
    void HardReset();

    /// <summary>
    /// Handles the active RESET signal of the CPU.
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Checks if the next instruction to be executed is a call instruction or not
    /// </summary>
    /// <returns>
    /// 0, if the next instruction is not a call; otherwise the length of the call instruction
    /// </returns>
    int GetCallInstructionLength();

    /// <summary>
    /// Call this method to execute a CPU instruction cycle.
    /// </summary>
    void ExecuteCpuCycle();

    /// <summary>
    /// Read the byte at the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <returns>The byte read from the memory</returns>
    byte DoReadMemory(ushort address);

    /// <summary>
    /// This function implements the memory read delay of the CPU.
    /// </summary>
    /// <param name="address">Memory address to read</param>
    /// <remarks>
    /// Normally, it is exactly 3 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 3-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 3 T-states!
    /// </remarks>
    void DelayMemoryRead(ushort address) => TactPlus3();

    /// <summary>
    /// Write the given byte to the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    void DoWriteMemory(ushort address, byte value);

    /// <summary>
    /// This function implements the memory write delay of the CPU.
    /// </summary>
    /// <param name="address">Memory address to write</param>
    /// <remarks>
    /// Normally, it is exactly 3 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 3-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 3 T-states!
    /// </remarks>
    void DelayMemoryWrite(ushort address) => TactPlus3();

    /// <summary>
    /// This function handles address-based memory read contention.
    /// </summary>
    void DelayAddressBusAccess(ushort address);

    /// <summary>
    /// This function reads a byte (8-bit) from an I/O port using the provided 16-bit address.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port read operation.
    /// </remarks>
    public abstract byte DoReadPort(ushort address);

    /// <summary>
    /// This function implements the I/O port read delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    void DelayPortRead(ushort address);

    /// <summary>
    /// This function writes a byte (8-bit) to the 16-bit I/O port address provided in the first argument.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port write operation.
    /// </remarks>
    void DoWritePort(ushort address, byte value);

    /// <summary>
    /// This function implements the I/O port write delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    void DelayPortWrite(ushort address);

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    /// <param name="increment">The tact increment value</param>
    /// <remarks>
    /// With this function, you can emulate hardware activities running simultaneously with the CPU. For example,
    /// rendering the screen or sound,  handling peripheral devices, and so on.
    /// </remarks>
    void OnTactIncremented(int increment);

    /// <summary>
    /// This method increments the current CPU tacts by one.
    /// </summary>
    void TactPlus1();

    /// <summary>
    /// This method increments the current CPU tacts by three.
    /// </summary>
    void TactPlus3();

    /// <summary>
    /// This method increments the current CPU tacts by four.
    /// </summary>
    void TactPlus4();

    /// <summary>
    /// This method increments the current CPU tacts by N.
    /// </summary>
    /// <param name="n">The number to increate the CPU tacts by</param>
    void TactPlusN(byte n);
}