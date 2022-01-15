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
    private static bool s_TablesInitialized = false;

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
    /// Initialize the helper tables used for ALU operations.
    /// </summary>
    private void InitializeAluTables()
    {
        // --- Initialize instance tables
        // TODO

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
        for (var i = 0; i < 0x100; i++)
        {
            s_SZ53Table[i] = (byte)(i & (FlagsSetMask.S | FlagsSetMask.R5 | FlagsSetMask.R3));
        }
        s_SZ53Table[0] |= FlagsSetMask.Z;
    }

    /// <summary>
    /// Adds the <paramref name="regHL"/> value and <paramref name="regOther"/> value
    /// according to the rule of ADD HL,QQ operation
    /// </summary>
    /// <param name="regHL">HL (IX, IY) value</param>
    /// <param name="regOther">Other value</param>
    /// <returns>Result value</returns>
    private ushort AluAddHL(ushort regHL, ushort regOther)
    {
        var tmpVal = regHL + regOther;
        var lookup =
          ((regHL & 0x0800) >> 11) |
          ((regOther & 0x0800) >> 10) |
          ((tmpVal & 0x0800) >> 9);
        Regs.WZ = (ushort)(regHL + 1);
        Regs.F =(byte)((Regs.F & FlagsSetMask.SZPV) |
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

    // 
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
}