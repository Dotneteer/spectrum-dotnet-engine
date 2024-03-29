﻿// ReSharper disable InconsistentNaming
namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition contains several helper fields and methods to support the CPU's ALU operations.
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// Take care that the code initializes static ALU helper tables only once.
    /// </summary>
    private static bool s_TablesInitialized;

    /// <summary>
    /// Provide a table that contains the value of the F register after an 8-bit INC operation.
    /// </summary>
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static byte[] s_8BitIncFlags;
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Provide a table that contains the value of the F register after an 8-bit DEC operation.
    /// </summary>
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static byte[] s_8BitDecFlags;
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Provide a table that contains half-carry flags for add operations.
    /// </summary>
    private static readonly byte[] s_HalfCarryAddFlags = new byte[] { 0x00, 0x10, 0x10, 0x10, 0x00, 0x00, 0x00, 0x10 };

    /// <summary>
    /// Provide a table that contains overflow flags for add operations.
    /// </summary>
    private static readonly byte[] s_OverflowAddFlags = new byte[] { 0x00, 0x00, 0x00, 0x04, 0x04, 0x00, 0x00, 0x00 };

    /// <summary>
    /// Provide a table that contains half-carry flags for subtract operations.
    /// </summary>
    private static readonly byte[] s_HalfCarrySubFlags = new byte[] { 0x00, 0x00, 0x10, 0x00, 0x10, 0x00, 0x10, 0x10 };

    /// <summary>
    /// Provide a table that contains overflow flags for subtract operations.
    /// </summary>
    private static readonly byte[] s_OverflowSubFlags = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00 };

    /// <summary>
    /// Provide a table that contains P/V flag values for each byte in the 0x00-0xff range.
    /// </summary>
    private static byte[]? s_ParityTable;

    /// <summary>
    /// Provide a table that masks out the S, Z, 5, and 3 flags from a byte.
    /// </summary>
    private static byte[]? s_SZ53Table;

    /// <summary>
    /// Provide a table that masks out the S, Z, 5, 3, and PV flags from a byte.
    /// </summary>
    private static byte[]? s_SZ53PVTable;

    /// <summary>
    /// Initialize the helper tables used for ALU operations.
    /// </summary>
    private static void InitializeAluTables()
    {
        // --- Initialize static tables
        if (s_TablesInitialized)
        {
            return;
        }

        // --- Prepare 8-bit INC flags
        s_8BitIncFlags = new byte[0x100];
        for (var b = 0; b < 0x100; b++)
        {
            var oldVal = (byte)b;
            var newVal = (byte)(oldVal + 1);
            var flags =
                // --- C is unaffected, we keep it 0 here in this table.
                (newVal & FlagsSetMask.R3) |
                (newVal & FlagsSetMask.R5) |
                ((newVal & 0x80) != 0 ? FlagsSetMask.S : 0) |
                (newVal == 0 ? FlagsSetMask.Z : 0) |
                ((oldVal & 0x0F) == 0x0F ? FlagsSetMask.H : 0) |
                (oldVal == 0x7F ? FlagsSetMask.PV : 0);
                // --- Observe, N is 0, as this is an increment operation
            s_8BitIncFlags[b] = (byte)flags;
        }

        // --- Prepare 8-bit DEC flags
        s_8BitDecFlags = new byte[0x100];
        for (var b = 0; b < 0x100; b++)
        {
            var oldVal = (byte)b;
            var newVal = (byte)(oldVal - 1);
            var flags =
                // --- C is unaffected, we keep it 0 here in this table.
                (newVal & FlagsSetMask.R3) |
                (newVal & FlagsSetMask.R5) |
                ((newVal & 0x80) != 0 ? FlagsSetMask.S : 0) |
                (newVal == 0 ? FlagsSetMask.Z : 0) |
                ((oldVal & 0x0F) == 0x00 ? FlagsSetMask.H : 0) |
                (oldVal == 0x80 ? FlagsSetMask.PV : 0) |
                // --- Observe, N is 1, as this is a decrement operation
                FlagsSetMask.N;
            s_8BitDecFlags[b] = (byte)flags;
        }

        // --- Prepare the parity table
        s_ParityTable = new byte[0x100];
        for (var i = 0; i < 0x100; i++)
        {
            var parity = 0;
            var b = i;
            for (var j = 0; j < 8; j++)
            {
                parity ^= (b & 0x01);
                b >>= 1;
            }
            s_ParityTable[i] = (byte)(parity == 0 ? FlagsSetMask.PV : 0);
        }

        // --- Prepare the SZ53 table
        s_SZ53Table = new byte[0x100];
        s_SZ53PVTable = new byte[0x100];
        for (var i = 0; i < 0x100; i++)
        {
            s_SZ53Table[i] = (byte)(i & (FlagsSetMask.S | FlagsSetMask.R5 | FlagsSetMask.R3));
            s_SZ53PVTable[i] = (byte)(s_SZ53Table[i] | s_ParityTable[i]);
        }
        s_SZ53Table[0] |= FlagsSetMask.Z;
        s_SZ53PVTable[0] |= FlagsSetMask.Z;

        s_TablesInitialized = true;
    }

    /// <summary>
    /// Adds the <paramref name="regHl"/> value and <paramref name="regOther"/> value
    /// according to the rule of ADD HL,QQ operation
    /// </summary>
    /// <param name="regHl">HL (IX, IY) value</param>
    /// <param name="regOther">Other value</param>
    /// <returns>Result value</returns>
    private ushort Add16(ushort regHl, ushort regOther)
    {
        var tmpVal = regHl + regOther;
        var lookup =
          ((regHl & 0x0800) >> 11) |
          ((regOther & 0x0800) >> 10) |
          ((tmpVal & 0x0800) >> 9);
        Regs.WZ = (ushort)(regHl + 1);
        Regs.F =(byte)((Regs.SZPVValue) |
          ((tmpVal & 0x10000) != 0 ? FlagsSetMask.C : 0x00) |
          ((tmpVal >> 8) & (FlagsSetMask.R3R5)) |
          s_HalfCarryAddFlags[lookup]);
        F53Updated = true;
        return (ushort)tmpVal;
    }

    /// <summary>
    /// The core of the 8-bit ADD operation 
    /// </summary>
    /// <param name="value">Value to add to A</param>
    private void Add8(byte value)
    {
        var tmp = Regs.A + value;
        var lookup =
            ((Regs.A & 0x88) >> 3) |
            ((value & 0x88) >> 2) |
            ((tmp & 0x88) >> 1);
        Regs.A = (byte)tmp;
        Regs.F = (byte)
          (((tmp & 0x100) != 0 ? FlagsSetMask.C : 0) |
          s_HalfCarryAddFlags[lookup & 0x07] |
          s_OverflowAddFlags[lookup >> 4] |
          s_SZ53Table![Regs.A]);
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit ADC operation 
    /// </summary>
    /// <param name="value">Value to add to A</param>
    private void Adc8(byte value)
    {
        var tmp = Regs.A + value + (Regs.CFlagValue);
        var lookup =
            ((Regs.A & 0x88) >> 3) |
            ((value & 0x88) >> 2) |
            ((tmp & 0x88) >> 1);
        Regs.A = (byte)tmp;
        Regs.F = (byte)
          (((tmp & 0x100) != 0 ? FlagsSetMask.C : 0) |
          s_HalfCarryAddFlags[lookup & 0x07] |
          s_OverflowAddFlags[lookup >> 4] |
          s_SZ53Table![Regs.A]);
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit SUB operation
    /// </summary>
    /// <param name="value">Value to subtract to A</param>
    private void Sub8(byte value)
    {
        var tmp = Regs.A - value;
        var lookup =
          ((Regs.A & 0x88) >> 3) |
          ((value & 0x88) >> 2) |
          ((tmp & 0x88) >> 1);
        Regs.A = (byte)tmp;
        Regs.F =(byte)
          (((tmp & 0x100) != 0 ? FlagsSetMask.C : 0) |
          FlagsSetMask.N |
          s_HalfCarrySubFlags[lookup & 0x07] |
          s_OverflowSubFlags[lookup >> 4] |
          s_SZ53Table![Regs.A]);
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit SBC operation
    /// </summary>
    /// <param name="value">Value to subtract to A</param>
    private void Sbc8(byte value)
    {
        var tmp = Regs.A - value - Regs.CFlagValue;
        var lookup =
          ((Regs.A & 0x88) >> 3) |
          ((value & 0x88) >> 2) |
          ((tmp & 0x88) >> 1);
        Regs.A = (byte)tmp;
        Regs.F = (byte)
          (((tmp & 0x100) != 0 ? FlagsSetMask.C : 0) |
          FlagsSetMask.N |
          s_HalfCarrySubFlags[lookup & 0x07] |
          s_OverflowSubFlags[lookup >> 4] |
          s_SZ53Table![Regs.A]);
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit AND operation 
    /// </summary>
    /// <param name="value">Value to AND with A</param>
    private void And8(byte value)
    {
        Regs.A &= value;
        Regs.F = (byte)(FlagsSetMask.H | s_SZ53PVTable![Regs.A]);
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit XOR operation 
    /// </summary>
    /// <param name="value">Value to XOR with A</param>
    private void Xor8(byte value)
    {
        Regs.A ^= value;
        Regs.F = s_SZ53PVTable![Regs.A];
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit OR operation 
    /// </summary>
    /// <param name="value">Value to OR with A</param>
    private void Or8(byte value)
    {
        Regs.A |= value;
        Regs.F = s_SZ53PVTable![Regs.A];
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit CP operation 
    /// </summary>
    /// <param name="value">Value to compare with A</param>
    private void Cp8(byte value)
    {
        var tmp = Regs.A - value;
        var lookup =
          ((Regs.A & 0x88) >> 3) |
          ((value & 0x88) >> 2) |
          ((tmp & 0x88) >> 1);
        Regs.F = (byte)
          (((tmp & 0x100) != 0 ? FlagsSetMask.C : 0) |
          (tmp != 0 ? 0 : FlagsSetMask.Z) |
          FlagsSetMask.N |
          s_HalfCarrySubFlags[lookup & 0x07] |
          s_OverflowSubFlags[lookup >> 4] |
          (value & FlagsSetMask.R3R5) |
          (tmp & FlagsSetMask.S));
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit RLC operation.
    /// </summary>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private byte Rlc8(byte oper)
    {
        byte result = (byte)((oper << 1) | (oper >> 7));
        Regs.F = (byte)((result & FlagsSetMask.C) | s_SZ53PVTable![result]);
        F53Updated = true;
        return result;
    }

    /// <summary>
    /// The core of the 8-bit RRC operation.
    /// </summary>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private byte Rrc8(byte oper)
    {
        Regs.F = (byte)(oper & FlagsSetMask.C);
        byte result = (byte)((oper >> 1) | (oper << 7));
        Regs.F |= s_SZ53PVTable![result];
        F53Updated = true;
        return result;
    }

    /// <summary>
    /// The core of the 8-bit RL operation.
    /// </summary>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private byte Rl8(byte oper)
    {
        byte result = (byte)((oper << 1) | Regs.CFlagValue);
        Regs.F = (byte)((oper >> 7) | s_SZ53PVTable![result]);
        F53Updated = true;
        return result;
    }

    /// <summary>
    /// The core of the 8-bit RR operation.
    /// </summary>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private byte Rr8(byte oper)
    {
        var result = (byte)((oper >> 1) | (Regs.F << 7));
        Regs.F = (byte)((oper & FlagsSetMask.C) | s_SZ53PVTable![result]);
        F53Updated = true;
        return result;
    }

    /// <summary>
    /// The core of the 8-bit SLA operation.
    /// </summary>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private byte Sla8(byte oper)
    {
        Regs.F = (byte)(oper >> 7);
        var result = (byte)(oper << 1);
        Regs.F |= s_SZ53PVTable![result];
        F53Updated = true;
        return result;
    }

    /// <summary>
    /// The core of the 8-bit SLA operation.
    /// </summary>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private byte Sra8(byte oper)
    {
        Regs.F = (byte)(oper & FlagsSetMask.C);
        byte result = (byte)((oper & 0x80) | (oper >> 1));
        Regs.F |= s_SZ53PVTable![result];
        F53Updated = true;
        return result;
    }

    /// <summary>
    /// The core of the 8-bit SLL operation.
    /// </summary>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private byte Sll8(byte oper)
    {
        Regs.F = (byte)(oper >> 7);
        var result = (byte)((oper << 1) | 0x01);
        Regs.F |= s_SZ53PVTable![result];
        F53Updated = true;
        return result;
    }

    /// <summary>
    /// The core of the 8-bit SRL operation.
    /// </summary>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private byte Srl8(byte oper)
    {
        Regs.F = (byte)(oper & FlagsSetMask.C);
        byte result = (byte)(oper >> 1);
        Regs.F |= s_SZ53PVTable![result];
        F53Updated = true;
        return result;
    }

    /// <summary>
    /// The core of the 8-bit BIT operation.
    /// </summary>
    /// <param name="bit">Bit index (0-7)</param>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private void Bit8(int bit, byte oper)
    {
        Regs.F = (byte)(Regs.CFlagValue | FlagsSetMask.H | (oper & FlagsSetMask.R3R5));
        var bitVal = oper & (0x01 << bit);
        if (bitVal == 0)
        {
            Regs.F |= FlagsSetMask.PV | FlagsSetMask.Z;
        }
        Regs.F |= (byte)(bitVal & FlagsSetMask.S);
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 8-bit BIT operation with WZ.
    /// </summary>
    /// <param name="bit">Bit index (0-7)</param>
    /// <param name="oper">Operand</param>
    /// <returns>Operation result</returns>
    private void Bit8W(int bit, byte oper)
    {
        Regs.F = (byte)(Regs.CFlagValue | FlagsSetMask.H | (Regs.WH & FlagsSetMask.R3R5));
        var bitVal = oper & (0x01 << bit);
        if (bitVal == 0)
        {
            Regs.F |= FlagsSetMask.PV | FlagsSetMask.Z;
        }
        Regs.F |= (byte)(bitVal & FlagsSetMask.S);
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 16-bit SBC operation.
    /// </summary>
    /// <param name="value">Value to subtract from HL</param>
    private void Sbc16(ushort value)
    {
        int tmpVal = Regs.HL - value - Regs.CFlagValue;
        var lookup =
          ((Regs.HL & 0x8800) >> 11) |
          ((value & 0x8800) >> 10) |
          ((tmpVal & 0x8800) >> 9);
        Regs.WZ = (ushort)(Regs.HL + 1);
        Regs.HL = (ushort)tmpVal;
        Regs.F = (byte)
          (((tmpVal & 0x10000) != 0 ? FlagsSetMask.C : 0) |
          FlagsSetMask.N |
          s_OverflowSubFlags[lookup >> 4] |
          (Regs.H & (FlagsSetMask.R3R5 | FlagsSetMask.S)) |
          s_HalfCarrySubFlags[lookup & 0x07] |
          (Regs.HL != 0 ? 0 : FlagsSetMask.Z));
        F53Updated = true;
    }

    /// <summary>
    /// The core of the 16-bit ADC operation.
    /// </summary>
    /// <param name="value">Value to add to HL</param>
    private void Adc16(ushort value)
    {
        var tmpVal = Regs.HL + value + Regs.CFlagValue;
        var lookup =
          ((Regs.HL & 0x8800) >> 11) |
          ((value & 0x8800) >> 10) |
          ((tmpVal & 0x8800) >> 9);
        Regs.WZ = (ushort)(Regs.HL + 1);
        Regs.HL = (ushort)tmpVal;
        Regs.F = (byte)
          (((tmpVal & 0x10000) != 0 ? FlagsSetMask.C : 0) |
          s_OverflowAddFlags[lookup >> 4] |
          (Regs.H & (FlagsSetMask.R3R5 | FlagsSetMask.S)) |
          s_HalfCarryAddFlags[lookup & 0x07] |
          (Regs.HL != 0 ? 0 : FlagsSetMask.Z));
        F53Updated = true;
    }
}