﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition defines a couple of nested types the CPU emulation utilizes in its implementation.
/// </remarks>
public abstract partial class Z80Cpu: IZ80Cpu
{
    /// <summary>
    /// Initializes the Z80 CPU instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The constructor sets the default values of dependency methods. The CPU raises an exception for memory and I/O
    /// port operations if any methods have not been provided by the time they are invoked. The tact handler method is
    /// empty by default.
    /// </para>
    /// <para>
    /// The constructor also sets up methods for optional memory contention and prepares the helper tables used for
    /// instruction execution.
    /// </para>
    /// </remarks>
    protected Z80Cpu()
    {
        // --- Initialize worker tables
        InitializeAluTables();
        InitializeStandardInstructionsTable();
        InitializeBitInstructionsTable();
        InitializeExtendedInstructionsTable();
        InitializeIndexedInstructionsTable();
        InitializeIndexedBitInstructionsTable();
    }

    /// <summary>
    /// Represents integer constants that mask out particular flags of the Z80 CPU's F register.
    /// </summary>
    public sealed class FlagsSetMask
    {
        /// <summary>Sign Flag</summary>
        /// <remarks>
        /// The Sign Flag (S) stores the state of the accumulator's most significant bit (bit 7). When the Z80 CPU
        /// performs arithmetic operations on signed numbers, it uses the binary twos-complement notation to
        /// represent and process numeric information.
        /// </remarks>
        public const byte S = 0x80;

        /// <summary>
        /// Zero Flag
        /// </summary>
        /// <remarks>
        /// The Zero Flag is set (1) or cleared (0) if the result generated by the execution of particular
        /// instructions is 0. For 8-bit arithmetic and logical operations, the Z flag is set to 1 if the resulting
        /// byte in the Accumulator is zero. If the byte is not zero, the Z flag is reset to 0.
        /// </remarks>
        public const byte Z = 0x40;

        /// <summary>Undocumented flag at Bit 5.</summary>
        /// <remarks>
        /// Though there is no explicit semantics behind this flag, numerous Z80 instructions set or reset its
        /// value.
        /// </remarks>
        public const byte R5 = 0x20;

        /// <summary>Half Carry Flag</summary>
        /// <remarks>
        /// The Half Carry Flag (H) is set (1) or cleared (0) depending on the Carry and Borrow status between bits
        /// 3 and 4 of an 8-bit arithmetic operation. This flag is used by the Decimal Adjust Accumulator (DAA)
        /// instruction to correct the result of a packed BCD add or subtract operation.
        /// </remarks>
        public const byte H = 0x10;

        /// <summary>Undocumented flag at Bit 3.</summary>
        /// <remarks>
        /// Though there is no explicit semantics behind this flag, numerous Z80 instructions set or reset its
        /// value.
        /// </remarks>
        public const byte R3 = 0x08;

        /// <summary>Parity/Overflow Flag</summary>
        /// <remarks>
        /// The Parity/Overflow (P/V) Flag is set to a specific state depending on the operation being performed.
        /// For arithmetic operations, this flag indicates an overflow condition when the result in the Accumulator
        /// is greater than the maximum possible number (+127) or is less than the minimum possible number (–128).
        /// This overflow condition is determined by examining the sign bits of the operands.
        /// </remarks>
        public const byte PV = 0x04;

        /// <summary>Add/Subtract Flag</summary>
        /// <remarks>
        /// The Add/Subtract Flag (N) is used by the Decimal Adjust Accumulator 
        /// instruction (DAA) to distinguish between the ADD and SUB instructions.
        /// For ADD instructions, N is cleared to 0. For SUB instructions, N is set to 1.
        /// </remarks>
        public const byte N = 0x02;

        /// <summary>Carry Flag</summary>
        /// <remarks>
        /// The Carry Flag (C) is set or cleared depending on the operation being performed.
        /// </remarks>
        public const byte C = 0x01;

        /// <summary>
        /// Combination of S, Z, and PV
        /// </summary>
        public const byte SZPV = S | Z | PV;

        /// <summary>
        /// Combination of N, and H
        /// </summary>
        public const byte NH = N | H;

        /// <summary>
        /// Combination of R3, and R5
        /// </summary>
        public const byte R3R5 = R3 | R5;
    }

