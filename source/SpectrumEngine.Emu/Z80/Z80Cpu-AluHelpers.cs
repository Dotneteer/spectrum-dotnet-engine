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
    private static byte[] s_8BitIncFlags;

    /// <summary>
    /// Provide a table that contains the value of the F register after an 8-bit DEC operation.
    /// </summary>
    private static byte[] s_8BitDecFlags;

    /// <summary>
    /// Provide a table that contains half-carry flags for add operations
    /// </summary>
    private static byte[] s_HalfCarryAddFlags = new byte[] { 0x00, 0x10, 0x10, 0x10, 0x00, 0x00, 0x00, 0x10 };


    /// <summary>
    /// Initialize the helper tables used for ALU operations.
    /// </summary>
    private void InitializeAluTables()
    {
        // --- Initialize instance tables
        // TODO

        // --- Initializr static tables
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
}