namespace SpectrumEngine.Emu;

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
            XRlcB,      XRlcC,      XRlcD,      XRlcE,      XRlcH,      XRlcL,      XRlc,       XRlcA,      // 00-07
            XRrcB,      XRrcC,      XRrcD,      XRrcE,      XRrcH,      XRrcL,      XRrc,       XRrcA,      // 08-0f
            XRlB,       XRlC,       XRlD,       XRlE,       XRlH,       XRlL,       XRl,        XRlA,       // 10-17
            XRrB,       XRrC,       XRrD,       XRrE,       XRrH,       XRrL,       XRr,        XRrA,       // 18-1f
            XSlaB,      XSlaC,      XSlaD,      XSlaE,      XSlaH,      XSlaL,      XSla,       XSlaA,      // 20-27
            XSraB,      XSraC,      XSraD,      XSraE,      XSraH,      XSraL,      XSra,       XSraA,      // 28-2f
            XSllB,      XSllC,      XSllD,      XSllE,      XSllH,      XSllL,      XSll,       XSllA,      // 30-37
            XSrlB,      XSrlC,      XSrlD,      XSrlE,      XSrlH,      XSrlL,      XSrl,       XSrlA,      // 38-3f

            XBit0,      XBit0,      XBit0,      XBit0,      XBit0,      XBit0,      XBit0,      XBit0,      // 40-47
            XBit1,      XBit1,      XBit1,      XBit1,      XBit1,      XBit1,      XBit1,      XBit1,      // 48-4f
            XBit2,      XBit2,      XBit2,      XBit2,      XBit2,      XBit2,      XBit2,      XBit2,      // 50-57
            XBit3,      XBit3,      XBit3,      XBit3,      XBit3,      XBit3,      XBit3,      XBit3,      // 58-5f
            XBit4,      XBit4,      XBit4,      XBit4,      XBit4,      XBit4,      XBit4,      XBit4,      // 60-67
            XBit5,      XBit5,      XBit5,      XBit5,      XBit5,      XBit5,      XBit5,      XBit5,      // 68-6f
            XBit6,      XBit6,      XBit6,      XBit6,      XBit6,      XBit6,      XBit6,      XBit6,      // 70-77
            XBit7,      XBit7,      XBit7,      XBit7,      XBit7,      XBit7,      XBit7,      XBit7,      // 78-7f

            XRes0B,     XRes0C,     XRes0D,     XRes0E,     XRes0H,     XRes0L,     XRes0,      XRes0A,     // 80-87
            XRes1B,     XRes1C,     XRes1D,     XRes1E,     XRes1H,     XRes1L,     XRes1,      XRes1A,     // 88-8f
            XRes2B,     XRes2C,     XRes2D,     XRes2E,     XRes2H,     XRes2L,     XRes2,      XRes2A,     // 90-97
            XRes3B,     XRes3C,     XRes3D,     XRes3E,     XRes3H,     XRes3L,     XRes3,      XRes3A,     // 98-9f
            XRes4B,     XRes4C,     XRes4D,     XRes4E,     XRes4H,     XRes4L,     XRes4,      XRes4A,     // a0-a7
            XRes5B,     XRes5C,     XRes5D,     XRes5E,     XRes5H,     XRes5L,     XRes5,      XRes5A,     // a8-af
            XRes6B,     XRes6C,     XRes6D,     XRes6E,     XRes6H,     XRes6L,     XRes6,      XRes6A,     // b0-b7
            XRes7B,     XRes7C,     XRes7D,     XRes7E,     XRes7H,     XRes7L,     XRes7,      XRes7A,     // b8-bf

            XSet0B,     XSet0C,     XSet0D,     XSet0E,     XSet0H,     XSet0L,     XSet0,      XSet0A,     // c0-c7
            XSet1B,     XSet1C,     XSet1D,     XSet1E,     XSet1H,     XSet1L,     XSet1,      XSet1A,     // c8-cf
            XSet2B,     XSet2C,     XSet2D,     XSet2E,     XSet2H,     XSet2L,     XSet2,      XSet2A,     // d0-d7
            XSet3B,     XSet3C,     XSet3D,     XSet3E,     XSet3H,     XSet3L,     XSet3,      XSet3A,     // d8-df
            XSet4B,     XSet4C,     XSet4D,     XSet4E,     XSet4H,     XSet4L,     XSet4,      XSet4A,     // e0-e7
            XSet5B,     XSet5C,     XSet5D,     XSet5E,     XSet5H,     XSet5L,     XSet5,      XSet5A,     // e8-ef
            XSet6B,     XSet6C,     XSet6D,     XSet6E,     XSet6H,     XSet6L,     XSet6,      XSet6A,     // f0-f7
            XSet7B,     XSet7C,     XSet7D,     XSet7E,     XSet7H,     XSet7L,     XSet7,      XSet7A,     // f8-ff
        };
    }

    /// <summary>
    /// "rlc (ix + D),Q" operation (0x00-0x07)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are rotated left 1 bit position. The contents of bit 7 are copied
    /// to the Carry flag and also to bit 0. The result is stored in register Q
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
     
    // 0x00
    private void XRlcB()
    {
        Regs.B = Rlc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x01
    private void XRlcC()
    {
        Regs.C = Rlc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x02
    private void XRlcD()
    {
        Regs.D = Rlc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x03
    private void XRlcE()
    {
        Regs.E = Rlc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x04
    private void XRlcH()
    {
        Regs.H = Rlc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x05
    private void XRlcL()
    {
        Regs.L = Rlc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "rlc (ix + D)" operation (0x06)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are rotated left 1 bit position. The contents of bit 7 are copied to
    /// the Carry flag and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    private void XRlc()
    {
        var tmp = Rlc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x07
    private void XRlcA()
    {
        Regs.A = Rlc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "rrc (ix + D),Q" operation (0x08-0x0f)
    /// </summary>
    /// <param name="addr">Indexed address</param>
    /// <remarks>
    /// The contents of the indexed memory address are rotated right 1 bit position. The contents of bit 0 are copied
    /// to the Carry flag and also to bit 7. The result is stored in register Q.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the source byte.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    
    // 0x08
    private void XRrcB()
    {
        Regs.B = Rrc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x09
    private void XRrcC()
    {
        Regs.C = Rrc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x0A
    private void XRrcD()
    {
        Regs.D = Rrc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x0B
    private void XRrcE()
    {
        Regs.E = Rrc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x0C
    private void XRrcH()
    {
        Regs.H = Rrc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x0D
    private void XRrcL()
    {
        Regs.L = Rrc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "rrc (ix + D)" operation (0x0E)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are rotated right 1 bit position. The contents of bit 0 are copied
    /// to the Carry flag and also to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the source byte.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    private void XRrc()
    {
        var tmp = Rrc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x0F
    private void XRrcA()
    {
        Regs.A = Rrc8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "rl (ix + D),Q" operation (0x10-0x17)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are rotated left 1 bit position. The contents of bit 7 are copied
    /// to the Carry flag, and the previous contents of the Carry flag are copied to bit 0. The result is stored in
    /// register Q.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    
    // 0x10
    private void XRlB()
    {
        Regs.B = Rl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x11
    private void XRlC()
    {
        Regs.C = Rl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x12
    private void XRlD()
    {
        Regs.D = Rl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x13
    private void XRlE()
    {
        Regs.E = Rl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x14
    private void XRlH()
    {
        Regs.H = Rl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x15
    private void XRlL()
    {
        Regs.L = Rl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "rl (ix + D),Q" operation (0x16)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are rotated left 1 bit position. The contents of bit 7 are copied
    /// to the Carry flag, and the previous contents of the Carry flag are copied to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    private void XRl()
    {
        var tmp = Rl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x17
    private void XRlA()
    {
        Regs.A = Rl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "rr (ix + D),Q" operation (0x18)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are rotated right 1 bit position. The contents of bit 0 are copied
    /// to the Carry flag, and the previous contents of the Carry flag are copied to bit 7. The result is stored in
    /// register Q.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the source byte.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>

    // 0x18
    private void XRrB()
    {
        Regs.B = Rr8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x19
    private void XRrC()
    {
        Regs.C = Rr8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x1A
    private void XRrD()
    {
        Regs.D = Rr8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x1B
    private void XRrE()
    {
        Regs.E = Rr8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x1C
    private void XRrH()
    {
        Regs.H = Rr8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x1D
    private void XRrL()
    {
        Regs.L = Rr8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "rr (ix + D)" operation (0x1E)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are rotated right 1 bit position. The contents of bit 0 are copied
    /// to the Carry flag, and the previous contents of the Carry flag are copied to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the source byte.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    private void XRr()
    {
        var tmp = Rr8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x1F
    private void XRrA()
    {
        Regs.A = Rr8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "sla (ix + D),Q" operation (0x20-0x27)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of the indexed memory address. The
    /// contents of bit 7 are copied to the Carry flag. The result is stored in register Q.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>

    // 0x20
    private void XSlaB()
    {
        Regs.B = Sla8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x21
    private void XSlaC()
    {
        Regs.C = Sla8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x22
    private void XSlaD()
    {
        Regs.D = Sla8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x23
    private void XSlaE()
    {
        Regs.E = Sla8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x24
    private void XSlaH()
    {
        Regs.H = Sla8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x25
    private void XSlaL()
    {
        Regs.L = Sla8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "sla (ix + D)" operation (0x26)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of the indexed memory address. The
    /// contents of bit 7 are copied to the Carry flag.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// =================================
    /// | 1 | 1 | X | 1 | 1 | 1 | 0 | 1 | DD/FD prefix
    /// =================================
    /// | 1 | 1 | 0 | 0 | 1 | 0 | 1 | 1 | CB prefix
    /// =================================
    /// | 0 | 0 | 1 | 0 | 0 | 1 | 1 | 0 |
    /// =================================
    /// T-States: 4, 4, 3, 5, 4, 3 (23)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    private void XSla()
    {
        var tmp = Sla8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x27
    private void XSlaA()
    {
        Regs.A = Sla8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "sra (ix + D),Q" operation (0x28-0x2f)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of the indexed memory address. The
    /// contents of bit 0 are copied to the Carry flag and the previous contents of bit 7 remain unchanged. The
    /// result is stored in register Q.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>

    // 0x28
    private void XSraB()
    {
        Regs.B = Sra8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x29
    private void XSraC()
    {
        Regs.C = Sra8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x2A
    private void XSraD()
    {
        Regs.D = Sra8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x2B
    private void XSraE()
    {
        Regs.E = Sra8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x2C
    private void XSraH()
    {
        Regs.H = Sra8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x2D
    private void XSraL()
    {
        Regs.L = Sra8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "sra (ix + D)" operation (0x2E)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of the indexed memory address. The
    /// contents of bit 0 are copied to the Carry flag and the previous contents of bit 7 remain unchanged.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    private void XSra()
    {
        var tmp = Sra8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x2F
    private void XSraA()
    {
        Regs.A = Sra8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "sll (ix + D),Q" operation (0x30-0x37)
    /// </summary>
    /// <remarks>
    /// A logic shift left 1 bit position is performed on the contents of the indexed memory address. The contents of
    /// bit 7 are copied to the Carry flag and bit 0 is set. The result is stored in register Q.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>

    // 0x30
    private void XSllB()
    {
        Regs.B = Sll8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x31
    private void XSllC()
    {
        Regs.C = Sll8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x32
    private void XSllD()
    {
        Regs.D = Sll8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x33
    private void XSllE()
    {
        Regs.E = Sll8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x34
    private void XSllH()
    {
        Regs.H = Sll8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x35
    private void XSllL()
    {
        Regs.L = Sll8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "sll (IDR + D)" operation (0x36)
    /// </summary>
    /// <remarks>
    /// A logic shift left 1 bit position is performed on the contents of the indexed memory address. The contents of
    /// bit 7 are copied to the Carry flag and bit 0 is set.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    private void XSll()
    {
        var tmp = Sll8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x37
    private void XSllA()
    {
        Regs.A = Sll8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "srl (ix + D),Q" operation (0x38-0x3F)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are shifted right 1 bit position. The contents of bit 0 are copied
    /// to the Carry flag, and bit 7 is reset. The result is stored in register Q.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the source byte.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>

    // 0x38
    private void XSrlB()
    {
        Regs.B = Srl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x39
    private void XSrlC()
    {
        Regs.C = Srl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x3A
    private void XSrlD()
    {
        Regs.D = Srl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x3B
    private void XSrlE()
    {
        Regs.E = Srl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x3C
    private void XSrlH()
    {
        Regs.H = Srl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x3D
    private void XSrlL()
    {
        Regs.L = Srl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "srl (ix + D)" operation (0x3E)
    /// </summary>
    /// <remarks>
    /// The contents of the indexed memory address are shifted right 1 bit position. The contents of bit 0 are copied
    /// to the Carry flag, and bit 7 is reset.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the source byte.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>
    private void XSrl()
    {
        var tmp = Srl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x3F
    private void XSrlA()
    {
        Regs.A = Srl8(ReadMemory(Regs.WZ));
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "bit N,(ix+D)" operation (0x40-0x7F)
    /// </summary>
    /// <remarks>
    /// This instruction tests bit N in the indexed memory location and sets the Z flag accordingly.
    /// 
    /// S Set if N = 7 and tested bit is set.
    /// Z is set if specified bit is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is Set just like ZF flag.
    /// N is reset.
    /// C is not affected.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 20 (4, 4, 3, 5, 4)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 ×2,ii+n:3,ii+n:1
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4
    /// </remarks>

    // 0x40-0x47
    private void XBit0()
    {
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Bit8W(0, tmp);
    }

    // 0x48-0x4F
    private void XBit1()
    {
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Bit8W(1, tmp);
    }

    // 0x50-0x57
    private void XBit2()
    {
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Bit8W(2, tmp);
    }

    // 0x58-0x5F
    private void XBit3()
    {
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Bit8W(3, tmp);
    }

    // 0x60-0x67
    private void XBit4()
    {
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Bit8W(4, tmp);
    }

    // 0x68-0x6F
    private void XBit5()
    {
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Bit8W(5, tmp);
    }

    // 0x70-0x77
    private void XBit6()
    {
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Bit8W(6, tmp);
    }

    // 0x78-0x7F
    private void XBit7()
    {
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Bit8W(7, tmp);
    }

    /// <summary>
    /// "res N,(ix+D),Q" operation (0x80-0xBF)
    /// </summary>
    /// <remarks>
    /// Bit N of the indexed memory location addressed is reset. The result is autocopied to register Q.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>

    // 0x80
    private void XRes0B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) & 0xfe);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x81
    private void XRes0C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) & 0xfe);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x82
    private void XRes0D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) & 0xfe);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x83
    private void XRes0E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) & 0xfe);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x84
    private void XRes0H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) & 0xfe);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x85
    private void XRes0L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) & 0xfe);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0x86
    private void XRes0()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) & 0xfe);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x87
    private void XRes0A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) & 0xfe);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0x88
    private void XRes1B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) & 0xfd);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x89
    private void XRes1C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) & 0xfd);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x8A
    private void XRes1D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) & 0xfd);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x8B
    private void XRes1E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) & 0xfd);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x8C
    private void XRes1H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) & 0xfd);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x8D
    private void XRes1L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) & 0xfd);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0x8E
    private void XRes1()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) & 0xfd);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x8F
    private void XRes1A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) & 0xfd);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0x90
    private void XRes2B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) & 0xfb);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x91
    private void XRes2C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) & 0xfb);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x92
    private void XRes2D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) & 0xfb);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x93
    private void XRes2E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) & 0xfb);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x94
    private void XRes2H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) & 0xfb);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x95
    private void XRes2L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) & 0xfb);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0x96
    private void XRes2()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) & 0xfb);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x97
    private void XRes2A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) & 0xfb);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0x98
    private void XRes3B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) & 0xf7);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0x99
    private void XRes3C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) & 0xf7);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0x9A
    private void XRes3D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) & 0xf7);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0x9B
    private void XRes3E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) & 0xf7);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0x9C
    private void XRes3H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) & 0xf7);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0x9D
    private void XRes3L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) & 0xf7);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0x9E
    private void XRes3()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) & 0xf7);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0x9F
    private void XRes3A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) & 0xf7);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xA0
    private void XRes4B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) & 0xef);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xA1
    private void XRes4C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) & 0xef);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xA2
    private void XRes4D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) & 0xef);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xA3
    private void XRes4E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) & 0xef);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xA4
    private void XRes4H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) & 0xef);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xA5
    private void XRes4L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) & 0xef);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xA6
    private void XRes4()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) & 0xef);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xA7
    private void XRes4A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) & 0xef);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xA8
    private void XRes5B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) & 0xdf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xA9
    private void XRes5C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) & 0xdf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xAA
    private void XRes5D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) & 0xdf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xAB
    private void XRes5E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) & 0xdf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xAC
    private void XRes5H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) & 0xdf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xAD
    private void XRes5L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) & 0xdf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xAE
    private void XRes5()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) & 0xdf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xAF
    private void XRes5A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) & 0xdf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xB0
    private void XRes6B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) & 0xbf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xB1
    private void XRes6C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) & 0xbf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xB2
    private void XRes6D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) & 0xbf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xB3
    private void XRes6E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) & 0xbf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xB4
    private void XRes6H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) & 0xbf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xB5
    private void XRes6L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) & 0xbf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xB6
    private void XRes6()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) & 0xbf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xB7
    private void XRes6A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) & 0xbf);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xB8
    private void XRes7B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) & 0x7f);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xB9
    private void XRes7C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) & 0x7f);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xBA
    private void XRes7D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) & 0x7f);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xBB
    private void XRes7E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) & 0x7f);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xBC
    private void XRes7H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) & 0x7f);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xBD
    private void XRes7L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) & 0x7f);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xBE
    private void XRes7()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) & 0x7f);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xBF
    private void XRes7A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) & 0x7f);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "set N,(ix+D),Q" operation (0xC0-0xFF)
    /// </summary>
    /// <remarks>
    /// Bit N of the indexed memory location addressed is set. The result is autocopied to register Q.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:4,ii+n(write):3
    /// </remarks>

    // 0xC0
    private void XSet0B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) | 0x01);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xC1
    private void XSet0C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) | 0x01);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xC2
    private void XSet0D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) | 0x01);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xC3
    private void XSet0E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) | 0x01);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xC4
    private void XSet0H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) | 0x01);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xC5
    private void XSet0L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) | 0x01);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xC6
    private void XSet0()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) | 0x01);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xC7
    private void XSet0A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) | 0x01);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xC8
    private void XSet1B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) | 0x02);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xC9
    private void XSet1C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) | 0x02);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xCA
    private void XSet1D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) | 0x02);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xCB
    private void XSet1E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) | 0x02);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xCC
    private void XSet1H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) | 0x02);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xCD
    private void XSet1L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) | 0x02);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xCE
    private void XSet1()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) | 0x02);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xCF
    private void XSet1A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) | 0x02);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xD0
    private void XSet2B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) | 0x04);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xD1
    private void XSet2C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) | 0x04);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xD2
    private void XSet2D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) | 0x04);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xD3
    private void XSet2E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) | 0x04);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xD4
    private void XSet2H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) | 0x04);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xD5
    private void XSet2L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) | 0x04);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xD6
    private void XSet2()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) | 0x04);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xD7
    private void XSet2A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) | 0x04);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xD8
    private void XSet3B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) | 0x08);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xD9
    private void XSet3C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) | 0x08);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xDA
    private void XSet3D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) | 0x08);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xDB
    private void XSet3E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) | 0x08);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xDC
    private void XSet3H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) | 0x08);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xDD
    private void XSet3L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) | 0x08);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xDE
    private void XSet3()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) | 0x08);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xDF
    private void XSet3A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) | 0x08);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xE0
    private void XSet4B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) | 0x10);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xE1
    private void XSet4C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) | 0x10);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xE2
    private void XSet4D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) | 0x10);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xE3
    private void XSet4E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) | 0x10);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xE4
    private void XSet4H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) | 0x10);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xE5
    private void XSet4L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) | 0x10);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xE6
    private void XSet4()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) | 0x10);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xE7
    private void XSet4A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) | 0x10);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xE8
    private void XSet5B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) | 0x20);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xE9
    private void XSet5C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) | 0x20);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xEA
    private void XSet5D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) | 0x20);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xEB
    private void XSet5E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) | 0x20);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xEC
    private void XSet5H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) | 0x20);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xED
    private void XSet5L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) | 0x20);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xEE
    private void XSet5()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) | 0x20);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xEF
    private void XSet5A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) | 0x20);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xF0
    private void XSet6B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) | 0x40);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xF1
    private void XSet6C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) | 0x40);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xF2
    private void XSet6D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) | 0x40);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xF3
    private void XSet6E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) | 0x40);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xF4
    private void XSet6H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) | 0x40);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xF5
    private void XSet6L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) | 0x40);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xF6
    private void XSet6()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) | 0x40);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xF7
    private void XSet6A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) | 0x40);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }

    // 0xF8
    private void XSet7B()
    {
        Regs.B = (byte)(ReadMemory(Regs.WZ) | 0x80);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.B);
    }

    // 0xF9
    private void XSet7C()
    {
        Regs.C = (byte)(ReadMemory(Regs.WZ) | 0x80);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.C);
    }

    // 0xFA
    private void XSet7D()
    {
        Regs.D = (byte)(ReadMemory(Regs.WZ) | 0x80);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.D);
    }

    // 0xFB
    private void XSet7E()
    {
        Regs.E = (byte)(ReadMemory(Regs.WZ) | 0x80);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.E);
    }

    // 0xFC
    private void XSet7H()
    {
        Regs.H = (byte)(ReadMemory(Regs.WZ) | 0x80);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.H);
    }

    // 0xFD
    private void XSet7L()
    {
        Regs.L = (byte)(ReadMemory(Regs.WZ) | 0x80);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.L);
    }

    // 0xFE
    private void XSet7()
    {
        var tmp = (byte)(ReadMemory(Regs.WZ) | 0x80);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, tmp);
    }

    // 0xFF
    private void XSet7A()
    {
        Regs.A = (byte)(ReadMemory(Regs.WZ) | 0x80);
        TactPlus1(Regs.WZ);
        WriteMemory(Regs.WZ, Regs.A);
    }
}