    /// <summary>
    /// This class implements the storage and data access for the registers of the Z80 CPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Z80 CPU contains 208 bits of readable and writable memory available to the programmer. This memory is
    /// configured to eighteen 8-bit registers and four 16-bit registers, and all registers are implemented using
    /// static RAM. The registers include two sets of six general-purpose registers that can be used individually
    /// as 8-bit registers or in pairs as 16-bit registers. There are also two sets of Accumulator and Flag
    /// registers and six special-purpose registers.
    /// </para>
    /// <para>
    /// Besides the registers available for programmers, the Z80 CPU has an internal 16-bit register named WZ in
    /// the architecture diagrams.The Z80 community prefers the MEMPTR(memory pointer) name to WZ.Be aware this
    /// register cannot be accessed programmatically.
    /// </para>
    /// <para>
    /// This structure uses a flat layout to store and access registers. The layout defines the byte offset of each
    /// particular register. This implementation provides excellent performance handling 16-bit register pairs and
    /// their 8-bit constituent registers.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public class Registers
    {
        // ====================================================================
        // 16-bit register access
        // ====================================================================

        // --------------------------------------------------------------------
        // --- Main register set
        // --------------------------------------------------------------------

        [FieldOffset(0)]
        public ushort AF;
        [FieldOffset(2)]
        public ushort BC;
        [FieldOffset(4)]
        public ushort DE;
        [FieldOffset(6)]
        public ushort HL;

        // --- Alternate register set
        [FieldOffset(8)]
        public ushort _AF_;
        [FieldOffset(10)]
        public ushort _BC_;
        [FieldOffset(12)]
        public ushort _DE_;
        [FieldOffset(14)]
        public ushort _HL_;

        // --------------------------------------------------------------------
        // --- Special purpose registers
        // --------------------------------------------------------------------

        /// <summary>Index Register (IX)</summary>
        /// <remarks>
        /// The two independent index registers hold a 16-bit base address that is used in indexed addressing modes.
        /// In this mode, an index register is used as a base to point to a region in memory from which data is to
        /// be stored or retrieved. An additional byte is included in indexed instructions to specify a displacement
        /// from this base. This displacement is specified as a two's complement signed integer. This mode of
        /// addressing significantly simplifies many types of programs, especially when tables of data are used.
        /// </remarks>
        [FieldOffset(16)]
        public ushort IX;

        /// <summary>Index Register (IY)</summary>
        [FieldOffset(18)]
        public ushort IY;

        /// <summary>
        /// Interrupt Page Address (I) Register/Memory Refresh (R) Register
        /// </summary>
        [FieldOffset(20)]
        public ushort IR;

        /// <summary>Program Counter (PC)</summary>
        /// <remarks>
        /// The program counter holds the 16-bit address of the current instruction being fetched from memory. The
        /// Program Counter is automatically incremented after its contents are transferred to the address lines. When
        /// a program jump occurs, the new value is automatically placed in the Program Counter, overriding the
        /// incrementer.
        /// </remarks>
        [FieldOffset(22)]
        public ushort PC;

        /// <summary>Stack Pointer (SP)</summary>
        /// <remarks>
        /// The stack pointer holds the 16-bit address of the current top of a stack located anywhere in external
        /// system RAM memory. The external stack memory is organized as a last-in, first-out (LIFO) file. Data can be
        /// pushed onto the stack from specific CPU registers or popped off of the stack to specific CPU registers
        /// through the execution of PUSH and POP instructions. The data popped from the stack is always the most
        /// recent data pushed onto it. The stack allows simple implementation of multiple level interrupts, unlimited
        /// subroutine nesting, and simplification of many types of data manipulation.
        /// </remarks>
        [FieldOffset(24)]
        public ushort SP;

        /// <summary>
        /// Internal register WZ to support 16-bit addressing operations
        /// </summary>
        [FieldOffset(26)]
        public ushort WZ;

        // ====================================================================
        // 8-bit register access
        // ====================================================================

        /// <summary>Accumulator</summary>
        [FieldOffset(1)]
        public byte A;

        /// <summary>Flags</summary>
        [FieldOffset(0)]
        public byte F;

        /// <summary>General purpose register B</summary>
        [FieldOffset(3)]
        public byte B;

        /// <summary>General purpose register C</summary>
        [FieldOffset(2)]
        public byte C;

        /// <summary>General purpose register D</summary>
        [FieldOffset(5)]
        public byte D;

        /// <summary>General purpose register E</summary>
        [FieldOffset(4)]
        public byte E;

        /// <summary>General purpose register H</summary>
        [FieldOffset(7)]
        public byte H;

        /// <summary>General purpose register L</summary>
        [FieldOffset(6)]
        public byte L;

        /// <summary>High 8-bit of IX</summary>
        [FieldOffset(17)]
        public byte XH;

        /// <summary>Low 8-bit of IX</summary>
        [FieldOffset(16)]
        public byte XL;

        /// <summary>High 8-bit of IY</summary>
        [FieldOffset(19)]
        public byte YH;

        /// <summary>High 8-bit of IY</summary>
        [FieldOffset(18)]
        public byte YL;

        /// <summary>Interrupt Page Address (I) Register</summary>
        /// <remarks>
        /// The Z80 CPU can be operated in a mode in which an indirect call to any memory location can be achieved in
        /// response to an interrupt. The I register is used for this purpose and stores the high-order eight bits of
        /// the indirect address, while the interrupting device provides the lower eight bits of the address. This
        /// feature allows interrupt routines to be dynamically located anywhere in memory with minimal access time
        /// to the routine.
        /// </remarks>
        [FieldOffset(21)]
        public byte I;

        /// <summary>
        /// Memory Refresh (R) Register
        /// </summary>
        /// <remarks>
        /// The Z80 CPU contains a memory refresh counter, enabling dynamic memories to be used with the same ease as
        /// static memories. Seven bits of this 8-bit register are automatically incremented after each instruction
        /// fetch. The eighth bit remains as programmed, resulting from an LD R, A instruction. The data in the refresh
        /// counter is sent out on the lower portion of the address bus along with a refresh control signal while the
        /// CPU is decoding and executing the fetched instruction. This mode of refresh is transparent to the programmer
        /// and does not slow the CPU operation. The programmer can load the R register for testing purposes, but this
        /// register usually is not used by the programmer. During refresh, the contents of the I Register are placed on
        /// the upper eight bits of the address bus.
        /// </remarks>
        [FieldOffset(20)]
        public byte R;

        /// <summary>High 8-bit of WZ</summary>
        [FieldOffset(27)]
        public byte WH;

        /// <summary>Low 8-bit of WZ</summary>
        [FieldOffset(26)]
        public byte WL;

        /// <summary>
        /// Sign Flag
        /// </summary>
        public bool IsSFlagSet => (F & FlagsSetMask.S) != 0;

        /// <summary>
        /// Zero Flag
        /// </summary>
        public bool IsZFlagSet => (F & FlagsSetMask.Z) != 0;

        /// <summary>
        /// R5 Flag (Bit 5 of last ALU operation result)
        /// </summary>
        public bool IsR5FlagSet => (F & FlagsSetMask.R5) != 0;

        /// <summary>
        /// Half Carry Flag
        /// </summary>
        public bool IsHFlagSet => (F & FlagsSetMask.H) != 0;

        /// <summary>
        /// R3 Flag (Bit 3 of last ALU operation result)
        /// </summary>
        public bool IsR3FlagSet => (F & FlagsSetMask.R3) != 0;

        /// <summary>
        /// Parity/Overflow Flag
        /// </summary>
        public bool IsPFlagSet => (F & FlagsSetMask.PV) != 0;

        /// <summary>
        /// Add/Subtract Flag
        /// </summary>
        public bool IsNFlagSet => (F & FlagsSetMask.N) != 0;

        /// <summary>
        /// Carry Flag
        /// </summary>
        public bool IsCFlagSet => (F & FlagsSetMask.C) != 0;

        /// <summary>
        /// Exchanges the AF -- AF' register sets
        /// </summary>
        public void ExchangeAfSet()
        {
            Swap(ref AF, ref _AF_);
        }

        /// <summary>
        /// Exchanges the BC -- BC', DE -- DE', HL -- HL' register sets
        /// </summary>
        public void ExchangeRegisterSet()
        {
            Swap(ref BC, ref _BC_);
            Swap(ref DE, ref _DE_);
            Swap(ref HL, ref _HL_);
        }

        /// <summary>
        /// Swaps the content of specified registers
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="alt"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(ref ushort orig, ref ushort alt)
        {
            (orig, alt) = (alt, orig);
        }
    }

