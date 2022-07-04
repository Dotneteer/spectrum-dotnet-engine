// ReSharper disable InconsistentNaming
namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This file contains the code for processing extended Z80 instructions (with `$ED` prefix).
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// This array contains the 256 function references, each executing a  standard Z80 instruction.
    /// </summary>
    private Action[]? _extendedInstrs;

    /// <summary>
    /// Initialize the table of standard instructions.
    /// </summary>
    private void InitializeExtendedInstructionsTable()
    {
        _extendedInstrs = new Action[]
        {
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 00-07
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 08-0f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 10-17
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 18-1f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 20-27
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 28-2f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 30-37
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 38-3f

            InBC,       OutCB,      SbcHLBC,    LdNNiBC,    Neg,        Retn,       Im0,        LdIA,       // 40-47
            InCC,       OutCC,      AdcHLBC,    LdBCNNi,    Neg,        Retn,       Im0,        LdRA,       // 48-4f
            InDC,       OutCD,      SbcHLDE,    LdNNiDE,    Neg,        Retn,       Im1,        LdAI,       // 50-57
            InEC,       OutCE,      AdcHLDE,    LdDENNi,    Neg,        Retn,       Im2,        LdAR,       // 58-5f
            InHC,       OutCH,      SbcHLHL,    LdNNiHL,    Neg,        Retn,       Im0,        Rrd,        // 60-67
            InLC,       OutCL,      AdcHLHL,    LdHLNNi,    Neg,        Retn,       Im0,        Rld,        // 68-6f
            InC,        OutC0,      SbcHLSP,    LdNNiSP,    Neg,        Retn,       Im1,        Nop,        // 70-77
            InAC,       OutCA,      AdcHLSP,    LdSPNNi,    Neg,        Retn,       Im2,        Nop,        // 78-7f

            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 80-87
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 88-8f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 90-97
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 98-9f
            Ldi,        Cpi,        Ini,        Outi,       Nop,        Nop,        Nop,        Nop,        // a0-a7
            Ldd,        Cpd,        Ind,        Outd,       Nop,        Nop,        Nop,        Nop,        // a8-af
            Ldir,       Cpir,       Inir,       Otir,       Nop,        Nop,        Nop,        Nop,        // b0-b7
            Lddr,       Cpdr,       Indr,       Otdr,       Nop,        Nop,        Nop,        Nop,        // b8-bf

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

    /// <summary>
    /// "in b,(c)" operation (0x40)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then one byte from the selected port is placed on the data bus and written to
    /// Register B in the CPU.
    /// 
    /// S is set if input data is negative; otherwise, it is reset.
    /// Z is set if input data is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity is even; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void InBC()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.B = ReadPort(Regs.BC);
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.B]);
        F53Updated = true;
    }

    /// <summary>
    /// "out (c),b" operation (0x41)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then the byte contained in register B is placed on the data bus and written to
    /// the selected peripheral device.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void OutCB()
    {
        WritePort(Regs.BC, Regs.B);
        Regs.WZ = (ushort)(Regs.BC + 1);
    }

    /// <summary>
    /// "sbc hl,bc" operation (0x42)
    /// </summary>
    /// <remarks>
    /// The contents of the register pair BC and the Carry Flag are subtracted from the contents of HL, and the result
    /// is stored in HL.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 12; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    ///
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void SbcHLBC()
    {
        TactPlus7(Regs.IR);
        Sbc16(Regs.BC);
    }

    /// <summary>
    /// "ld (NN),bc" operation (0x43)
    /// </summary>
    /// <remarks>
    /// The low-order byte of register pair BC is loaded to memory address (NN); the upper byte is loaded to memory
    /// address(NN + 1).
    /// 
    /// T-States: 20 (4, 4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,nn:3,nn+1:3
    /// </remarks>
    private void LdNNiBC()
    {
        Store16(Regs.C, Regs.B);
    }

    /// <summary>
    /// "neg" operation (0x44, 0x4c, 0x54, 0x5c, 0x64, 0x6c, 0x74, 0x7c)
    /// </summary>
    /// <remarks>
    /// The contents of the Accumulator are negated (two's complement).
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if Accumulator was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is set if Accumulator was not 00h before operation; otherwise, it is reset.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Neg()
    {
        var tmp = Regs.A;
        Regs.A = 0;
        Sub8(tmp);
    }

    /// <summary>
    /// "im 0" operation (0x46, 0x4E, 0x66, 0x6E)
    /// </summary>
    /// <remarks>
    /// Sets Interrupt Mode to 0
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Im0()
    {
        InterruptMode = 0;
    }

    /// <summary>
    /// "ld i,a" operation (0x47)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of A are loaded to I
    /// 
    /// T-States: 4, 5 (9)
    /// Contention breakdown: pc:4,pc+1:5
    /// </remarks>
    private void LdIA()
    {
        TactPlus1(Regs.IR);
        Regs.I = Regs.A;
    }

    /// <summary>
    /// "in c,(c)" operation (0x48)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then one byte from the selected port is placed on the data bus and written to
    /// Register C in the CPU.
    /// 
    /// S is set if input data is negative; otherwise, it is reset.
    /// Z is set if input data is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity is even; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void InCC()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.C = ReadPort(Regs.BC);
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.C]);
        F53Updated = true;
    }

    /// <summary>
    /// "out (c),c" operation (0x49)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then the byte contained in register C is placed on the data bus and written to
    /// the selected peripheral device.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void OutCC()
    {
        WritePort(Regs.BC, Regs.C);
        Regs.WZ = (ushort)(Regs.BC + 1);
    }

    /// <summary>
    /// "adc hl,bc" operation (0x4A)
    /// </summary>
    /// <remarks>
    /// The contents of register pair BC are added with the Carry flag to the contents of HL, and the result is stored
    /// in HL.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    ///
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void AdcHLBC()
    {
        TactPlus7(Regs.IR);
        Adc16(Regs.BC);
    }

    /// <summary>
    /// "ld bc,(NN)" operation (0x4B)
    /// </summary>
    /// <remarks>
    /// The contents of memory address (NN) are loaded to the low-order portion of BC (C), and the contents of the next
    /// highest memory address (NN + 1) are loaded to the high-order portion of BC (B).
    ///
    /// T-States: 16 (4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,nn:3,nn+1:3
    /// </remarks>
    private void LdBCNNi()
    {
        ushort tmp = FetchCodeByte();
        tmp += (ushort)(FetchCodeByte() << 8);
        Regs.C = ReadMemory(tmp);
        tmp += 1;
        Regs.WZ = tmp;
        Regs.B = ReadMemory(tmp);
    }

    /// <summary>
    /// "ld r,a" operation (0x4f)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of A are loaded to R
    /// 
    /// T-States: 4, 5 (9)
    /// Contention breakdown: pc:4,pc+1:5
    /// </remarks>
    private void LdRA()
    {
        TactPlus1(Regs.IR);
        Regs.R = Regs.A;
    }

    /// <summary>
    /// "in d,(c)" operation (0x50)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then one byte from the selected port is placed on the data bus and written to
    /// Register D in the CPU.
    /// 
    /// S is set if input data is negative; otherwise, it is reset.
    /// Z is set if input data is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity is even; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void InDC()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.D = ReadPort(Regs.BC);
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.D]);
        F53Updated = true;
    }

    /// <summary>
    /// "out (c),d" operation (0x51)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then the byte contained in register D is placed on the data bus and written to
    /// the selected peripheral device.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void OutCD()
    {
        WritePort(Regs.BC, Regs.D);
        Regs.WZ = (ushort)(Regs.BC + 1);
    }

    /// <summary>
    /// "sbc hl,de" operation (0x52)
    /// </summary>
    /// <remarks>
    /// The contents of the register pair DE and the Carry Flag are subtracted from the contents of HL, and the result
    /// is stored in HL.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 12; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    ///
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void SbcHLDE()
    {
        TactPlus7(Regs.IR);
        Sbc16(Regs.DE);
    }

    /// <summary>
    /// "ld (NN),de" operation (0x53)
    /// </summary>
    /// <remarks>
    /// The low-order byte of register pair DE is loaded to memory address (NN); the upper byte is loaded to memory
    /// address(NN + 1).
    /// 
    /// T-States: 20 (4, 4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,nn:3,nn+1:3
    /// </remarks>
    private void LdNNiDE()
    {
        Store16(Regs.E, Regs.D);
    }

    /// <summary>
    /// "im 1" operation (0x56, 0x76)
    /// </summary>
    /// <remarks>
    /// Sets Interrupt Mode to 1
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Im1()
    {
        InterruptMode = 1;
    }

    /// <summary>
    /// "ld a,i" operation (0x57)
    /// </summary>
    /// <remarks>
    /// The contents of I are loaded to A
    /// 
    /// S is set if the I Register is negative; otherwise, it is reset.
    /// Z is set if the I Register is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V contains contents of IFF2.
    /// N is reset.
    /// C is not affected.
    /// If an interrupt occurs during execution of this instruction, the Parity flag contains a 0.
    /// 
    /// T-States: 9 (4, 5)
    /// Contention breakdown: pc:4,pc+1:5
    /// </remarks>
    private void LdAI()
    {
        TactPlus1(Regs.IR);
        Regs.A = Regs.I;
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.A] | (Iff2 ? FlagsSetMask.PV : 0));
        F53Updated = true;
    }

    /// <summary>
    /// "in e,(c)" operation (0x58)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then one byte from the selected port is placed on the data bus and written to
    /// Register C in the CPU.
    /// 
    /// S is set if input data is negative; otherwise, it is reset.
    /// Z is set if input data is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity is even; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void InEC()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.E = ReadPort(Regs.BC);
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.E]);
        F53Updated = true;
    }

    /// <summary>
    /// "out (c),e" operation (0x59)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then the byte contained in register E is placed on the data bus and written to
    /// the selected peripheral device.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void OutCE()
    {
        WritePort(Regs.BC, Regs.E);
        Regs.WZ = (ushort)(Regs.BC + 1);
    }

    /// <summary>
    /// "adc hl,de" operation (0x5A)
    /// </summary>
    /// <remarks>
    /// The contents of register pair DE are added with the Carry flag to the contents of HL, and the result is stored
    /// in HL.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    ///
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void AdcHLDE()
    {
        TactPlus7(Regs.IR);
        Adc16(Regs.DE);
    }

    /// <summary>
    /// "ld de,(NN)" operation (0x5B)
    /// </summary>
    /// <remarks>
    /// The contents of memory address (NN) are loaded to the low-order portion of DE (E), and the contents of the next
    /// highest memory address (NN + 1) are loaded to the high-order portion of DE (D).
    ///
    /// T-States: 16 (4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,nn:3,nn+1:3
    /// </remarks>
    private void LdDENNi()
    {
        ushort tmp = FetchCodeByte();
        tmp += (ushort)(FetchCodeByte() << 8);
        Regs.E = ReadMemory(tmp);
        tmp += 1;
        Regs.WZ = tmp;
        Regs.D = ReadMemory(tmp);
    }

    /// <summary>
    /// "ld a,r" operation (0x5F)
    /// </summary>
    /// <remarks>
    /// The contents of R are loaded to A
    /// 
    /// S is set if the R Register is negative; otherwise, it is reset.
    /// Z is set if the R Register is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V contains contents of IFF2.
    /// N is reset.
    /// C is not affected.
    /// If an interrupt occurs during execution of this instruction, the Parity flag contains a 0.
    /// 
    /// T-States: 9 (4, 5)
    /// Contention breakdown: pc:4,pc+1:5
    /// </remarks>
    private void LdAR()
    {
        TactPlus1(Regs.IR);
        Regs.A = Regs.R;
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.A] | (Iff2 ? FlagsSetMask.PV : 0));
        F53Updated = true;
    }

    /// <summary>
    /// "im 2" operation (0x5E, 0x7E)
    /// </summary>
    /// <remarks>
    /// Sets Interrupt Mode to 2
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Im2()
    {
        InterruptMode = 2;
    }

    /// <summary>
    /// "in h,(c)" operation (0x60)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then one byte from the selected port is placed on the data bus and written to
    /// Register H in the CPU.
    /// 
    /// S is set if input data is negative; otherwise, it is reset.
    /// Z is set if input data is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity is even; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void InHC()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.H = ReadPort(Regs.BC);
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.H]);
        F53Updated = true;
    }

    /// <summary>
    /// "out (c),h" operation (0x61)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then the byte contained in register H is placed on the data bus and written to
    /// the selected peripheral device.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void OutCH()
    {
        WritePort(Regs.BC, Regs.H);
        Regs.WZ = (ushort)(Regs.BC + 1);
    }

    /// <summary>
    /// "sbc hl,hl" operation (0x62)
    /// </summary>
    /// <remarks>
    /// The contents of the register pair HL and the Carry Flag are subtracted from the contents of HL, and the result
    /// is stored in HL.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 12; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    ///
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void SbcHLHL()
    {
        TactPlus7(Regs.IR);
        Sbc16(Regs.HL);
    }

    /// <summary>
    /// "rrd" operation (0x67)
    /// </summary>
    /// <remarks>
    /// The contents of the low-order four bits (bits 3, 2, 1, and 0) of memory location (HL) are copied to the
    /// low-order four bits of A. The previous contents of the low-order four bits of A are opied to the high-order
    /// four bits(7, 6, 5, and 4) of location (HL); and the previous contents of the high-order four bits of (HL)
    /// are copied to the low-order four bits of (HL). The contents of the high-order bits of A are unaffected.
    /// 
    /// S is set if A is negative after an operation; otherwise, it is reset.
    /// Z is set if A is 0 after an operation; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if the parity of A is even after an operation; otherwise, 
    /// it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 18 (4, 4, 3, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1 ×4,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:7,hl(write):3
    /// </remarks>
    private void Rrd()
    {
        var tmp = ReadMemory(Regs.HL);
        TactPlus4(Regs.HL);
        WriteMemory(Regs.HL, (byte)((Regs.A << 4) | (tmp >> 4)));
        Regs.A = (byte)((Regs.A & 0xf0) | (tmp & 0x0f));
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53PVTable![Regs.A]);
        Regs.WZ = (ushort)(Regs.HL + 1);
    }

    /// <summary>
    /// "in l,(c)" operation (0x68)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then one byte from the selected port is placed on the data bus and written to
    /// Register L in the CPU.
    /// 
    /// S is set if input data is negative; otherwise, it is reset.
    /// Z is set if input data is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity is even; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void InLC()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.L = ReadPort(Regs.BC);
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.L]);
        F53Updated = true;
    }

    /// <summary>
    /// "out (c),l" operation (0x69)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then the byte contained in register L is placed on the data bus and written to
    /// the selected peripheral device.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void OutCL()
    {
        WritePort(Regs.BC, Regs.L);
        Regs.WZ = (ushort)(Regs.BC + 1);
    }

    /// <summary>
    /// "adc hl,hl" operation (0x6A)
    /// </summary>
    /// <remarks>
    /// The contents of register pair HL are added with the Carry flag to the contents of HL, and the result is stored
    /// in HL.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    ///
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void AdcHLHL()
    {
        TactPlus7(Regs.IR);
        Adc16(Regs.HL);
    }

    /// <summary>
    /// "rld" operation (0x6F)
    /// </summary>
    /// <remarks>
    /// The contents of the low-order four bits (bits 3, 2, 1, and 0) of the memory location (HL) are copied to the
    /// high-order four bits (7, 6, 5, and 4) of that same memory location; the previous contents of those high-order
    /// four bits are copied to the low-order four bits of A; and the previous contents of the low-order four bits of
    /// A are copied to the low-order four bits of memory location(HL). The contents of the high-order bits of A are
    /// unaffected.
    /// 
    /// S is set if A is negative after an operation; otherwise, it is reset.
    /// Z is set if the A is 0 after an operation; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if the parity of A is even after an operation; otherwise, 
    /// it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 18 (4, 4, 3, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1 ×4,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:7,hl(write):3
    /// </remarks>
    private void Rld()
    {
        var tmp = ReadMemory(Regs.HL);
        TactPlus4(Regs.HL);
        WriteMemory(Regs.HL, (byte)((tmp << 4) | (Regs.A & 0x0f)));
        Regs.A = (byte)((Regs.A & 0xf0) | (tmp >> 4));
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53PVTable![Regs.A]);
        Regs.WZ = (ushort)(Regs.HL + 1);
    }

    /// <summary>
    /// "in (c)" operation (0x70)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. 
    /// 
    /// S is set if input data is negative; otherwise, it is reset.
    /// Z is set if input data is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity is even; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void InC()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        var tmp = ReadPort(Regs.BC);
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![tmp]);
        F53Updated = true;
    }

    /// <summary>
    /// "out (c),0" operation (0x71)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then 0 is placed on the data bus and written to
    /// the selected peripheral device.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void OutC0()
    {
        WritePort(Regs.BC, 0);
        Regs.WZ = (ushort)(Regs.BC + 1);
    }

    /// <summary>
    /// "sbc hl,sp" operation (0x72)
    /// </summary>
    /// <remarks>
    /// The contents of the register pair SP and the Carry Flag are subtracted from the contents of HL, and the result
    /// is stored in HL.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 12; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    ///
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void SbcHLSP()
    {
        TactPlus7(Regs.IR);
        Sbc16(Regs.SP);
    }

    /// <summary>
    /// "ld (NN),sp" operation (0x73)
    /// </summary>
    /// <remarks>
    /// The low-order byte of register pair SP is loaded to memory address (NN); the upper byte is loaded to memory
    /// address(NN + 1).
    /// 
    /// T-States: 20 (4, 4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,nn:3,nn+1:3
    /// </remarks>
    private void LdNNiSP()
    {
        Store16((byte)Regs.SP, (byte)(Regs.SP >> 8));
    }

    /// <summary>
    /// "in a,(c)" operation (0x78)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then one byte from the selected port is placed on the data bus and written to
    /// Register A in the CPU.
    /// 
    /// S is set if input data is negative; otherwise, it is reset.
    /// Z is set if input data is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity is even; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void InAC()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.A = ReadPort(Regs.BC);
        Regs.F = (byte)(Regs.CFlagValue | s_SZ53Table![Regs.A]);
        F53Updated = true;
    }

    /// <summary>
    /// "out (c),a" operation (0x79)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. The contents of Register B are placed on the top half (A8 through A15) of
    /// the address bus at this time. Then the byte contained in register A is placed on the data bus and written to
    /// the selected peripheral device.
    /// 
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,I/O
    /// </remarks>
    private void OutCA()
    {
        WritePort(Regs.BC, Regs.A);
        Regs.WZ = (ushort)(Regs.BC + 1);
    }

    /// <summary>
    /// "adc hl,sp" operation (0x7A)
    /// </summary>
    /// <remarks>
    /// The contents of register pair SP are added with the Carry flag to the contents of HL, and the result is stored
    /// in HL.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    ///
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void AdcHLSP()
    {
        TactPlus7(Regs.IR);
        Adc16(Regs.SP);
    }

    /// <summary>
    /// "ld sp,(NN)" operation (0x7B)
    /// </summary>
    /// <remarks>
    /// The contents of memory address (NN) are loaded to the low-order portion of SP, and the contents of the next
    /// highest memory address (NN + 1) are loaded to the high-order portion of SP.
    ///
    /// T-States: 16 (4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,nn:3,nn+1:3
    /// </remarks>
    private void LdSPNNi()
    {
        ushort tmp = FetchCodeByte();
        tmp += (ushort)(FetchCodeByte() << 8);
        var val = ReadMemory(tmp);
        tmp += 1;
        Regs.WZ = tmp;
        Regs.SP = (ushort)((ReadMemory(tmp) << 8) + val);
    }

    /// <summary>
    /// "retn" operation (0x45, 0x4d, 0x55, 0x5d, 0x65, 0x6d, 0x75, 0x7d)
    /// </summary>
    /// <remarks>
    /// This instruction is used at the end of a nonmaskable interrupts service routine to restore the contents of PC.
    /// The state of IFF2 is copied back to IFF1 so that maskable interrupts are enabled immediately following the
    /// RETN if they were enabled before the nonmaskable interrupt.
    /// 
    /// T-States: 14 (4, 4, 4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:4,sp:3,sp+1:3
    /// </remarks>
    private void Retn()
    {
        Iff1 = Iff2;
        Ret();
    }

    /// <summary>
    /// "ldi" operation (0xA0)
    /// </summary>
    /// <remarks>
    /// A byte of data is transferred from the memory location addressed by the contents of HL to the memory location
    /// addressed by the contents of DE. Then both these register pairs are incremented and BC is decremented.
    /// 
    /// S is not affected.
    /// Z is not affected.
    /// H is reset.
    /// P/V is set if BC – 1 is not 0; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 16 (4, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,de:3,de:1 ×2
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:3,de:5
    /// </remarks>
    private void Ldi()
    {
        var tmp = ReadMemory(Regs.HL);
        Regs.BC--;
        WriteMemory(Regs.DE, tmp);
        TactPlus2(Regs.DE);
        Regs.DE++;
        Regs.HL++;
        tmp += Regs.A;
        Regs.F = (byte)
            ((Regs.F & (FlagsSetMask.C | FlagsSetMask.Z | FlagsSetMask.S)) |
        (Regs.BC != 0 ? FlagsSetMask.PV : 0) |
          (tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
        F53Updated = true;
    }

    /// <summary>
    /// "cpi" operation (0xA1)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location addressed by HL is compared with the contents of A. With a true compare, Z
    /// flag is set. Then HL is incremented and BC is decremented.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if A is (HL); otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if BC – 1 is not 0; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 16 (4, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1 ×5
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:8
    /// </remarks>
    private void Cpi()
    {
        var value = ReadMemory(Regs.HL);
        var tmp = (byte)(Regs.A - value);
        var lookup =
        ((Regs.A & 0x08) >> 3) |
          ((value & 0x08) >> 2) |
          ((tmp & 0x08) >> 1);
        TactPlus5(Regs.HL);
        Regs.HL++;
        Regs.BC--;
        Regs.F = (byte)
        (Regs.CFlagValue |
        (Regs.BC != 0 ? (FlagsSetMask.PV | FlagsSetMask.N) : FlagsSetMask.N) |
          s_HalfCarrySubFlags[lookup] |
        (tmp != 0 ? 0 : FlagsSetMask.Z) |
          (tmp & FlagsSetMask.S));
        if (Regs.IsHFlagSet)
        {
            tmp -= 1;
        }
        Regs.F |= (byte)((tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
        F53Updated = true;
        Regs.WZ++;
    }

    /// <summary>
    /// "ini" operation (0xA2)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. Register B can be used as a byte counter, and its contents are placed on
    /// the top half (A8 through A15) of the address bus at this time. Then one byte from the selected port is placed
    /// on the data bus and written to the CPU. The contents of the HL register pair are then placed on the address 
    /// bus and the input byte is written to the corresponding location of memory. Finally, B is decremented and HL
    /// is incremented.
    /// 
    /// S is unknown.
    /// Z is set if B – 1 = 0; otherwise it is reset.
    /// H is unknown.
    /// P/V is unknown.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 16 (4, 5, 3, 4)
    /// Contention breakdown: pc:4,pc+1:5,I/O,hl:3
    /// </remarks>
    private void Ini()
    {
        TactPlus1(Regs.IR);
        var tmp = ReadPort(Regs.BC);
        WriteMemory(Regs.HL, tmp);
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.B--;
        Regs.HL++;
        var tmp2 = (byte)(tmp + Regs.C + 1);
        Regs.F = (byte)
            (((tmp & 0x80) != 0 ? FlagsSetMask.N : 0) |
            (tmp2 < tmp ? FlagsSetMask.H | FlagsSetMask.C : 0) |
            (s_ParityTable![(tmp2 & 0x07) ^ Regs.B] != 0 ? FlagsSetMask.PV : 0) |
            s_SZ53Table![Regs.B]);
        F53Updated = true;
    }

    /// <summary>
    /// "outi" operation (0xa3)
    /// </summary>
    /// <remarks>
    /// The contents of the HL register pair are placed on the address bus to select a location in memory. The byte
    /// contained in this memory location is temporarily stored in the CPU. Then, after B is decremented, the contents
    /// of C are placed on the bottom half (A0 through A7) of the address bus to select the I/O device at one of 256
    /// possible ports. Register B is used as a byte counter, and its decremented value is placed on the top half (A8
    /// through A15) of the address bus. The byte to be output is placed on the data bus and written to a selected
    /// peripheral device. Finally, the HL is incremented.
    /// 
    /// S is unknown.
    /// Z is set if B – 1 = 0; otherwise it is reset.
    /// H is unknown.
    /// P/V is unknown.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 16 (4, 5, 3, 4)
    /// Contention breakdown: pc:4,pc+1:5,hl:3,I/O
    /// </remarks>
    private void Outi()
    {
        TactPlus1(Regs.IR);
        var tmp = ReadMemory(Regs.HL);
        Regs.B--;
        Regs.WZ = (ushort)(Regs.BC + 1);
        WritePort(Regs.BC, tmp);
        Regs.HL++;
        var tmp2 = (byte)(tmp + Regs.L);
        Regs.F = (byte)
            (((tmp & 0x80) != 0 ? FlagsSetMask.N : 0) |
            (tmp2 < tmp ? FlagsSetMask.H | FlagsSetMask.C : 0) |
            (s_ParityTable![(tmp2 & 0x07) ^ Regs.B] != 0 ? FlagsSetMask.PV : 0) |
            s_SZ53Table![Regs.B]);
        F53Updated = true;
    }

    /// <summary>
    /// "ldd" operation (0xA8)
    /// </summary>
    /// <remarks>
    /// Transfers a byte of data from the memory location addressed by HL to the memory location addressed by DE. Then
    /// DE, HL, and BC is decremented.
    /// 
    /// S is not affected.
    /// Z is not affected.
    /// H is reset.
    /// P/V is set if BC – 1 is not 0; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 16 (4, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,de:3,de:1 ×2
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:3,de:5
    /// </remarks>
    private void Ldd()
    {
        var tmp = ReadMemory(Regs.HL);
        Regs.BC--;
        WriteMemory(Regs.DE, tmp);
        TactPlus2(Regs.DE);
        Regs.DE--;
        Regs.HL--;
        tmp += Regs.A;
        Regs.F = (byte)
            ((Regs.F & (FlagsSetMask.C | FlagsSetMask.Z | FlagsSetMask.S)) |
        (Regs.BC != 0 ? FlagsSetMask.PV : 0) |
          (tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
        F53Updated = true;
    }

    /// <summary>
    /// "cpd" operation (0xA9)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location addressed by HL is compared with the contents of A. During the compare
    /// operation, the Zero flag is set or reset. HL and BC are decremented.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if A is (HL); otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if BC – 1 is not 0; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 16 (4, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1 ×5
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:8
    /// </remarks>
    private void Cpd()
    {
        var value = ReadMemory(Regs.HL);
        var tmp = (byte)(Regs.A - value);
        var lookup =
        ((Regs.A & 0x08) >> 3) |
          ((value & 0x08) >> 2) |
          ((tmp & 0x08) >> 1);
        TactPlus5(Regs.HL);
        Regs.HL--;
        Regs.BC--;
        Regs.F = (byte)
        (Regs.CFlagValue |
        (Regs.BC != 0 ? FlagsSetMask.PV | FlagsSetMask.N : FlagsSetMask.N) |
          s_HalfCarrySubFlags[lookup] |
        (tmp != 0 ? 0 : FlagsSetMask.Z) |
          (tmp & FlagsSetMask.S));
        if (Regs.IsHFlagSet)
        {
            tmp -= 1;
        }
        Regs.F |= (byte)((tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
        F53Updated = true;
        Regs.WZ--;
    }

    /// <summary>
    /// "ind" operation (0xAA)
    /// </summary>
    /// <remarks>
    /// The contents of C are placed on the bottom half (A0 through A7) of the address bus to select the I/O device at
    /// one of 256 possible ports. Register B is used as a byte counter, and its contents are placed on the top half
    /// (A8 through A15) of the address bus at this time. Then one byte from the selected port is placed on the data
    /// bus and written to the CPU. The contents of HL are placed on the address bus and the input byte is written to
    /// the corresponding location of memory. Finally, B and HLL are decremented.
    /// 
    /// S is unknown.
    /// Z is set if B – 1 = 0; otherwise it is reset.
    /// H is unknown.
    /// P/V is unknown.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 16 (4, 5, 3, 4)
    /// Contention breakdown: pc:4,pc+1:5,I/O,hl:3
    /// </remarks>
    private void Ind()
    {
        TactPlus1(Regs.IR);
        var tmp = ReadPort(Regs.BC);
        WriteMemory(Regs.HL, tmp);
        Regs.WZ = (ushort)(Regs.BC - 1);
        Regs.B--;
        Regs.HL--;
        var tmp2 = (byte)(tmp + Regs.C - 1);
        Regs.F = (byte)
            (((tmp & 0x80) != 0 ? FlagsSetMask.N : 0) |
            (tmp2 < tmp ? FlagsSetMask.H | FlagsSetMask.C : 0) |
            (s_ParityTable![(tmp2 & 0x07) ^ Regs.B] != 0 ? FlagsSetMask.PV : 0) |
            s_SZ53Table![Regs.B]);
        F53Updated = true;
    }

    /// <summary>
    /// "outd" operation (0xAD)
    /// </summary>
    /// <remarks>
    /// The contents of the HL register pair are placed on the address bus to select a location in memory. The byte
    /// contained in this memory location is temporarily stored in the CPU. Then, after B is decremented, the
    /// contents of C are placed on the bottom half (A0 through A7) of the address bus to select the I/O device at 
    /// one of 256 possible ports. Register B is used as a byte counter, and its decremented value is placed on the
    /// top half (A8 through A15) of the address bus. The byte to be output is placed on the data bus and written to
    /// a selected peripheral device. Finally, the HL is decremented.
    /// 
    /// S is unknown.
    /// Z is set if B – 1 = 0; otherwise it is reset.
    /// H is unknown.
    /// P/V is unknown.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 16 (4, 5, 3, 4)
    /// Contention breakdown: pc:4,pc+1:5,hl:3,I/O
    /// </remarks>
    private void Outd()
    {
        TactPlus1(Regs.IR);
        var tmp = ReadMemory(Regs.HL);
        Regs.B--;
        Regs.WZ = (ushort)(Regs.BC - 1);
        WritePort(Regs.BC, tmp);
        Regs.HL--;
        var tmp2 = (byte)(tmp + Regs.L);
        Regs.F = (byte)
            (((tmp & 0x80) != 0 ? FlagsSetMask.N : 0) |
            (tmp2 < tmp ? FlagsSetMask.H | FlagsSetMask.C : 0) |
            (s_ParityTable![(tmp2 & 0x07) ^ Regs.B] != 0 ? FlagsSetMask.PV : 0) |
            s_SZ53Table![Regs.B]);
        F53Updated = true;
    }

    /// <summary>
    /// "ldir" operation (0xB0)
    /// </summary>
    /// <remarks>
    /// Transfers a byte of data from the memory location addressed by HL to the memory location addressed DE. Then HL
    /// and DE are incremented. BC is decremented. If decrementing allows the BC to go to 0, the instruction is
    /// terminated. If BC isnot 0, the program counter is decremented by two and the instruction is repeated.
    /// Interrupts are recognized and two refresh cycles are executed after each data transfer. When the BC is set to
    /// 0 prior to instruction execution, the instruction loops through 64 KB.
    /// 
    /// S is not affected.
    /// Z is not affected.
    /// H is reset.
    /// P/V is set if BC – 1 is not 0; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 
    ///   BC!=0: 21 (4, 4, 3, 5, 5)
    ///   BC=0:  16 (4, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,de:3,de:1 ×2,[de:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:3,de:5,[5]
    /// </remarks>
    private void Ldir()
    {
        var tmp = ReadMemory(Regs.HL);
        WriteMemory(Regs.DE, tmp);
        TactPlus2(Regs.DE);
        Regs.BC--;
        tmp += Regs.A;
        Regs.F = (byte)
            ((Regs.F & (FlagsSetMask.C | FlagsSetMask.Z | FlagsSetMask.S)) |
        (Regs.BC != 0 ? FlagsSetMask.PV : 0) |
          (tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
        F53Updated = true;
        if (Regs.BC != 0)
        {
            TactPlus5(Regs.DE);
            Regs.PC -= 2;
            Regs.WZ = (byte)(Regs.PC +1);
        }
        Regs.HL++;
        Regs.DE++;
    }

    /// <summary>
    /// "cpir" operation (0xB1)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location addressed HL is compared with the contents of A. During a compare
    /// operation, the Zero flag is set or reset. Then HL is incremented and BC is decremented. If decrementing causes
    /// BC to go to 0 or if A = (HL), the instruction is terminated. If BC is not 0 and A is not equal (HL), the
    /// program counter is decremented by two and the instruction is repeated. Interrupts are recognized and two 
    /// refresh cycles are executed after each data transfer. If BC is set to 0 before instruction execution, the
    /// instruction loops through 64 KB if no match is found.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if A is (HL); otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if BC – 1 is not 0; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 
    ///   BC!=0: 21 (4, 4, 3, 5, 5)
    ///   BC=0:  16 (4, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1 ×5,[hl:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:8,[5]
    /// </remarks>
    private void Cpir()
    {
        var value = ReadMemory(Regs.HL);
        var tmp = (byte)(Regs.A - value);
        var lookup =
        ((Regs.A & 0x08) >> 3) |
          ((value & 0x08) >> 2) |
          ((tmp & 0x08) >> 1);
        TactPlus5(Regs.HL);
        Regs.BC--;
        Regs.F = (byte)
        (Regs.CFlagValue |
        (Regs.BC != 0 ? FlagsSetMask.PV | FlagsSetMask.N : FlagsSetMask.N) |
          s_HalfCarrySubFlags[lookup] |
        (tmp != 0 ? 0 : FlagsSetMask.Z) |
          (tmp & FlagsSetMask.S));
        if (Regs.IsHFlagSet)
        {
            tmp -= 1;
        }
        Regs.F |= (byte)((tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
        F53Updated = true;
        if ((Regs.F & (FlagsSetMask.PV | FlagsSetMask.Z)) == FlagsSetMask.PV)
        {
            TactPlus5(Regs.HL);
            Regs.PC -= 2;
            Regs.WZ = (byte)(Regs.PC + 1);
        }
        else
        {
            Regs.WZ++;
        }
        Regs.HL++;
    }

    /// <summary>
    /// "inir" operation (0xB2)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are placed on the bottom half (A0 through A7) of the address bus to select the I/O
    /// device at one of 256 possible ports. Register B can be used as a byte counter, and its contents are placed on
    /// the top half (A8 through A15) of the address bus at this time. Then one byte from the selected port is placed
    /// on the data bus and written to the CPU. The contents of the HL register pair are then placed on the address 
    /// bus and the input byte is written to the corresponding location of memory. Finally, B is decremented and HL
    /// is incremented. If decrementing causes B to go to 0, the instruction is terminated. If B is not 0, PC is
    /// decremented by two and the instruction repeated. Interrupts are recognized and two refresh cycles execute after
    /// each data transfer.
    /// 
    /// S is unknown.
    /// Z is set if B – 1 = 0; otherwise it is reset.
    /// H is unknown.
    /// P/V is unknown.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 
    ///   BC!=0: 21 (4, 5, 3, 4, 5)
    ///   BC=0:  16 (4, 5, 3, 4)
    /// Contention breakdown: pc:4,pc+1:5,I/O,hl:3,[hl:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:5,I/O,hl:3,[5]
    /// </remarks>
    private void Inir()
    {
        TactPlus1(Regs.IR);
        var tmp = ReadPort(Regs.BC);
        WriteMemory(Regs.HL, tmp);
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.B--;
        var tmp2 = (byte)(tmp + Regs.C + 1);
        Regs.F = (byte)
            (((tmp & 0x80) != 0 ? FlagsSetMask.N : 0) |
            (tmp2 < tmp ? FlagsSetMask.H | FlagsSetMask.C : 0) |
            (s_ParityTable![(tmp2 & 0x07) ^ Regs.B] != 0 ? FlagsSetMask.PV : 0) |
            s_SZ53Table![Regs.B]);
        F53Updated = true;
        if (Regs.B != 0)
        {
            TactPlus5(Regs.HL);
            Regs.PC -= 2;
        }
        Regs.HL++;
    }

    /// <summary>
    /// "otir" operation (0xB3)
    /// </summary>
    /// <remarks>
    /// The contents of the HL register pair are placed on the address bus to select a location in memory. The byte
    /// contained in this memory location is temporarily stored in the CPU. Then, after B is decremented, the contents
    /// of C are placed on the bottom half (A0 through A7) of the address bus to select the I/O device at one of 256
    /// possible ports. Register B is used as a byte counter, and its decremented value is placed on the top half (A8
    /// through A15) of the address bus. The byte to be output is placed on the data bus and written to a selected
    /// peripheral device. Finally, the HL is incremented. If the decremented B Register is not 0, PC is decremented
    /// by two and the instruction is repeated. If B has gone to 0, the instruction is terminated. Interrupts are
    /// recognized and two refresh cycles are executed after each data transfer.
    /// 
    /// S is unknown.
    /// Z is set if B – 1 = 0; otherwise it is reset.
    /// H is unknown.
    /// P/V is unknown.
    /// N is set.
    /// C is not affected.
    /// T-States: 
    ///   BC!=0: 21 (4, 5, 3, 4, 5)
    ///   BC=0:  16 (4, 5, 3, 4)
    /// Contention breakdown: pc:4,pc+1:5,hl:3,I/O,[bc:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:5,hl:3,I/O,[5]
    /// </remarks>
    private void Otir()
    {
        TactPlus1(Regs.IR);
        var tmp = ReadMemory(Regs.HL);
        Regs.B--;
        Regs.WZ = (byte)(Regs.BC + 1);
        WritePort(Regs.BC, tmp);
        Regs.HL++;
        var tmp2 = (byte)(tmp + Regs.L);
        Regs.F = (byte)
            (((tmp & 0x80) != 0 ? FlagsSetMask.N : 0) |
            (tmp2 < tmp ? FlagsSetMask.H | FlagsSetMask.C : 0) |
            (s_ParityTable![(tmp2 & 0x07) ^ Regs.B] != 0 ? FlagsSetMask.PV : 0) |
            s_SZ53Table![Regs.B]);
        F53Updated = true;
        if (Regs.B == 0) return;
        TactPlus5(Regs.HL);
        Regs.PC -= 2;
    }

    /// <summary>
    /// "lddr" operation (0xB8)
    /// </summary>
    /// <remarks>
    /// Transfers a byte of data from the memory location addressed by HL to the memory location addressed by DE. Then
    /// DE, HL, and BC is decremented. If decrementing causes BC to go to 0, the instruction is terminated. If BC is
    /// not 0, PC is decremented by two and the instruction is repeated. Interrupts are recognized and two refresh
    /// cycles execute after each data transfer. When BC is set to 0, prior to instruction execution, the instruction
    /// loops through 64 KB.
    ///  
    /// S is not affected.
    /// Z is not affected.
    /// H is reset.
    /// P/V is set if BC – 1 is not 0; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 
    ///   BC!=0: 21 (4, 4, 3, 5, 5)
    ///   BC=0:  16 (4, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,de:3,de:1 ×2,[de:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:3,de:5,[5]
    /// </remarks>
    private void Lddr()
    {
        var tmp = ReadMemory(Regs.HL);
        WriteMemory(Regs.DE, tmp);
        TactPlus2(Regs.DE);
        Regs.BC--;
        tmp += Regs.A;
        Regs.F = (byte)
            ((Regs.F & (FlagsSetMask.C | FlagsSetMask.Z | FlagsSetMask.S)) |
        (Regs.BC != 0 ? FlagsSetMask.PV : 0) |
          (tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
        F53Updated = true;
        if (Regs.BC != 0)
        {
            TactPlus5(Regs.DE);
            Regs.PC -= 2;
            Regs.WZ = (byte)(Regs.PC + 1);
        }
        Regs.HL--;
        Regs.DE--;
    }

    /// <summary>
    /// "cpdr" operation (0xB9)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location addressed by HL is compared with the contents of A. During the compare
    /// operation, the Zero flag is set or reset. HL and BC are decremented. If BC is not 0 and A = (HL), PC is
    /// decremented by two and the instruction is repeated. Interrupts are recognized and two refresh cycles execute
    /// after each data transfer. When the BC is set to 0, prior to instruction execution, the instruction loops 
    /// through 64 KB if no match is found.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if A is (HL); otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if BC – 1 is not 0; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 
    ///   BC!=0: 21 (4, 4, 3, 5, 5)
    ///   BC=0:  16 (4, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1 ×5,[hl:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:8,[5]
    /// </remarks>
    private void Cpdr()
    {
        var value = ReadMemory(Regs.HL);
        var tmp = (byte)(Regs.A - value);
        var lookup =
        ((Regs.A & 0x08) >> 3) |
          ((value & 0x08) >> 2) |
          ((tmp & 0x08) >> 1);
        TactPlus5(Regs.HL);
        Regs.BC--;
        Regs.F = (byte)
        (Regs.CFlagValue |
        (Regs.BC != 0 ? FlagsSetMask.PV | FlagsSetMask.N : FlagsSetMask.N) |
          s_HalfCarrySubFlags[lookup] |
        (tmp != 0 ? 0 : FlagsSetMask.Z) |
          (tmp & FlagsSetMask.S));
        if (Regs.IsHFlagSet)
        {
            tmp -= 1;
        }
        Regs.F |= (byte)((tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
        F53Updated = true;
        if ((Regs.F & (FlagsSetMask.PV | FlagsSetMask.Z)) == FlagsSetMask.PV)
        {
            TactPlus5(Regs.HL);
            Regs.PC -= 2;
            Regs.WZ = (byte)(Regs.PC + 1);
        }
        else
        {
            Regs.WZ--;
        }
        Regs.HL--;
    }

    /// <summary>
    /// "indr" operation (0xBA)
    /// </summary>
    /// <remarks>
    /// The contents of C are placed on the bottom half (A0 through A7) of the address bus to select the I/O device at
    /// one of 256 possible ports. Register B is used as a byte counter, and its contents are placed on the top half
    /// (A8 through A15) of the address bus at this time. Then one byte from the selected port is placed on the data
    /// bus and written to the CPU. The contents of HL are placed on the address bus and the input byte is written to
    /// the corresponding location of memory. Finally, B and HL are decremented. If decrementing causes B to go to 0,
    /// the instruction is terminated. If B is not 0, PC is decremented by two and the instruction repeated. Interrupts
    /// are recognized and two refresh cycles are executed after each data transfer.
    /// 
    /// S is unknown.
    /// Z is set if B – 1 = 0; otherwise it is reset.
    /// H is unknown.
    /// P/V is unknown.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 
    ///   BC!=0: 21 (4, 5, 3, 4, 5)
    ///   BC=0:  16 (4, 5, 3, 4)
    /// Contention breakdown: pc:4,pc+1:5,I/O,hl:3,[hl:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:5,I/O,hl:3,[5]
    /// </remarks>
    private void Indr()
    {
        TactPlus1(Regs.IR);
        var tmp = ReadPort(Regs.BC);
        WriteMemory(Regs.HL, tmp);
        Regs.WZ = (ushort)(Regs.BC - 1);
        Regs.B--;
        var tmp2 = (byte)(tmp + Regs.C - 1);
        Regs.F = (byte)
            (((tmp & 0x80) != 0 ? FlagsSetMask.N : 0) |
            (tmp2 < tmp ? FlagsSetMask.H | FlagsSetMask.C : 0) |
            (s_ParityTable![(tmp2 & 0x07) ^ Regs.B] != 0 ? FlagsSetMask.PV : 0) |
            s_SZ53Table![Regs.B]);
        F53Updated = true;
        if (Regs.B != 0)
        {
            TactPlus5(Regs.HL);
            Regs.PC -= 2;
        }
        Regs.HL--;
    }

    /// <summary>
    /// "otdr" operation (0xBB)
    /// </summary>
    /// <remarks>
    /// The contents of the HL register pair are placed on the address bus to select a location in memory. The byte
    /// contained in this memory location is temporarily stored in the CPU. Then, after B is decremented, the
    /// contents of C are placed on the bottom half (A0 through A7) of the address bus to select the I/O device at 
    /// one of 256 possible ports. Register B is used as a byte counter, and its decremented value is placed on the
    /// top half (A8 through A15) of the address bus. The byte to be output is placed on the data bus and written to
    /// a selected peripheral device. Finally, the HL is decremented. If the decremented B Register is not 0, PC is
    /// decremented by two and the instruction is repeated. If B has gone to 0, the instruction is terminated.
    /// Interrupts are recognized and two refresh cycles are executed after each data transfer.
    /// 
    /// S is unknown.
    /// Z is set if B – 1 = 0; otherwise it is reset.
    /// H is unknown.
    /// P/V is unknown.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 
    ///   BC!=0: 21 (4, 5, 3, 4, 5)
    ///   BC=0:  16 (4, 5, 3, 4)
    /// Contention breakdown: pc:4,pc+1:5,hl:3,I/O,[bc:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:5,hl:3,I/O,[5]
    /// </remarks>
    private void Otdr()
    {
        TactPlus1(Regs.IR);
        var tmp = ReadMemory(Regs.HL);
        Regs.B--;
        Regs.WZ = (byte)(Regs.BC - 1);
        WritePort(Regs.BC, tmp);
        Regs.HL--;
        var tmp2 = (byte)(tmp + Regs.L);
        Regs.F = (byte)
            (((tmp & 0x80) != 0 ? FlagsSetMask.N : 0) |
            (tmp2 < tmp ? FlagsSetMask.H | FlagsSetMask.C : 0) |
            (s_ParityTable![(tmp2 & 0x07) ^ Regs.B] != 0 ? FlagsSetMask.PV : 0) |
            s_SZ53Table![Regs.B]);
        F53Updated = true;
        if (Regs.B != 0)
        {
            TactPlus5(Regs.HL);
            Regs.PC -= 2;
        }
    }
}