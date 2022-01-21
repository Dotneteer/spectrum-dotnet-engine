﻿namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition contains the code for processing IX- or IY-indexed bit manipulation instructions (with `$DDCB` or
/// `$FDCB` prefix).
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// This array contains the 256 function references, each executing a  standard Z80 instruction.
    /// </summary>
    private Action[]? _indexedBitInstrs;

    /// <summary>
    /// Initialize the table of standard instructions.
    /// </summary>
    private void InitializeIndexedBitInstructionsTable()
    {
        _indexedBitInstrs = new Action[]
        {
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 00-07
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 08-0f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 10-17
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 18-1f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 20-27
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 28-2f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 30-37
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 38-3f

            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 40-47
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 48-4f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 50-57
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 58-5f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 60-67
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 68-6f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 70-77
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 78-7f

            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 80-87
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 88-8f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 90-97
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 98-9f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // a0-a7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // a8-af
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // b0-b7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // b8-bf

            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // c0-c7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // c8-cf
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // d0-d7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // d8-df
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // e0-e7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // e8-ef
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // f0-f7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // f8-ff
        };
    }
}