    /// <summary>
    /// This enum represents a set of signals we use when emulating the Z80 CPU. The enum combines independent flags,
    /// each representing a particular signal. The value 0 stands for the state when the CPU does not have any active
    /// signal. Observe, the CPU can have multiple active signals at a time.
    /// </summary>
    [Flags]
    public enum Z80Signals
    {
        /// <summary>
        /// No signal is set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates an active INT (Maskable Interrupt) signal
        /// </summary>
        Int = 0x01,

        /// <summary>
        /// Indicates an active NMI (Non-Maskable Interrupt) signal
        /// </summary>
        Nmi = 0x02,

        /// <summary>
        /// Indicates an active RESET signal
        /// </summary>
        Reset = 0x04,
    }

    /// <summary>
    /// The Z80 CPU uses multi-byte operations. The values of this enum indicate the prefix of an executable opcode.
    /// </summary>
    public enum OpCodePrefix
    {
        /// <summary>
        /// The opcode does not have a prefix (standard instruction)
        /// </summary>
        None,

        /// <summary>
        /// The opcode has an ED prefix (extended instruction)
        /// </summary>
        ED,

        /// <summary>
        /// The opcode has a CB prefix (bit instruction)
        /// </summary>
        CB,

        /// <summary>
        /// The opcode has a DD prefix (IX-indexed instruction)
        /// </summary>
        DD,

        /// <summary>
        /// The opcode has an FD prefix (IY-indexed instruction)
        /// </summary>
        FD,

        /// <summary>
        /// The opcode has DD and CB prefixes (IX-indexed bit instruction)
        /// </summary>
        DDCB,

        /// <summary>
        /// The opcode has FD and CB prefixes (IY-indexed bit instruction)
        /// </summary>
        FDCB,
    }
}
