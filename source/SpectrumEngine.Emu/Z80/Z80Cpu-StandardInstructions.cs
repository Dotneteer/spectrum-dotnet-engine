namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition contains the code for processing standard Z80 instructions (with no prefix).
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// This array contains the 256 function references, each executing a  standard Z80 instruction.
    /// </summary>
    private Action[]? _standardInstrs;

    /// <summary>
    /// Initialize the table of standard instructions.
    /// </summary>
    private void InitializeStandardInstructionsTable()
    {
        _standardInstrs = new Action[]
        {
            Nop,        LdBCNN,     LdBCiA,     IncBC,      IncB,       DecB,       LdBN,       Rlca,       // 00-07
            ExAF,       AddHLBC,    LdABCi,     DecBC,      IncC,       DecC,       LdCN,       Rrca,       // 08-0f
            Djnz,       LdDENN,     LdDEiA,     IncDE,      IncD,       DecD,       LdDN,       Rla,        // 10-17
            JrE,        AddHLDE,    LdADEi,     DecDE,      IncE,       DecE,       LdEN,       Rra,        // 18-1f
            JrNZ,       LdHLNN,     LdNNiHL,    IncHL,      IncH,       DecH,       LdHN,       Daa,        // 20-27
            JrZ,        AddHLHL,    LdHLNNi,    DecHL,      IncL,       DecL,       LdLN,       Cpl,        // 28-2f
            JrNC,       LdSPNN,     LdNNiA,     IncSP,      IncHLi,     DecHLi,     LdHLiN,     Scf,        // 30-37
            JrC,        AddHLSP,    LdANNi,     DecSP,      IncA,       DecA,       LdAN,       Ccf,        // 38-3f

            Nop,        LdB_C,      LdB_D,      LdB_E,      LdB_H,      LdB_L,      LdB_HLi,    LdB_A,      // 40-47
            LdC_B,      Nop,        LdC_D,      LdC_E,      LdC_H,      LdC_L,      LdC_HLi,    LdC_A,      // 48-4f
            LdD_B,      LdD_C,      Nop,        LdD_E,      LdD_H,      LdD_L,      LdD_HLi,    LdD_A,      // 50-57
            LdE_B,      LdE_C,      LdE_D,      Nop,        LdE_H,      LdE_L,      LdE_HLi,    LdE_A,      // 58-5f
            LdH_B,      LdH_C,      LdH_D,      LdH_E,      Nop,        LdH_L,      LdH_HLi,    LdH_A,      // 60-67
            LdL_B,      LdL_C,      LdL_D,      LdL_E,      LdL_H,      Nop,        LdL_HLi,    LdL_A,      // 68-6f
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

    /// <summary>
    /// "nop" instruction (0x00)
    /// </summary>
    /// <remarks>
    /// The CPU performs no operation during this machine cycle.
    /// This instruction does not affect any flag.
    /// 
    /// T-states: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Nop() { }

    /// <summary>
    /// "ld bc,NN" instruction (0x01, N-LSB, N-MSB)
    /// </summary>
    /// <remarks>
    /// The 16-bit integer value is loaded to the BC register pair.
    /// This instruction does not affect any flag.
    /// 
    /// T-states: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void LdBCNN()
    {
        Regs.C = ReadCodeMemory();
        Regs.B = ReadCodeMemory();
    }

    /// <summary>
    /// "ld (bc),a" operation (0x02)
    /// </summary>
    /// <remarks>
    /// The contents of the A are loaded to the memory location specified by the contents of the register pair BC.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,bc:3
    /// </remarks>
    private void LdBCiA()
    {
        WriteMemory(Regs.BC, Regs.A);
        Regs.WH = Regs.A;
    }

    /// <summary>
    /// "inc bc" operation (0x03)
    /// </summary>
    /// <remarks>
    /// The contents of register pair BC are incremented.
    ///
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void IncBC()
    {
        Regs.BC++;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc b" operation (0x04)
    /// </summary>
    /// <remarks>
    /// Register B is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncB()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.B++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec b" operation (0x05)
    /// </summary>
    /// <remarks>
    /// Register B is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void DecB()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.B--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld b,N" operation (0x06)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to B.
    /// 
    /// T-States: 7, (4, 3)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void LdBN()
    {
        Regs.B = ReadCodeMemory();
    }

    /// <summary>
    /// "rlca" operation (0x07)
    /// </summary>
    /// <remarks>
    /// The contents of  A are rotated left 1 bit position. The sign bit (bit 7) is copied to the Carry flag and also
    /// to bit 0.
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Rlca()
    {
        int rlcaVal = Regs.A;
        rlcaVal <<= 1;
        var cf = (byte)((rlcaVal & 0x100) != 0 ? FlagsSetMask.C : 0);
        if (cf != 0)
        {
            rlcaVal = (rlcaVal | 0x01) & 0xFF;
        }
        Regs.A = (byte)rlcaVal;
        Regs.F = (byte)(cf | (Regs.F & FlagsSetMask.SZPV) | (Regs.A & FlagsSetMask.R3R5));
        F53Updated = true;
    }

    /// <summary>
    /// "ex af,af'" operation (0x08)
    /// </summary>
    /// <remarks>
    ///  The 2-byte contents of the register pairs AF and AF' are exchanged.
    ///  
    ///  T-States: 4
    ///  Contention breakdown: pc:4
    /// </remarks>
    private void ExAF()
    {
        Regs.ExchangeAfSet();
    }

    /// <summary>
    /// "add hl,bc" operation (0x09)
    /// </summary>
    /// <remarks>
    /// The contents of BC are added to the contents of HL and the result is stored in HL.
    /// S, Z, P/V are not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:11
    /// </remarks>
    private void AddHLBC()
    {
        Regs.WZ = (ushort)(Regs.HL + 1);
        Regs.HL = AluAddHL(Regs.HL, Regs.BC);
        TactPlus7(Regs.IR);
    }

    /// <summary>
    /// "ld a,(bc)" operation (0x0A)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location specified by BC are loaded to A.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,bc:3
    /// </remarks>
    private void LdABCi()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.A = ReadMemory(Regs.BC);
    }

    /// <summary>
    /// "dec bc" operation (0x0B)
    /// </summary>
    /// <remarks>
    /// The contents of register pair BC are decremented.
    /// 
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void DecBC()
    {
        Regs.BC--;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc c" operation (0x0c)
    /// </summary>
    /// <remarks>
    /// Register C is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncC()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.C++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec c" operation (0x0D)
    /// </summary>
    /// <remarks>
    /// Register C is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// </remarks>
    private void DecC()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.C--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld c,N" operation (0x0E)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to C.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdCN()
    {
        Regs.C = ReadCodeMemory();
    }

    /// <summary>
    /// "rrca" operation (0x0F)
    /// </summary>
    /// <remarks>
    /// The contents of A are rotated right 1 bit position. Bit 0 is copied to the Carry flag and also to bit 7.
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Rrca()
    {
        int rrcaVal = Regs.A;
        var cf = (byte)((rrcaVal & 0x01) != 0 ? FlagsSetMask.C : 0);
        if ((rrcaVal & 0x01) != 0)
        {
            rrcaVal = (rrcaVal >> 1) | 0x80;
        }
        else
        {
            rrcaVal >>= 1;
        }
        Regs.A = (byte)rrcaVal;
        Regs.F = (byte)(cf | (Regs.F & FlagsSetMask.SZPV) | (Regs.A & FlagsSetMask.R3R5));
        F53Updated = true;
    }

    /// <summary>
    /// "djnz E" operation (0x10)
    /// </summary>
    /// <remarks>
    /// This instruction is similar to the conditional jump instructions except that value of B is used to determine
    /// branching. B is decremented, and if a nonzero value remains, the value of displacement E is added to PC. The
    /// next instruction is fetched from the location designated by the new contents of the PC. The jump is measured
    /// from the address of the instruction opcode and contains a range of –126 to +129 bytes. The assembler
    /// automatically adjusts for the twice incremented PC. If the result of decrementing leaves B with a zero value,
    /// the next instruction executed is taken from the location following this instruction.
    /// 
    /// T-States:
    ///   B!=0: 13 (5, 3, 5)
    ///   B=0:  8 (5, 3)
    /// Contention breakdown: pc:5,pc+1:3,[pc+1:1 x 5]
    /// Gate array contention breakdown: pc:5,pc+1:3,[5]
    /// </remarks>
    private void Djnz()
    {
        TactPlus1(Regs.IR);
        var e = ReadCodeMemory();
        if (--Regs.B != 0)
        {
            RelativeJump(e);
        }
    }

    /// <summary>
    /// "ld de,NN" instruction (0x11, N-LSB, N-MSB)
    /// </summary>
    /// <remarks>
    /// The 16-bit integer value is loaded to the DE register pair.
    /// This instruction does not affect any flag.
    /// 
    /// T-states: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void LdDENN()
    {
        Regs.E = ReadCodeMemory();
        Regs.D = ReadCodeMemory();
    }

    /// <summary>
    /// "ld (de),a" operation (0x12)
    /// </summary>
    /// <remarks>
    /// The contents of the A are loaded to the memory location specified by the contents of the register pair DE.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,bc:3
    /// </remarks>
    private void LdDEiA()
    {
        WriteMemory(Regs.DE, Regs.A);
        Regs.WH = Regs.A;
    }

    /// <summary>
    /// "inc de" operation (0x13)
    /// </summary>
    /// <remarks>
    /// The contents of register pair DE are incremented.
    ///
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void IncDE()
    {
        Regs.DE++;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc d" operation (0x14)
    /// </summary>
    /// <remarks>
    /// Register D is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncD()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.D++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec d" operation (0x15)
    /// </summary>
    /// <remarks>
    /// Register D is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void DecD()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.D--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld d,N" operation (0x16)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to D.
    /// 
    /// T-States: 7, (4, 3)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void LdDN()
    {
        Regs.D = ReadCodeMemory();
    }

    /// <summary>
    /// "rla" operation (0x17)
    /// </summary>
    /// <remarks>
    /// The contents of A are rotated left 1 bit position through the Carry flag. The previous contents of the Carry
    /// flag are copied to bit 0.
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Rla()
    {
        var rlaVal = Regs.A;
        var newCF = (rlaVal & 0x80) != 0 ? FlagsSetMask.C : 0;
        rlaVal <<= 1;
        if (Regs.CFlag)
        {
            rlaVal |= 0x01;
        }
        Regs.A = rlaVal;
        Regs.F = (byte)(newCF | (Regs.F & FlagsSetMask.SZPV) | (Regs.A & FlagsSetMask.R3R5));
        F53Updated = true;
    }

    /// <summary>
    /// "jr e" operation (0x18)
    /// </summary>
    /// <remarks>
    /// This instruction provides for unconditional branching to other segments of a program. The value of displacement
    /// E is added to PC and the next instruction is fetched from the location designated by the new contents of the
    /// PC. This jump is measured from the address of the instruction op code and contains a range of –126 to +129
    /// bytes. The assembler automatically adjusts for the twice incremented PC.
    /// 
    /// T-States: 12 (4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void JrE()
    {
        RelativeJump(ReadCodeMemory());
    }

    /// <summary>
    /// "add hl,de" operation (0x19)
    /// </summary>
    /// <remarks>
    /// The contents of DE are added to the contents of HL and the result is stored in HL.
    /// S, Z, P/V are not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:11
    /// </remarks>
    private void AddHLDE()
    {
        Regs.WZ = (ushort)(Regs.HL + 1);
        Regs.HL = AluAddHL(Regs.HL, Regs.DE);
        TactPlus7(Regs.IR);
    }

    /// <summary>
    /// "ld a,(de)" operation (0x1A)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location specified by DE are loaded to A.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,bc:3
    /// </remarks>
    private void LdADEi()
    {
        Regs.WZ = (ushort)(Regs.DE + 1);
        Regs.A = ReadMemory(Regs.DE);
    }

    /// <summary>
    /// "dec de" operation (0x1B)
    /// </summary>
    /// <remarks>
    /// The contents of register pair DE are decremented.
    /// 
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void DecDE()
    {
        Regs.DE--;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc e" operation (0x1C)
    /// </summary>
    /// <remarks>
    /// Register E is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncE()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.E++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec e" operation (0x1D)
    /// </summary>
    /// <remarks>
    /// Register E is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// </remarks>
    private void DecE()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.E--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld e,N" operation (0x1E)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to E.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdEN()
    {
        Regs.E = ReadCodeMemory();
    }

    /// <summary>
    /// "rra" operation (0x1F)
    /// </summary>
    /// <remarks>
    /// The contents of A are rotated right 1 bit position through the Carry flag. The previous contents of the Carry
    /// flag are copied to bit 7.
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of A.
    ///     
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Rra()
    {
        var rraVal = Regs.A;
        var newCF = (rraVal & 0x01) != 0 ? FlagsSetMask.C : 0;
        rraVal >>= 1;
        if (Regs.CFlag)
        {
            rraVal |= 0x80;
        }
        Regs.A = rraVal;
        Regs.F = (byte)(newCF | (Regs.F & FlagsSetMask.SZPV) | (Regs.A & FlagsSetMask.R3R5));
        F53Updated = true;
    }

    /// <summary>
    /// "jr nz,E" operation (0x20)
    /// </summary>
    /// <remarks>
    /// This instruction provides for conditional branching to other segments of a program depending on the results of
    /// a test (Z flag is not set). If the test evaluates to *true*, the value of displacement E is added to PC and
    /// the next instruction is fetched from the location designated by the new contents of the PC. The jump is
    /// measured from the address of the instruction op code and contains a range of –126 to +129 bytes. The assembler
    /// automatically adjusts for the twice incremented PC.
    /// 
    /// T-States:
    ///   Condition is met: 12 (4, 3, 5)
    ///   Condition is not met: 7 (4, 3)
    /// Contention breakdown: pc:4,pc+1:3,[pc+1:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:3,[5]
    /// </remarks>
    private void JrNZ()
    {
        var e = ReadCodeMemory();
        if ((Regs.F & FlagsSetMask.Z) == 0)
        {
            RelativeJump(e);
        }
    }

    /// <summary>
    /// "ld hl,NN" operation (0x21)
    /// </summary>
    /// <remarks>
    /// The 16-bit integer value is loaded to the HL register pair.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void LdHLNN()
    {
        Regs.L = ReadCodeMemory();
        Regs.H = ReadCodeMemory();
    }

    /// <summary>
    /// "ld (NN),hl" operation (0x22)
    /// </summary>
    /// <remarks>
    /// The contents of the low-order portion of HL (L) are loaded to memory address (NN), and the contents of the
    /// high-order portion of HL (H) are loaded to the next highest memory address(NN + 1).
    /// 
    /// T-States: 16 (4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,nn:3,nn+1:3
    /// </remarks>
    private void LdNNiHL()
    {
        Store16(Regs.L, Regs.H);
    }

    /// <summary>
    /// "inc hl" operation (0x23)
    /// </summary>
    /// <remarks>
    /// The contents of register pair HL are incremented.
    ///
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void IncHL()
    {
        Regs.HL++;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc h" operation (0x24)
    /// </summary>
    /// <remarks>
    /// Register H is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncH()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.H++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec h" operation (0x25)
    /// </summary>
    /// <remarks>
    /// Register H is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void DecH()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.H--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld h,N" operation (0x26)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to H.
    /// 
    /// T-States: 7, (4, 3)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void LdHN()
    {
        Regs.H = ReadCodeMemory();
    }

    /// <summary>
    /// "daa" operation (0x27)
    /// </summary>
    /// <remarks>
    /// This instruction conditionally adjusts A for BCD addition and subtraction operations. For addition(ADD, ADC,
    /// INC) or subtraction(SUB, SBC, DEC, NEG), the following table indicates the operation being performed:
    /// 
    /// ====================================================
    /// |Oper.|C before|Upper|H before|Lower|Number|C after|
    /// |     |DAA     |Digit|Daa     |Digit|Added |Daa    |
    /// ====================================================
    /// | ADD |   0    | 9-0 |   0    | 0-9 |  00  |   0   |
    /// |     |   0    | 0-8 |   0    | A-F |  06  |   0   |
    /// |     |   0    | 0-9 |   1    | 0-3 |  06  |   0   |
    /// |     |   0    | A-F |   0    | 0-9 |  60  |   1   |
    /// ----------------------------------------------------
    /// | ADC |   0    | 9-F |   0    | A-F |  66  |   1   |
    /// ----------------------------------------------------
    /// | INC |   0    | A-F |   1    | 0-3 |  66  |   1   |
    /// |     |   1    | 0-2 |   0    | 0-9 |  60  |   1   |
    /// |     |   1    | 0-2 |   0    | A-F |  66  |   1   |
    /// |     |   1    | 0-3 |   1    | 0-3 |  66  |   1   |
    /// ----------------------------------------------------
    /// | SUB |   0    | 0-9 |   0    | 0-9 |  00  |   0   |
    /// ----------------------------------------------------
    /// | SBC |   0    | 0-8 |   1    | 6-F |  FA  |   0   |
    /// ----------------------------------------------------
    /// | DEC |   1    | 7-F |   0    | 0-9 |  A0  |   1   |
    /// ----------------------------------------------------
    /// | NEG |   1    | 6-7 |   1    | 6-F |  9A  |   1   |
    /// ====================================================
    /// 
    /// S is set if most-significant bit of the A is 1 after an operation; otherwise, it is reset.
    /// Z is set if A is 0 after an operation; otherwise, it is reset.
    /// H: see the DAA instruction table.
    /// P/V is set if A is at even parity after an operation; otherwise, it is reset.
    /// N is not affected.
    /// C: see the DAA instruction table.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Daa()
    {
        var add = 0;
        var carry = Regs.F & FlagsSetMask.C;
        if (((Regs.F & FlagsSetMask.H) != 0) || (Regs.A & 0x0f) > 9)
        {
            add = 6;
        }
        if (carry != 0 || (Regs.A > 0x99))
        {
            add |= 0x60;
        }
        if (Regs.A > 0x99)
        {
            carry = FlagsSetMask.C;
        }
        if ((Regs.F & FlagsSetMask.N) != 0)
        {
            Sub8((byte)add);
        }
        else
        {
            Add8((byte)add);
        }

        Regs.F = (byte)((Regs.F & ~(FlagsSetMask.C | FlagsSetMask.PV)) | carry | s_ParityTable![Regs.A]);
        F53Updated = true;
    }


    /// <summary>
    /// "jr z,E" operation (0x28)
    /// </summary>
    /// <remarks>
    /// This instruction provides for conditional branching to other segments of a program depending on the results of
    /// a test (Z flag is set). If the test evaluates to *true*, the value of displacement E is added to PC and the
    /// next instruction is fetched from the location designated by the new contents of the PC. The jump is measured
    /// from the address of the instruction op code and contains a range of –126 to +129 bytes. The assembler
    /// automatically adjusts for the twice incremented PC.
    ///
    /// T-States: 
    ///   Condition is met: 12 (4, 3, 5)
    ///   Condition is not met: 7 (4, 3)
    /// Contention breakdown: pc:4,pc+1:3,[pc+1:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:3,[5]
    /// </remarks>
    private void JrZ()
    {
        var e = ReadCodeMemory();
        if ((Regs.F & FlagsSetMask.Z) != 0)
        {
            RelativeJump(e);
        }
    }

    /// <summary>
    /// "add hl,hl" operation (0x29)
    /// </summary>
    /// <remarks>
    /// The contents of HL are added to the contents of HL and the result is stored in HL.
    /// S, Z, P/V are not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:11
    /// </remarks>
    private void AddHLHL()
    {
        Regs.WZ = (ushort)(Regs.HL + 1);
        Regs.HL = AluAddHL(Regs.HL, Regs.HL);
        TactPlus7(Regs.IR);
    }

    /// <summary>
    /// "ld hl,(NN)" operation (0x2A)
    /// </summary>
    /// <remarks>
    /// The contents of memory address (NN) are loaded to the low-order portion of HL (L), and the contents of the next
    /// highest memory address (NN + 1) are loaded to the high-order portion of HL (H).
    ///
    /// T-States: 16 (4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,nn:3,nn+1:3
    /// </remarks>
    private void LdHLNNi()
    {
        ushort adr = ReadCodeMemory();
        adr += (ushort)(ReadCodeMemory() << 8);
        Regs.WZ = (ushort)(adr + 1);
        ushort val = ReadMemory(adr);
        val += (ushort)(ReadMemory(Regs.WZ) << 8);
        Regs.HL = val;
    }

    /// <summary>
    /// "dec hl" operation (0x2B)
    /// </summary>
    /// <remarks>
    /// The contents of register pair HL are decremented.
    /// 
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void DecHL()
    {
        Regs.HL--;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc l" operation (0x2C)
    /// </summary>
    /// <remarks>
    /// Register L is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncL()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.L++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec l" operation (0x2D)
    /// </summary>
    /// <remarks>
    /// Register E is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// </remarks>
    private void DecL()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.L--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld l,N" operation (0x2E)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to E.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdLN()
    {
        Regs.L = ReadCodeMemory();
    }

    /// <summary>
    /// "cpl" operation (0x2F)
    /// </summary>
    /// <remarks>
    /// The contents of A are inverted (one's complement).
    /// S, Z, P/V, C are not affected.
    /// H and N are set.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Cpl()
    {
        Regs.A ^= 0xFF;
        Regs.F = (byte)((Regs.F & (FlagsSetMask.C | FlagsSetMask.PV | FlagsSetMask.Z | FlagsSetMask.S)) |
            (Regs.A & FlagsSetMask.R3R5) | FlagsSetMask.N | FlagsSetMask.H);
        F53Updated = true;
    }

    /// <summary>
    /// "jr nc,E" operation (0x30)
    /// </summary>
    /// <remarks>
    /// This instruction provides for conditional branching to other segments of a program depending on the results of
    /// a test (C flag is not set). If the test evaluates to *true*, the value of displacement E is added to PC and
    /// the next instruction is fetched from the location designated by the new contents of the PC. The jump is
    /// measured from the address of the instruction op code and contains a range of –126 to +129 bytes. The assembler
    /// automatically adjusts for the twice incremented PC.
    /// 
    /// T-States: 
    ///   Condition is met: 12 (4, 3, 5)
    ///   Condition is not met: 7, (4, 3)
    /// Contention breakdown: pc:4,pc+1:3,[pc+1:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:3,[5]
    /// </remarks>
    private void JrNC()
    {
        var e = ReadCodeMemory();
        if ((Regs.F & FlagsSetMask.C) == 0)
        {
            RelativeJump(e);
        }
    }

    /// <summary>
    /// "ld sp,NN" instruction (0x31, N-LSB, N-MSB)
    /// </summary>
    /// <remarks>
    /// The 16-bit integer value is loaded to the SP register pair.
    /// This instruction does not affect any flag.
    /// 
    /// T-states: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void LdSPNN()
    {
        Regs.SP = (ushort)(ReadCodeMemory() + (ReadCodeMemory() << 8));
    }

    /// <summary>
    /// "ld (NN),a" operation (0x32)
    /// </summary>
    /// <remarks>
    /// The contents of A are loaded to the memory address specified by the operand NN
    /// 
    /// T-States: 13 (4, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,nn:3
    /// </remarks>
    private void LdNNiA()
    {
        var l = ReadCodeMemory();
        var addr = (ushort)((ReadCodeMemory() << 8) | l);
        Regs.WL = (byte)((addr + 1) & 0xFF);
        Regs.WH = Regs.A;
        WriteMemory(addr, Regs.A);
    }

    /// <summary>
    /// "inc sp" operation (0x33)
    /// </summary>
    /// <remarks>
    /// The contents of register pair SP are incremented.
    ///
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void IncSP()
    {
        Regs.SP++;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc (hl)" operation (0x34)
    /// </summary>
    /// <remarks>
    /// The byte contained in the address specified by the contents HL is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if (HL) was 0x7F before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,hl:4,hl(write):3
    /// </remarks>
    private void IncHLi()
    {
        var memValue = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Regs.F = (byte)(s_8BitIncFlags[memValue++] | Regs.F & FlagsSetMask.C);
        F53Updated = true;
        WriteMemory(Regs.HL, memValue);
    }

    /// <summary>
    /// "dec (hl)" operation (0x35)
    /// </summary>
    /// <remarks>
    /// The byte contained in the address specified by the contents HL is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if (HL) was 0x80 before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,hl:4,hl(write):3
    /// </remarks>
    private void DecHLi()
    {
        var memValue = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Regs.F = (byte)(s_8BitDecFlags[memValue--] | Regs.F & FlagsSetMask.C);
        F53Updated = true;
        WriteMemory(Regs.HL, memValue);
    }

    /// <summary>
    /// "ld (hl),N" operation (0x36)
    /// </summary>
    /// <remarks>
    /// The N 8-bit value is loaded to the memory address specified by HL.
    ///
    /// T-States: 10, (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,hl:3
    /// </remarks>
    private void LdHLiN()
    {
        var val = ReadCodeMemory();
        WriteMemory(Regs.HL, val);
    }

    /// <summary>
    /// "scf" operation (0x37)
    /// </summary>
    /// <remarks>
    /// The Carry flag in F is set.
    /// Other flags are not affected, except R5 and R3
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Scf()
    {
        Regs.F = (byte)((Regs.F & FlagsSetMask.SZPV) | FlagsSetMask.C);
        SetR5R3ForScfAndCcf();
    }

    /// <summary>
    /// "jr c,E" operation (0x38)
    /// </summary>
    /// <remarks>
    /// This instruction provides for conditional branching to other segments of a program depending on the results of
    /// a test (C flag is set). If the test evaluates to *true*, the value of displacement E is added to PC and the 
    /// next instruction is fetched from the location designated by the new contents of the PC. The jump is measured
    /// from the address of the instruction op code and contains a range of –126 to +129 bytes. The assembler
    /// automatically adjusts for the twice incremented PC.
    /// 
    /// T-States: 
    ///   Condition is met: 12, (4, 3, 5)
    ///   Condition is not met: 7 (4, 3)
    /// Contention breakdown: pc:4,pc+1:3,[pc+1:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:3,[5]
    /// </remarks>
    private void JrC()
    {
        var e = ReadCodeMemory();
        if ((Regs.F & FlagsSetMask.C) != 0)
        {
            RelativeJump(e);
        }
    }

    /// <summary>
    /// "add hl,sp" operation (0x39)
    /// </summary>
    /// <remarks>
    /// The contents of SP are added to the contents of HL and the result is stored in HL.
    /// S, Z, P/V are not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:11
    /// </remarks>
    private void AddHLSP()
    {
        Regs.WZ = (ushort)(Regs.HL + 1);
        Regs.HL = AluAddHL(Regs.HL, Regs.SP);
        TactPlus7(Regs.IR);
    }

    /// <summary>
    /// "ld a,(NN)" operation (0x3A)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location specified by the operands NN are loaded to A.
    /// 
    /// T-States: 13 (4, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,nn:3
    /// </remarks>
    private void LdANNi()
    {
        Regs.WL = ReadCodeMemory();
        Regs.WH = ReadCodeMemory();
        Regs.A = ReadMemory(Regs.WZ);
        Regs.WZ++;
    }

    /// <summary>
    /// "dec sp" operation (0x3B)
    /// </summary>
    /// <remarks>
    /// The contents of register pair HL are decremented.
    /// 
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void DecSP()
    {
        Regs.SP--;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc a" operation (0x3C)
    /// </summary>
    /// <remarks>
    /// Register A is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncA()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.A++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec a" operation (0x3D)
    /// </summary>
    /// <remarks>
    /// Register A is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// </remarks>
    private void DecA()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.A--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld a,N" operation (0x3E)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to A.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void LdAN()
    {
        Regs.A = ReadCodeMemory();
    }

    /// <summary>
    /// "ccf" operation (0x3F)
    /// </summary>
    /// <remarks>
    /// The Carry flag in F is inverted.
    /// Other flags are not affected, excep R3 and R5
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Ccf()
    {
        Regs.F = (byte)((Regs.F & FlagsSetMask.SZPV) | ((Regs.F & FlagsSetMask.C) != 0 ? FlagsSetMask.H : FlagsSetMask.C));
        SetR5R3ForScfAndCcf();
    }

    /// <summary>
    /// "ld b,c" operation (0x41)
    /// </summary>
    /// <remarks>
    /// The contents of C are loaded to B.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdB_C()
    {
        Regs.B = Regs.C;
    }

    /// <summary>
    /// "ld b,d" operation (0x42)
    /// </summary>
    /// <remarks>
    /// The contents of D are loaded to B.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdB_D()
    {
        Regs.B = Regs.D;
    }

    /// <summary>
    /// "ld b,e" operation (0x43)
    /// </summary>
    /// <remarks>
    /// The contents of E are loaded to B.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdB_E()
    {
        Regs.B = Regs.E;
    }

    /// <summary>
    /// "ld b,h" operation (0x44)
    /// </summary>
    /// <remarks>
    /// The contents of H are loaded to B.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdB_H()
    {
        Regs.B = Regs.H;
    }

    /// <summary>
    /// "ld b,l" operation (0x45)
    /// </summary>
    /// <remarks>
    /// The contents of L are loaded to B.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdB_L()
    {
        Regs.B = Regs.L;
    }

    /// <summary>
    /// "ld b,(hl)" operation (0x46)
    /// </summary>
    /// <remarks>
    /// The 8-bit contents of memory location (HL) are loaded to B.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdB_HLi()
    {
        Regs.B = ReadMemory(Regs.HL);
    }

    /// <summary>
    /// "ld b,a" operation (0x47)
    /// </summary>
    /// <remarks>
    /// The contents of A are loaded to B.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdB_A()
    {
        Regs.B = Regs.A;
    }

    /// <summary>
    /// "ld c,b" operation (0x48)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to C.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdC_B()
    {
        Regs.C = Regs.B;
    }

    /// <summary>
    /// "ld c,d" operation (0x4A)
    /// </summary>
    /// <remarks>
    /// The contents of D are loaded to C.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdC_D()
    {
        Regs.C = Regs.D;
    }

    /// <summary>
    /// "ld c,e" operation (0x4B)
    /// </summary>
    /// <remarks>
    /// The contents of E are loaded to C.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdC_E()
    {
        Regs.C = Regs.E;
    }

    /// <summary>
    /// "ld c,h" operation (0x4C)
    /// </summary>
    /// <remarks>
    /// The contents of H are loaded to C.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdC_H()
    {
        Regs.C = Regs.H;
    }

    /// <summary>
    /// "ld c,l" operation (0x4D)
    /// </summary>
    /// <remarks>
    /// The contents of L are loaded to C.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdC_L()
    {
        Regs.C = Regs.L;
    }

    /// <summary>
    /// "ld c,(hl)" operation (0x4E)
    /// </summary>
    /// <remarks>
    /// The 8-bit contents of memory location (HL) are loaded to C.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdC_HLi()
    {
        Regs.C = ReadMemory(Regs.HL);
    }

    /// <summary>
    /// "ld c,a" operation (0x4F)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to C.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdC_A()
    {
        Regs.C = Regs.A;
    }

    /// <summary>
    /// "ld d,b" operation (0x50)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to D.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdD_B()
    {
        Regs.D = Regs.B;
    }

    /// <summary>
    /// "ld d,c" operation (0x51)
    /// </summary>
    /// <remarks>
    /// The contents of C are loaded to D.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdD_C()
    {
        Regs.D = Regs.C;
    }

    /// <summary>
    /// "ld d,e" operation (0x53)
    /// </summary>
    /// <remarks>
    /// The contents of E are loaded to D.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdD_E()
    {
        Regs.D = Regs.E;
    }

    /// <summary>
    /// "ld d,h" operation (0x54)
    /// </summary>
    /// <remarks>
    /// The contents of H are loaded to D.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdD_H()
    {
        Regs.D = Regs.H;
    }

    /// <summary>
    /// "ld d,l" operation (0x55)
    /// </summary>
    /// <remarks>
    /// The contents of L are loaded to D.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdD_L()
    {
        Regs.D = Regs.L;
    }

    /// <summary>
    /// "ld d,(hl)" operation (0x56)
    /// </summary>
    /// <remarks>
    /// The 8-bit contents of memory location (HL) are loaded to D.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdD_HLi()
    {
        Regs.D = ReadMemory(Regs.HL);
    }

    /// <summary>
    /// "ld d,a" operation (0x57)
    /// </summary>
    /// <remarks>
    /// The contents of A are loaded to D.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdD_A()
    {
        Regs.D = Regs.A;
    }

    /// <summary>
    /// "ld e,b" operation (0x58)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to E.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdE_B()
    {
        Regs.E = Regs.B;
    }

    /// <summary>
    /// "ld e,c" operation (0x59)
    /// </summary>
    /// <remarks>
    /// The contents of C are loaded to E.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdE_C()
    {
        Regs.E = Regs.C;
    }

    /// <summary>
    /// "ld e,d" operation (0x5A)
    /// </summary>
    /// <remarks>
    /// The contents of D are loaded to E.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdE_D()
    {
        Regs.E = Regs.D;
    }

    /// <summary>
    /// "ld e,h" operation (0x5C)
    /// </summary>
    /// <remarks>
    /// The contents of H are loaded to E.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdE_H()
    {
        Regs.E = Regs.H;
    }

    /// <summary>
    /// "ld e,l" operation (0x5D)
    /// </summary>
    /// <remarks>
    /// The contents of L are loaded to E.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdE_L()
    {
        Regs.E = Regs.L;
    }

    /// <summary>
    /// "ld e,(hl)" operation (0x5E)
    /// </summary>
    /// <remarks>
    /// The 8-bit contents of memory location (HL) are loaded to E.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdE_HLi()
    {
        Regs.E = ReadMemory(Regs.HL);
    }

    /// <summary>
    /// "ld e,a" operation (0x5F)
    /// </summary>
    /// <remarks>
    /// The contents of A are loaded to E.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdE_A()
    {
        Regs.E = Regs.A;
    }

    /// <summary>
    /// "ld h,b" operation (0x60)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to H.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdH_B()
    {
        Regs.H = Regs.B;
    }

    /// <summary>
    /// "ld h,c" operation (0x61)
    /// </summary>
    /// <remarks>
    /// The contents of C are loaded to H.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdH_C()
    {
        Regs.H = Regs.C;
    }

    /// <summary>
    /// "ld h,d" operation (0x62)
    /// </summary>
    /// <remarks>
    /// The contents of D are loaded to H.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdH_D()
    {
        Regs.H = Regs.D;
    }

    /// <summary>
    /// "ld h,e" operation (0x63)
    /// </summary>
    /// <remarks>
    /// The contents of E are loaded to H.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdH_E()
    {
        Regs.H = Regs.E;
    }

    /// <summary>
    /// "ld h,l" operation (0x65)
    /// </summary>
    /// <remarks>
    /// The contents of L are loaded to H.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdH_L()
    {
        Regs.H = Regs.L;
    }

    /// <summary>
    /// "ld h,(hl)" operation (0x66)
    /// </summary>
    /// <remarks>
    /// The 8-bit contents of memory location (HL) are loaded to H.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdH_HLi()
    {
        Regs.H = ReadMemory(Regs.HL);
    }

    /// <summary>
    /// "ld h,a" operation (0x67)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to H.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdH_A()
    {
        Regs.H = Regs.A;
    }

    /// <summary>
    /// "ld l,b" operation (0x68)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to L.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdL_B()
    {
        Regs.L = Regs.B;
    }

    /// <summary>
    /// "ld l,c" operation (0x69)
    /// </summary>
    /// <remarks>
    /// The contents of C are loaded to L.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdL_C()
    {
        Regs.L = Regs.C;
    }

    /// <summary>
    /// "ld l,d" operation (0x6A)
    /// </summary>
    /// <remarks>
    /// The contents of D are loaded to L.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdL_D()
    {
        Regs.L = Regs.D;
    }

    /// <summary>
    /// "ld l,e" operation (0x6B)
    /// </summary>
    /// <remarks>
    /// The contents of E are loaded to L.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdL_E()
    {
        Regs.L = Regs.E;
    }

    /// <summary>
    /// "ld l,h" operation (0x6C)
    /// </summary>
    /// <remarks>
    /// The contents of H are loaded to L.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdL_H()
    {
        Regs.L = Regs.H;
    }

    /// <summary>
    /// "ld l,(hl)" operation (0x6E)
    /// </summary>
    /// <remarks>
    /// The 8-bit contents of memory location (HL) are loaded to L.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdL_HLi()
    {
        Regs.L = ReadMemory(Regs.HL);
    }

    /// <summary>
    /// "ld l,a" operation (0x6F)
    /// </summary>
    /// <remarks>
    /// The contents of A are loaded to L.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdL_A()
    {
        Regs.L = Regs.A;
    }
}