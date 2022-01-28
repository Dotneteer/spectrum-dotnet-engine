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
            LdHLi_B,    LdHLi_C,    LdHLi_D,    LdHLi_E,    LdHLi_H,    LdHLi_L,    Halt,       LdHLi_A,    // 70-77
            LdA_B,      LdA_C,      LdA_D,      LdA_E,      LdA_H,      LdA_L,      LdA_HLi,    Nop,        // 78-7f

            AddA_B,     AddA_C,     AddA_D,     AddA_E,     AddA_H,     AddA_L,     AddA_HLi,   AddA_A,     // 80-87
            AdcA_B,     AdcA_C,     AdcA_D,     AdcA_E,     AdcA_H,     AdcA_L,     AdcA_HLi,   AdcA_A,     // 88-8f
            SubB,       SubC,       SubD,       SubE,       SubH,       SubL,       SubHLi,     SubA,       // 90-97
            SbcB,       SbcC,       SbcD,       SbcE,       SbcH,       SbcL,       SbcHLi,     SbcA,       // 98-9f
            AndB,       AndC,       AndD,       AndE,       AndH,       AndL,       AndHLi,     AndA,       // a0-a7
            XorB,       XorC,       XorD,       XorE,       XorH,       XorL,       XorHLi,     XorA,       // a8-af
            OrB,        OrC,        OrD,        OrE,        OrH,        OrL,        OrHLi,      OrA,        // b0-b7
            CpB,        CpC,        CpD,        CpE,        CpH,        CpL,        CpHLi,      CpA,        // b8-bf

            RetNZ,      PopBC,      JpNZ_NN,    JpNN,       CallNZ,     PushBC,     AddAN,      Rst00,      // c0-c7
            RetZ,       Ret,        JpZ_NN,     Nop,        CallZ,      CallNN,     AdcAN,      Rst08,      // c8-cf
            RetNC,      PopDE,      JpNC_NN,    OutNA,      CallNC,     PushDE,     SubAN,      Rst10,      // d0-d7
            RetC,       Exx,        JpC_NN,     InAN,       CallC,      Nop,        SbcAN,      Rst18,      // d8-df
            RetPO,      PopHL,      JpPO_NN,    ExSPiHL,    CallPO,     PushHL,     AndAN,      Rst20,      // e0-e7
            RetPE,      JpHL,       JpPE_NN,    ExDEHL,     CallPE,     Nop,        XorAN,      Rst28,      // e8-ef
            RetP,       PopAF,      JpP_NN,     Di,         CallP,      PushAF,     OrAN,       Rst30,      // f0-f7
            RetM,       LdSPHL,     JpM_NN,     Ei,         CallM,      Nop,        CpAN,       Rst38,      // f8-ff
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
        Regs.C = FetchCodeByte();
        Regs.B = FetchCodeByte();
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
        Regs.B = FetchCodeByte();
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
        TactPlus7(Regs.IR);
        Regs.HL = Add16(Regs.HL, Regs.BC);
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
        Regs.C = FetchCodeByte();
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
        var e = FetchCodeByte();
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
        Regs.E = FetchCodeByte();
        Regs.D = FetchCodeByte();
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
        Regs.D = FetchCodeByte();
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
        RelativeJump(FetchCodeByte());
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
        TactPlus7(Regs.IR);
        Regs.HL = Add16(Regs.HL, Regs.DE);
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
        Regs.E = FetchCodeByte();
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
        var e = FetchCodeByte();
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
        Regs.L = FetchCodeByte();
        Regs.H = FetchCodeByte();
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
        Regs.H = FetchCodeByte();
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
        var e = FetchCodeByte();
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
        TactPlus7(Regs.IR);
        Regs.HL = Add16(Regs.HL, Regs.HL);
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
        ushort adr = FetchCodeByte();
        adr += (ushort)(FetchCodeByte() << 8);
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
        Regs.L = FetchCodeByte();
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
        var e = FetchCodeByte();
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
        Regs.SP = (ushort)(FetchCodeByte() + (FetchCodeByte() << 8));
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
        var l = FetchCodeByte();
        var addr = (ushort)((FetchCodeByte() << 8) | l);
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
        var val = FetchCodeByte();
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
        var e = FetchCodeByte();
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
        TactPlus7(Regs.IR);
        Regs.HL = Add16(Regs.HL, Regs.SP);
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
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
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
        Regs.A = FetchCodeByte();
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

    /// <summary>
    /// "ld (hl),b" operation (0x70)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to the memory location specified by the contents of HL.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdHLi_B()
    {
        WriteMemory(Regs.HL, Regs.B);
    }

    /// <summary>
    /// "ld (hl),c" operation (0x71)
    /// </summary>
    /// <remarks>
    /// The contents of C are loaded to the memory location specified by the contents of HL.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdHLi_C()
    {
        WriteMemory(Regs.HL, Regs.C);
    }

    /// <summary>
    /// "ld (hl),d" operation (0x72)
    /// </summary>
    /// <remarks>
    /// The contents of D are loaded to the memory location specified by the contents of HL.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdHLi_D()
    {
        WriteMemory(Regs.HL, Regs.D);
    }

    /// <summary>
    /// "ld (hl),e" operation (0x73)
    /// </summary>
    /// <remarks>
    /// The contents of E are loaded to the memory location specified by the contents of HL.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdHLi_E()
    {
        WriteMemory(Regs.HL, Regs.E);
    }

    /// <summary>
    /// "ld (hl),h" operation (0x74)
    /// </summary>
    /// <remarks>
    /// The contents of H are loaded to the memory location specified by the contents of HL.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdHLi_H()
    {
        WriteMemory(Regs.HL, Regs.H);
    }

    /// <summary>
    /// "ld (hl),l" operation (0x75)
    /// </summary>
    /// <remarks>
    /// The contents of L are loaded to the memory location specified by the contents of HL.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdHLi_L()
    {
        WriteMemory(Regs.HL, Regs.L);
    }

    /// <summary>
    /// "halt" operation (0x76)
    /// </summary>
    /// <remarks>
    /// The HALT instruction suspends CPU operation until a subsequent interrupt or reset is received.While in the HALT
    /// state, the processor executes NOPs to maintain memory refresh logic.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Halt()
    {
        Halted = true;
        Regs.PC--;
    }

    /// <summary>
    /// "ld (hl),a" operation (0x77)
    /// </summary>
    /// <remarks>
    /// The contents of A are loaded to the memory location specified by the contents of HL.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdHLi_A()
    {
        WriteMemory(Regs.HL, Regs.A);
    }

    /// <summary>
    /// "ld a,b" operation (0x78)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdA_B()
    {
        Regs.A = Regs.B;
    }

    /// <summary>
    /// "ld a,c" operation (0x79)
    /// </summary>
    /// <remarks>
    /// The contents of C are loaded to A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdA_C()
    {
        Regs.A = Regs.C;
    }

    /// <summary>
    /// "ld a,d" operation (0x7A)
    /// </summary>
    /// <remarks>
    /// The contents of D are loaded to A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdA_D()
    {
        Regs.A = Regs.D;
    }

    /// <summary>
    /// "ld a,e" operation (0x7B)
    /// </summary>
    /// <remarks>
    /// The contents of E are loaded to A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdA_E()
    {
        Regs.A = Regs.E;
    }

    /// <summary>
    /// "ld a,h" operation (0x7C)
    /// </summary>
    /// <remarks>
    /// The contents of H are loaded to A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdA_H()
    {
        Regs.A = Regs.H;
    }

    /// <summary>
    /// "ld a,l" operation (0x7D)
    /// </summary>
    /// <remarks>
    /// The contents of L are loaded to A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdA_L()
    {
        Regs.A = Regs.L;
    }

    /// <summary>
    /// "ld a,(hl)" operation (0x7E)
    /// </summary>
    /// <remarks>
    /// The 8-bit contents of memory location (HL) are loaded to A.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void LdA_HLi()
    {
        Regs.A = ReadMemory(Regs.HL);
    }

    /// <summary>
    /// "add a,b" operation (0x80)
    /// </summary>
    /// <remarks>
    /// The contents of B are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AddA_B()
    {
        Add8(Regs.B);
    }

    /// <summary>
    /// "add a,c" operation (0x81)
    /// </summary>
    /// <remarks>
    /// The contents of C are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AddA_C()
    {
        Add8(Regs.C);
    }

    /// <summary>
    /// "add a,d" operation (0x82)
    /// </summary>
    /// <remarks>
    /// The contents of D are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AddA_D()
    {
        Add8(Regs.D);
    }

    /// <summary>
    /// "add a,e" operation (0x83)
    /// </summary>
    /// <remarks>
    /// The contents of E are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AddA_E()
    {
        Add8(Regs.E);
    }

    /// <summary>
    /// "add a,h" operation (0x84)
    /// </summary>
    /// <remarks>
    /// The contents of H are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AddA_H()
    {
        Add8(Regs.H);
    }

    /// <summary>
    /// "add a,l" operation (0x85)
    /// </summary>
    /// <remarks>
    /// The contents of L are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AddA_L()
    {
        Add8(Regs.L);
    }

    /// <summary>
    /// "add a,(hl)" operation (0x86)
    /// </summary>
    /// <remarks>
    /// The byte at the memory address specified by the contents of HL is added to the contents of A, and the result
    /// is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void AddA_HLi()
    {
        Add8(ReadMemory(Regs.HL));
    }

    /// <summary>
    /// "add a,a" operation (0x87)
    /// </summary>
    /// <remarks>
    /// The contents of A are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AddA_A()
    {
        Add8(Regs.A);
    }

    /// <summary>
    /// "adc a,b" operation (0x88)
    /// </summary>
    /// <remarks>
    /// The contents of B and the C flag are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AdcA_B()
    {
        Adc8(Regs.B);
    }

    /// <summary>
    /// "adc a,c" operation (0x89)
    /// </summary>
    /// <remarks>
    /// The contents of C and the C flag are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AdcA_C()
    {
        Adc8(Regs.C);
    }

    /// <summary>
    /// "adc a,d" operation (0x8A)
    /// </summary>
    /// <remarks>
    /// The contents of D and the C flag are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AdcA_D()
    {
        Adc8(Regs.D);
    }

    /// <summary>
    /// "adc a,e" operation (0x8B)
    /// </summary>
    /// <remarks>
    /// The contents of E and the C flag are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AdcA_E()
    {
        Adc8(Regs.E);
    }

    /// <summary>
    /// "adc a,h" operation (0x8C)
    /// </summary>
    /// <remarks>
    /// The contents of H and the C flag are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AdcA_H()
    {
        Adc8(Regs.H);
    }

    /// <summary>
    /// "adc a,b" operation (0x8D)
    /// </summary>
    /// <remarks>
    /// The contents of L and the C flag are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AdcA_L()
    {
        Adc8(Regs.L);
    }

    /// <summary>
    /// "adc a,(hl)" operation (0x8E)
    /// </summary>
    /// <remarks>
    /// The byte at the memory address specified by the contents of HL and the C flag is added to the contents of A,
    /// and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void AdcA_HLi()
    {
        Adc8(ReadMemory(Regs.HL));
    }

    /// <summary>
    /// "adc a,a" operation (0x8F)
    /// </summary>
    /// <remarks>
    /// The contents of A and the C flag are added to the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AdcA_A()
    {
        Adc8(Regs.A);
    }

    /// <summary>
    /// "sub b" operation (0x90)
    /// </summary>
    /// <remarks>
    /// The contents of B are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SubB()
    {
        Sub8(Regs.B);
    }

    /// <summary>
    /// "sub c" operation (0x91)
    /// </summary>
    /// <remarks>
    /// The contents of C are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SubC()
    {
        Sub8(Regs.C);
    }

    /// <summary>
    /// "sub d" operation (0x92)
    /// </summary>
    /// <remarks>
    /// The contents of D are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SubD()
    {
        Sub8(Regs.D);
    }

    /// <summary>
    /// "sub e" operation (0x93)
    /// </summary>
    /// <remarks>
    /// The contents of E are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SubE()
    {
        Sub8(Regs.E);
    }

    /// <summary>
    /// "sub h" operation (0x94)
    /// </summary>
    /// <remarks>
    /// The contents of H are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SubH()
    {
        Sub8(Regs.H);
    }

    /// <summary>
    /// "sub l" operation (0x95)
    /// </summary>
    /// <remarks>
    /// The contents of L are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SubL()
    {
        Sub8(Regs.L);
    }

    /// <summary>
    /// "sub (hl)" operation (0x96)
    /// </summary>
    /// <remarks>
    /// The byte at the memory address specified by the contents of HL is subtracted from the contents of A, and the
    /// result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void SubHLi()
    {
        Sub8(ReadMemory(Regs.HL));
    }

    /// <summary>
    /// "sub a" operation (0x97)
    /// </summary>
    /// <remarks>
    /// The contents of A are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SubA()
    {
        Sub8(Regs.A);
    }

    /// <summary>
    /// "sbc b" operation (0x98)
    /// </summary>
    /// <remarks>
    /// The contents of B and the C flag are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SbcB()
    {
        Sbc8(Regs.B);
    }

    /// <summary>
    /// "sbc c" operation (0x99)
    /// </summary>
    /// <remarks>
    /// The contents of C and the C flag are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SbcC()
    {
        Sbc8(Regs.C);
    }

    /// <summary>
    /// "sbc d" operation (0x9A)
    /// </summary>
    /// <remarks>
    /// The contents of D and the C flag are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SbcD()
    {
        Sbc8(Regs.D);
    }

    /// <summary>
    /// "sbc e" operation (0x9B)
    /// </summary>
    /// <remarks>
    /// The contents of E and the C flag are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SbcE()
    {
        Sbc8(Regs.E);
    }

    /// <summary>
    /// "sbc h" operation (0x9C)
    /// </summary>
    /// <remarks>
    /// The contents of B and the C flag are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SbcH()
    {
        Sbc8(Regs.H);
    }

    /// <summary>
    /// "sbc l" operation (0x9D)
    /// </summary>
    /// <remarks>
    /// The contents of B and the C flag are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SbcL()
    {
        Sbc8(Regs.L);
    }

    /// <summary>
    /// "sbc (hl)" operation (0x9E)
    /// </summary>
    /// <remarks>
    /// The byte at the memory address specified by the contents of HL and the C flag is subtracted from the contents
    /// of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void SbcHLi()
    {
        Sbc8(ReadMemory(Regs.HL));
    }

    /// <summary>
    /// "sbc a" operation (0x9F)
    /// </summary>
    /// <remarks>
    /// The contents of A and the C flag are subtracted from the contents of A, and the result is stored in A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void SbcA()
    {
        Sbc8(Regs.A);
    }

    /// <summary>
    /// "and b" operation (0xA0)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between B and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AndB()
    {
        And8(Regs.B);
    }

    /// <summary>
    /// "and c" operation (0xA1)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between C and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AndC()
    {
        And8(Regs.C);
    }

    /// <summary>
    /// "and d" operation (0xA2)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between D and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AndD()
    {
        And8(Regs.D);
    }

    /// <summary>
    /// "and e" operation (0xA3)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between E and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AndE()
    {
        And8(Regs.E);
    }

    /// <summary>
    /// "and h" operation (0xA4)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between H and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AndH()
    {
        And8(Regs.H);
    }

    /// <summary>
    /// "and l" operation (0xA5)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between L and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AndL()
    {
        And8(Regs.L);
    }

    /// <summary>
    /// "and (hl)" operation (0xA6)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between the byte at the memory address specified by the contents of HL
    /// and the byte contained in A; the result is stored in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void AndHLi()
    {
        And8(ReadMemory(Regs.HL));
    }


    /// <summary>
    /// "and a" operation (0xA7)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between A and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void AndA()
    {
        And8(Regs.A);
    }

    /// <summary>
    /// "xor b" operation (0xA8)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between B and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void XorB()
    {
        Xor8(Regs.B);
    }

    /// <summary>
    /// "xor c" operation (0xA9)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between C and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void XorC()
    {
        Xor8(Regs.C);
    }

    /// <summary>
    /// "xor d" operation (0xAA)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between D and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void XorD()
    {
        Xor8(Regs.D);
    }

    /// <summary>
    /// "xor e" operation (0xAB)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between E and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void XorE()
    {
        Xor8(Regs.E);
    }

    /// <summary>
    /// "xor h" operation (0xAC)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between H and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void XorH()
    {
        Xor8(Regs.H);
    }

    /// <summary>
    /// "xor l" operation (0xAD)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between L and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void XorL()
    {
        Xor8(Regs.L);
    }

    /// <summary>
    /// "xor (hl)" operation (0xAE)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between the byte at the memory address specified by the contents of HL and
    /// the byte contained in A; the result is stored in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void XorHLi()
    {
        Xor8(ReadMemory(Regs.HL));
    }

    /// <summary>
    /// "xor a" operation (0xAF)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between A and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void XorA()
    {
        Xor8(Regs.A);
    }

    /// <summary>
    /// "or b" operation (0xB0)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between B and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void OrB()
    {
        Or8(Regs.B);
    }

    /// <summary>
    /// "or c" operation (0xB1)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between C and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void OrC()
    {
        Or8(Regs.C);
    }

    /// <summary>
    /// "or d" operation (0xB2)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between D and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void OrD()
    {
        Or8(Regs.D);
    }

    /// <summary>
    /// "or e" operation (0xB3)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between E and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void OrE()
    {
        Or8(Regs.E);
    }

    /// <summary>
    /// "or h" operation (0xB4)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between H and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void OrH()
    {
        Or8(Regs.H);
    }

    /// <summary>
    /// "or l" operation (0xB5)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between L and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void OrL()
    {
        Or8(Regs.L);
    }

    /// <summary>
    /// "or (hl)" operation (0xB6)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between the byte at the memory address specified by the contents of HL and
    /// the byte contained in A; the result is stored in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void OrHLi()
    {
        Or8(ReadMemory(Regs.HL));
    }

    /// <summary>
    /// "or a" operation (0xB7)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between A and the byte contained in A; the result is stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void OrA()
    {
        Or8(Regs.A);
    }

    /// <summary>
    /// "cp b" operation (0xB8)
    /// </summary>
    /// <remarks>
    /// The contents of B are compared with the contents of A. If there is a true compare, the Z flag is set. The
    /// execution of this instruction does not affect A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void CpB()
    {
        Cp8(Regs.B);
    }

    /// <summary>
    /// "cp c" operation (0xB9)
    /// </summary>
    /// <remarks>
    /// The contents of C are compared with the contents of A. If there is a true compare, the Z flag is set. The
    /// execution of this instruction does not affect A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void CpC()
    {
        Cp8(Regs.C);
    }

    /// <summary>
    /// "cp d" operation (0xBA)
    /// </summary>
    /// <remarks>
    /// The contents of D are compared with the contents of A. If there is a true compare, the Z flag is set. The
    /// execution of this instruction does not affect A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void CpD()
    {
        Cp8(Regs.D);
    }

    /// <summary>
    /// "cp e" operation (0xBB)
    /// </summary>
    /// <remarks>
    /// The contents of E are compared with the contents of A. If there is a true compare, the Z flag is set. The
    /// execution of this instruction does not affect A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void CpE()
    {
        Cp8(Regs.E);
    }

    /// <summary>
    /// "cp h" operation (0xBC)
    /// </summary>
    /// <remarks>
    /// The contents of H are compared with the contents of A. If there is a true compare, the Z flag is set. The
    /// execution of this instruction does not affect A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void CpH()
    {
        Cp8(Regs.H);
    }

    /// <summary>
    /// "cp l" operation (0xBD)
    /// </summary>
    /// <remarks>
    /// The contents of L are compared with the contents of A. If there is a true compare, the Z flag is set. The
    /// execution of this instruction does not affect A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void CpL()
    {
        Cp8(Regs.L);
    }

    /// <summary>
    /// "cp (hl)" operation (0xBE)
    /// </summary>
    /// <remarks>
    /// The contents of the byte at the memory address specified by the contents of HL are compared with the contents
    /// of A. If there is a true compare, the Z flag is set. The execution of this instruction does not affect A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,hl:3
    /// </remarks>
    private void CpHLi()
    {
        Cp8(ReadMemory(Regs.HL));
    }

    /// <summary>
    /// "cp a" operation (0xBF)
    /// </summary>
    /// <remarks>
    /// The contents of A are compared with the contents of A. If there is a true compare, the Z flag is set. The
    /// execution of this instruction does not affect A.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void CpA()
    {
        Cp8(Regs.A);
    }

    /// <summary>
    /// "ret nz" operation (0xC0)
    /// </summary>
    /// <remarks>
    /// If Z flag is not set, the byte at the memory location specified by the contents of SP is moved to the low-order
    /// 8 bits of PC. SP is incremented and the byte at the memory location specified by the new contents of the SP are
    /// moved to the high-order eight bits of PC.The SP is incremented again. The next op code following this
    /// instruction is fetched from the memory location specified by the PC. This instruction is normally used to
    /// return to the main line program at the completion of a routine entered by a CALL instruction. If condition X is
    /// false, PC is simply incremented as usual, and the program continues with the next sequential instruction.
    /// 
    /// T-States:
    ///   If condition met: 11 (5, 3, 3)
    ///   Otherwise: 5
    /// Contention breakdown: pc:5,[sp:3,sp+1:3]
    /// </remarks>
    private void RetNZ()
    {
        TactPlus1(Regs.IR);
        if ((Regs.F & FlagsSetMask.Z) == 0)
        {
            Ret();
        }
    }

    /// <summary>
    /// "pop bc" operation (0xC1)
    /// </summary>
    /// <remarks>
    /// The top two bytes of the external memory last-in, first-out (LIFO) stack are popped to register pair BC. SP
    /// holds the 16-bit address of the current top of the stack. This instruction first loads to the low-order 
    /// portion of RR, the byte at the memory location corresponding to the contents of SP; then SP is incremented and
    /// the contents of the corresponding adjacent memory location are loaded to the high-order portion of RR and the
    /// SP is now incremented again.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,sp:3,sp+1:3
    /// </remarks>
    private void PopBC()
    {
        Regs.C = ReadMemory(Regs.SP);
        Regs.SP++;
        Regs.B = ReadMemory(Regs.SP);
        Regs.SP++;
    }

    /// <summary>
    /// "jp nz,NN" operation (0xC2)
    /// </summary>
    /// <remarks>
    /// If Z flag is not set, the instruction loads operand NN to PC, and the program continues with the instruction
    /// beginning at address NN. If condition X is false, PC is incremented as usual, and the program continues with
    /// the next sequential instruction.
    /// 
    /// T-States: 4, 3, 3 (10)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpNZ_NN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.Z) == 0)
        {
            Regs.PC = Regs.WZ;
        }
    }

    /// <summary>
    /// "jp NN" operation (0xC3)
    /// </summary>
    /// <remarks>
    /// Operand NN is loaded to PC. The next instruction is fetched from the location designated by the new contents
    /// of the PC.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpNN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        Regs.PC = Regs.WZ;
    }

    /// <summary>
    /// "call nz,NN" operation (0xC4)
    /// </summary>
    /// <remarks>
    /// If flag Z is not set, this instruction pushes the current contents of PC onto the top of the external memory
    /// stack, then loads the operands NN to PC to point to the address in memory at which the first op code of a
    /// subroutine is to be fetched. At the end of the subroutine, a RET instruction can be used to return to the
    /// original program flow by popping the top of the stack back to PC. If condition X is false, PC is incremented as
    /// usual, and the program continues with the next sequential instruction. The stack push is accomplished by first
    /// decrementing the current contents of SP, loading the high-order byte of the PC contents to the memory address
    /// now pointed to by SP; then decrementing SP again, and loading the low-order byte of the PC contents to the top
    /// of the stack.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3]
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,[1,sp-1:3,sp-2:3]
    /// </remarks>
    private void CallNZ()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.Z) == 0)
        {
            CallCore();
        }
    }

    /// <summary>
    /// "push bc" operation (0xC5)
    /// </summary>
    /// <remarks>
    /// The contents of the register pair BC are pushed to the external  memory last-in, first-out (LIFO) stack. SP
    /// holds the 16-bit address of the current top of the Stack. This instruction first decrements SP and loads the
    /// high-order byte of register pair BC to the memory address specified by SP. Then SP is decremented again and
    /// loads the low-order byte of BC to the memory location corresponding to this new address in SP.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void PushBC()
    {
        TactPlus1(Regs.IR);
        Regs.SP--;
        WriteMemory(Regs.SP, Regs.B);
        Regs.SP--;
        WriteMemory(Regs.SP, Regs.C);
    }

    /// <summary>
    /// "add a,N" operation (0xC6)
    /// </summary>
    /// <remarks>
    /// The N integer is added to the contents of the Accumulator, and the results are stored in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// </remarks>
    private void AddAN()
    {
        Add8(FetchCodeByte());
    }

    /// <summary>
    /// "rst 00h" operation (0xC7)
    /// </summary>
    /// <remarks>
    /// The current PC contents are pushed onto the external memory stack, and 0 is loaded to PC. Program execution
    /// then begins with the op code in the address now pointed to by PC. The push is performed by first decrementing
    /// the contents of SP, loading the high-order byte of PC to the memory address now pointed to by SP, decrementing
    /// SP again, and loading the low-order byte of PC to the address now pointed to by SP. The Restart instruction
    /// allows for a jump to address 0000H. Because all addresses are stored in Page 0 of memory, the high-order byte
    /// of PC is loaded with 0x00.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void Rst00()
    {
        RstCore(0x0000);
    }

    /// <summary>
    /// "ret z" operation (0xC8)
    /// </summary>
    /// <remarks>
    /// If Z flag is set, the byte at the memory location specified by the contents of SP is moved to the low-order
    /// 8 bits of PC. SP is incremented and the byte at the memory location specified by the new contents of the SP are
    /// moved to the high-order eight bits of PC.The SP is incremented again. The next op code following this
    /// instruction is fetched from the memory location specified by the PC. This instruction is normally used to
    /// return to the main line program at the completion of a routine entered by a CALL instruction. If condition X is
    /// false, PC is simply incremented as usual, and the program continues with the next sequential instruction.
    /// 
    /// T-States:
    ///   If condition met: 11 (5, 3, 3)
    ///   Otherwise: 5
    /// Contention breakdown: pc:5,[sp:3,sp+1:3]
    /// </remarks>
    private void RetZ()
    {
        TactPlus1(Regs.IR);
        if ((Regs.F & FlagsSetMask.Z) != 0)
        {
            Ret();
        }
    }

    /// <summary>
    /// "ret" operation (0xC9)
    /// </summary>
    /// <remarks>
    /// The byte at the memory location specified by the contents of SP is moved to the low-order eight bits of PC. SP
    /// is now incremented and the byte at the memory location specified by the new contents of this instruction is
    /// fetched from the memory location specified by PC. This instruction is normally used to return to the main line
    /// program at the completion of a routine entered by a CALL instruction.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,sp:3,sp+1:3
    /// </remarks>
    private void Ret()
    {
        Regs.WL = ReadMemory(Regs.SP);
        Regs.SP++;
        Regs.WH = ReadMemory(Regs.SP);
        Regs.SP++;
        Regs.PC = Regs.WZ;
    }

    /// <summary>
    /// "jp z,NN" operation (0xCA)
    /// </summary>
    /// <remarks>
    /// If Z flag is set, the instruction loads operand NN to PC, and the program continues with the instruction
    /// beginning at address NN. If condition X is false, PC is incremented as usual, and the program continues with
    /// the next sequential instruction.
    /// 
    /// T-States: 4, 3, 3 (10)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpZ_NN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.Z) != 0)
        {
            Regs.PC = Regs.WZ;
        }
    }

    /// <summary>
    /// "call z,NN" operation (0xCC)
    /// </summary>
    /// <remarks>
    /// If flag Z is set, this instruction pushes the current contents of PC onto the top of the external memory
    /// stack, then loads the operands NN to PC to point to the address in memory at which the first op code of a
    /// subroutine is to be fetched. At the end of the subroutine, a RET instruction can be used to return to the
    /// original program flow by popping the top of the stack back to PC. If condition X is false, PC is incremented as
    /// usual, and the program continues with the next sequential instruction. The stack push is accomplished by first
    /// decrementing the current contents of SP, loading the high-order byte of the PC contents to the memory address
    /// now pointed to by SP; then decrementing SP again, and loading the low-order byte of the PC contents to the top
    /// of the stack.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3]
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,[1,sp-1:3,sp-2:3]
    /// </remarks>
    private void CallZ()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.Z) != 0)
        {
            CallCore();
        }
    }

    /// <summary>
    /// "call NN" operation (0xCD)
    /// </summary>
    /// <remarks>
    /// The current contents of PC are pushed onto the top of the external memory stack. The operands NN are then
    /// loaded to PC to point to the address in memory at which the first op code of a subroutine is to be fetched.
    /// At the end of the subroutine, a RET instruction can be used to return to the original program flow by  popping
    /// the top of the stack back to PC. The push is accomplished by first decrementing the current contents of SP,
    /// loading the high-order byte of the PC contents to the memory address now pointed to by SP; then decrementing
    /// SP again, and loading the low-order byte of the PC contents to the top of stack.
    /// 
    /// T-States: 17, (4, 3, 4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,pc+2:1,sp-1:3,sp-2:3
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,1,sp-1:3,sp-2:3
    /// </remarks>
    private void CallNN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        CallCore();
    }

    /// <summary>
    /// "adc a,N" operation (0xCE)
    /// </summary>
    /// <remarks>
    /// The N integer, along with the Carry Flag is added to the contents of the Accumulator, and the results are stored
    /// in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 7; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// </remarks>
    private void AdcAN()
    {
        Adc8(FetchCodeByte());
    }

    /// <summary>
    /// "rst 08h" operation (0xCF)
    /// </summary>
    /// <remarks>
    /// The current PC contents are pushed onto the external memory stack, and $08 is loaded to PC. Program execution
    /// then begins with the op code in the address now pointed to by PC. The push is performed by first decrementing
    /// the contents of SP, loading the high-order byte of PC to the memory address now pointed to by SP, decrementing
    /// SP again, and loading the low-order byte of PC to the address now pointed to by SP. The Restart instruction
    /// allows for a jump to address 0008H. Because all addresses are stored in Page 0 of memory, the high-order byte
    /// of PC is loaded with 0x0008.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void Rst08()
    {
        RstCore(0x0008);
    }

    /// <summary>
    /// "ret nc" operation (0xD0)
    /// </summary>
    /// <remarks>
    /// If C flag is not set, the byte at the memory location specified by the contents of SP is moved to the low-order
    /// 8 bits of PC. SP is incremented and the byte at the memory location specified by the new contents of the SP are
    /// moved to the high-order eight bits of PC.The SP is incremented again. The next op code following this
    /// instruction is fetched from the memory location specified by the PC. This instruction is normally used to
    /// return to the main line program at the completion of a routine entered by a CALL instruction. If condition X is
    /// false, PC is simply incremented as usual, and the program continues with the next sequential instruction.
    /// 
    /// T-States:
    ///   If condition met: 11 (5, 3, 3)
    ///   Otherwise: 5
    /// Contention breakdown: pc:5,[sp:3,sp+1:3]
    /// </remarks>
    private void RetNC()
    {
        TactPlus1(Regs.IR);
        if ((Regs.F & FlagsSetMask.C) == 0)
        {
            Ret();
        }
    }

    /// <summary>
    /// "pop de" operation (0xD1)
    /// </summary>
    /// <remarks>
    /// The top two bytes of the external memory last-in, first-out (LIFO) stack are popped to register pair DE. SP
    /// holds the 16-bit address of the current top of the stack. This instruction first loads to the low-order 
    /// portion of RR, the byte at the memory location corresponding to the contents of SP; then SP is incremented and
    /// the contents of the corresponding adjacent memory location are loaded to the high-order portion of RR and the
    /// SP is now incremented again.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,sp:3,sp+1:3
    /// </remarks>
    private void PopDE()
    {
        Regs.E = ReadMemory(Regs.SP);
        Regs.SP++;
        Regs.D = ReadMemory(Regs.SP);
        Regs.SP++;
    }

    /// <summary>
    /// "jp nc,NN" operation (0xD2)
    /// </summary>
    /// <remarks>
    /// If C flag is not set, the instruction loads operand NN to PC, and the program continues with the instruction
    /// beginning at address NN. If condition X is false, PC is incremented as usual, and the program continues with
    /// the next sequential instruction.
    /// 
    /// T-States: 4, 3, 3 (10)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpNC_NN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.C) == 0)
        {
            Regs.PC = Regs.WZ;
        }
    }

    /// <summary>
    /// "out (N),a" operation (0xD3)
    /// </summary>
    /// <remarks>
    /// The operand N is placed on the bottom half (A0 through A7) of the address bus to select the I/O device at one
    /// of 256 possible ports. The contents of A also appear on the top half(A8 through A15) of the address bus at this
    /// time. Then the byte contained in A is placed on the data bus and written to the selected peripheral device.
    /// 
    /// T-States: 11 (4, 3, 4)
    /// Contention breakdown: pc:4,pc+1:3,I/O
    /// </remarks>
    private void OutNA()
    {
        var nn = FetchCodeByte();
        var port = (ushort)(nn | (Regs.A << 8));
        Regs.WH = Regs.A;
        Regs.WL = (byte)(nn + 1);
        WritePort(port, Regs.A);
    }

    /// <summary>
    /// "call nc,NN" operation (0xD4)
    /// </summary>
    /// <remarks>
    /// If flag C is not set, this instruction pushes the current contents of PC onto the top of the external memory
    /// stack, then loads the operands NN to PC to point to the address in memory at which the first op code of a
    /// subroutine is to be fetched. At the end of the subroutine, a RET instruction can be used to return to the
    /// original program flow by popping the top of the stack back to PC. If condition X is false, PC is incremented as
    /// usual, and the program continues with the next sequential instruction. The stack push is accomplished by first
    /// decrementing the current contents of SP, loading the high-order byte of the PC contents to the memory address
    /// now pointed to by SP; then decrementing SP again, and loading the low-order byte of the PC contents to the top
    /// of the stack.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3]
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,[1,sp-1:3,sp-2:3]
    /// </remarks>
    private void CallNC()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.C) == 0)
        {
            CallCore();
        }
    }

    /// <summary>
    /// "push de" operation (0xD5)
    /// </summary>
    /// <remarks>
    /// The contents of the register pair DE are pushed to the external  memory last-in, first-out (LIFO) stack. SP
    /// holds the 16-bit address of the current top of the Stack. This instruction first decrements SP and loads the
    /// high-order byte of register pair BC to the memory address specified by SP. Then SP is decremented again and
    /// loads the low-order byte of BC to the memory location corresponding to this new address in SP.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void PushDE()
    {
        TactPlus1(Regs.IR);
        Regs.SP--;
        WriteMemory(Regs.SP, Regs.D);
        Regs.SP--;
        WriteMemory(Regs.SP, Regs.E);
    }

    /// <summary>
    /// "sub N" operation (0xD6)
    /// </summary>
    /// <remarks>
    /// The N integer is subtracted from the contents of the Accumulator, and the results are stored in the
    /// Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// </remarks>
    private void SubAN()
    {
        Sub8(FetchCodeByte());
    }

    /// <summary>
    /// "rst 10h" operation (0xD7)
    /// </summary>
    /// <remarks>
    /// The current PC contents are pushed onto the external memory stack, and $10 is loaded to PC. Program execution
    /// then begins with the op code in the address now pointed to by PC. The push is performed by first decrementing
    /// the contents of SP, loading the high-order byte of PC to the memory address now pointed to by SP, decrementing
    /// SP again, and loading the low-order byte of PC to the address now pointed to by SP. The Restart instruction
    /// allows for a jump to address 0010H. Because all addresses are stored in Page 0 of memory, the high-order byte
    /// of PC is loaded with 0x0010.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void Rst10()
    {
        RstCore(0x0010);
    }

    /// <summary>
    /// "ret c" operation (0xD8)
    /// </summary>
    /// <remarks>
    /// If C flag is set, the byte at the memory location specified by the contents of SP is moved to the low-order
    /// 8 bits of PC. SP is incremented and the byte at the memory location specified by the new contents of the SP are
    /// moved to the high-order eight bits of PC.The SP is incremented again. The next op code following this
    /// instruction is fetched from the memory location specified by the PC. This instruction is normally used to
    /// return to the main line program at the completion of a routine entered by a CALL instruction. If condition X is
    /// false, PC is simply incremented as usual, and the program continues with the next sequential instruction.
    /// 
    /// T-States:
    ///   If condition met: 11 (5, 3, 3)
    ///   Otherwise: 5
    /// Contention breakdown: pc:5,[sp:3,sp+1:3]
    /// </remarks>
    private void RetC()
    {
        TactPlus1(Regs.IR);
        if ((Regs.F & FlagsSetMask.C) != 0)
        {
            Ret();
        }
    }

    /// <summary>
    /// "exx" operation (0xD9)
    /// </summary>
    /// <remarks>
    /// Each 2-byte value in register pairs BC, DE, and HL is exchanged with the 2-byte value in BC', DE', and HL', 
    /// respectively.
    /// 
    /// T-States: 4, (4)
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Exx()
    {
        Regs.ExchangeRegisterSet();
    }

    /// <summary>
    /// "jp c,NN" operation (0xDA)
    /// </summary>
    /// <remarks>
    /// If C flag is set, the instruction loads operand NN to PC, and the program continues with the instruction
    /// beginning at address NN. If condition X is false, PC is incremented as usual, and the program continues with
    /// the next sequential instruction.
    /// 
    /// T-States: 4, 3, 3 (10)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpC_NN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.C) != 0)
        {
            Regs.PC = Regs.WZ;
        }
    }

    /// <summary>
    /// "in a,(N)" operation (0xDB)
    /// </summary>
    /// <remarks>
    /// The operand N is placed on the bottom half (A0 through A7) of the address bus to select the I/O device at one
    /// of 256 possible ports. The contents of A also appear on the top half (A8 through A15) of the address bus at
    /// this time. Then one byte from the selected port is placed on the data bus and written to A in the CPU.
    /// 
    /// T-States: 11 (4, 3, 4)
    /// Contention breakdown: pc:4,pc+1:3,I/O
    /// </remarks>
    private void InAN()
    {
        var inTemp = (ushort)(FetchCodeByte() | (Regs.A << 8));
        Regs.A = ReadPort(inTemp);
        Regs.WZ = (ushort)(inTemp + 1);
    }

    /// <summary>
    /// "call c,NN" operation (0xDC)
    /// </summary>
    /// <remarks>
    /// If flag C is set, this instruction pushes the current contents of PC onto the top of the external memory
    /// stack, then loads the operands NN to PC to point to the address in memory at which the first op code of a
    /// subroutine is to be fetched. At the end of the subroutine, a RET instruction can be used to return to the
    /// original program flow by popping the top of the stack back to PC. If condition X is false, PC is incremented as
    /// usual, and the program continues with the next sequential instruction. The stack push is accomplished by first
    /// decrementing the current contents of SP, loading the high-order byte of the PC contents to the memory address
    /// now pointed to by SP; then decrementing SP again, and loading the low-order byte of the PC contents to the top
    /// of the stack.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3]
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,[1,sp-1:3,sp-2:3]
    /// </remarks>
    private void CallC()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.C) != 0)
        {
            CallCore();
        }
    }

    /// <summary>
    /// "sbc a,N" operation (0xDE)
    /// </summary>
    /// <remarks>
    /// The N integer, along with the Carry flag is subtracted from the contents of the Accumulator, and the results
    /// are stored in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// </remarks>
    private void SbcAN()
    {
        Sbc8(FetchCodeByte());
    }

    /// <summary>
    /// "rst 18h" operation (0xDF)
    /// </summary>
    /// <remarks>
    /// The current PC contents are pushed onto the external memory stack, and $18 is loaded to PC. Program execution
    /// then begins with the op code in the address now pointed to by PC. The push is performed by first decrementing
    /// the contents of SP, loading the high-order byte of PC to the memory address now pointed to by SP, decrementing
    /// SP again, and loading the low-order byte of PC to the address now pointed to by SP. The Restart instruction
    /// allows for a jump to address 0018H. Because all addresses are stored in Page 0 of memory, the high-order byte
    /// of PC is loaded with 0x0018.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void Rst18()
    {
        RstCore(0x0018);
    }

    /// <summary>
    /// "ret po" operation (0xE0)
    /// </summary>
    /// <remarks>
    /// If P/V flag is not set, the byte at the memory location specified by the contents of SP is moved to the low-order
    /// 8 bits of PC. SP is incremented and the byte at the memory location specified by the new contents of the SP are
    /// moved to the high-order eight bits of PC.The SP is incremented again. The next op code following this
    /// instruction is fetched from the memory location specified by the PC. This instruction is normally used to
    /// return to the main line program at the completion of a routine entered by a CALL instruction. If condition X is
    /// false, PC is simply incremented as usual, and the program continues with the next sequential instruction.
    /// 
    /// T-States:
    ///   If condition met: 11 (5, 3, 3)
    ///   Otherwise: 5
    /// Contention breakdown: pc:5,[sp:3,sp+1:3]
    /// </remarks>
    private void RetPO()
    {
        TactPlus1(Regs.IR);
        if ((Regs.F & FlagsSetMask.PV) == 0)
        {
            Ret();
        }
    }

    /// <summary>
    /// "pop hl" operation (0xE1)
    /// </summary>
    /// <remarks>
    /// The top two bytes of the external memory last-in, first-out (LIFO) stack are popped to register pair HL. SP
    /// holds the 16-bit address of the current top of the stack. This instruction first loads to the low-order 
    /// portion of RR, the byte at the memory location corresponding to the contents of SP; then SP is incremented and
    /// the contents of the corresponding adjacent memory location are loaded to the high-order portion of RR and the
    /// SP is now incremented again.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,sp:3,sp+1:3
    /// </remarks>
    private void PopHL()
    {
        Regs.L = ReadMemory(Regs.SP);
        Regs.SP++;
        Regs.H = ReadMemory(Regs.SP);
        Regs.SP++;
    }

    /// <summary>
    /// "jp po,NN" operation (0xE2)
    /// </summary>
    /// <remarks>
    /// If P/V flag is not set, the instruction loads operand NN to PC, and the program continues with the instruction
    /// beginning at address NN. If condition X is false, PC is incremented as usual, and the program continues with
    /// the next sequential instruction.
    /// 
    /// T-States: 4, 3, 3 (10)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpPO_NN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.PV) == 0)
        {
            Regs.PC = Regs.WZ;
        }
    }

    /// <summary>
    /// "ex (sp),hl" operation (0xE3)
    /// </summary>
    /// <remarks>
    /// The low-order byte contained in HL is exchanged with the contents of the memory address specified by the
    /// contents of SP, and the high-order byte of HL is exchanged with the next highest memory address (SP+1).
    /// 
    /// T-States: 19 (4, 3, 4, 3, 5)
    /// Contention breakdown: pc:4,sp:3,sp+1:3,sp+1:1,sp+1(write):3,sp(write):3,sp(write):1 ×2
    /// Gate array contention breakdown: pc:4,sp:3,sp+1:4,sp+1(write):3,sp(write):5
    /// </remarks>
    private void ExSPiHL()
    {
        var sp1 = (ushort)(Regs.SP + 1);
        var tempL = ReadMemory(Regs.SP);
        var tempH = ReadMemory(sp1);
        TactPlus1(Regs.SP);
        WriteMemory(sp1, Regs.H);
        WriteMemory(Regs.SP, Regs.L);
        TactPlus2Write(Regs.SP);
        Regs.WL = tempL;
        Regs.WH = tempH;
        Regs.HL = Regs.WZ;
    }

    /// <summary>
    /// "call po,NN" operation (0xE4)
    /// </summary>
    /// <remarks>
    /// If flag P/V is not set, this instruction pushes the current contents of PC onto the top of the external memory
    /// stack, then loads the operands NN to PC to point to the address in memory at which the first op code of a
    /// subroutine is to be fetched. At the end of the subroutine, a RET instruction can be used to return to the
    /// original program flow by popping the top of the stack back to PC. If condition X is false, PC is incremented as
    /// usual, and the program continues with the next sequential instruction. The stack push is accomplished by first
    /// decrementing the current contents of SP, loading the high-order byte of the PC contents to the memory address
    /// now pointed to by SP; then decrementing SP again, and loading the low-order byte of the PC contents to the top
    /// of the stack.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3]
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,[1,sp-1:3,sp-2:3]
    /// </remarks>
    private void CallPO()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.PV) == 0)
        {
            CallCore();
        }
    }

    /// <summary>
    /// "push hl" operation (0xE5)
    /// </summary>
    /// <remarks>
    /// The contents of the register pair HL are pushed to the external  memory last-in, first-out (LIFO) stack. SP
    /// holds the 16-bit address of the current top of the Stack. This instruction first decrements SP and loads the
    /// high-order byte of register pair HL to the memory address specified by SP. Then SP is decremented again and
    /// loads the low-order byte of HL to the memory location corresponding to this new address in SP.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void PushHL()
    {
        TactPlus1(Regs.IR);
        Regs.SP--;
        WriteMemory(Regs.SP, Regs.H);
        Regs.SP--;
        WriteMemory(Regs.SP, Regs.L);
    }

    /// <summary>
    /// "and N" operation (0xE6)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is performed between 8-bit integer N and the byte contained in the Accumulator; the
    /// result is stored in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set.
    /// P/V is reset if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// </remarks>
    private void AndAN()
    {
        And8(FetchCodeByte());
    }

    /// <summary>
    /// "rst 20h" operation (0xE7)
    /// </summary>
    /// <remarks>
    /// The current PC contents are pushed onto the external memory stack, and $20 is loaded to PC. Program execution
    /// then begins with the op code in the address now pointed to by PC. The push is performed by first decrementing
    /// the contents of SP, loading the high-order byte of PC to the memory address now pointed to by SP, decrementing
    /// SP again, and loading the low-order byte of PC to the address now pointed to by SP. The Restart instruction
    /// allows for a jump to address 0020H. Because all addresses are stored in Page 0 of memory, the high-order byte
    /// of PC is loaded with 0x0020.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void Rst20()
    {
        RstCore(0x0020);
    }

    /// <summary>
    /// "ret pe" operation (0xE8)
    /// </summary>
    /// <remarks>
    /// If P/V flag is set, the byte at the memory location specified by the contents of SP is moved to the low-order
    /// 8 bits of PC. SP is incremented and the byte at the memory location specified by the new contents of the SP are
    /// moved to the high-order eight bits of PC.The SP is incremented again. The next op code following this
    /// instruction is fetched from the memory location specified by the PC. This instruction is normally used to
    /// return to the main line program at the completion of a routine entered by a CALL instruction. If condition X is
    /// false, PC is simply incremented as usual, and the program continues with the next sequential instruction.
    /// 
    /// T-States:
    ///   If condition met: 11 (5, 3, 3)
    ///   Otherwise: 5
    /// Contention breakdown: pc:5,[sp:3,sp+1:3]
    /// </remarks>
    private void RetPE()
    {
        TactPlus1(Regs.IR);
        if ((Regs.F & FlagsSetMask.PV) != 0)
        {
            Ret();
        }
    }

    /// <summary>
    /// "jp (hl)" operation (0xE9)
    /// </summary>
    /// <remarks>
    /// PC is loaded with the contents of HL. The next instruction is fetched from the location designated by the new
    /// contents of PC.
    /// 
    /// T-States: 4 (4)
    /// Contention breakdown: pc:4
    /// </remarks>
    private void JpHL()
    {
        Regs.PC = Regs.HL;
    }

    /// <summary>
    /// "jp pe,NN" operation (0xEA)
    /// </summary>
    /// <remarks>
    /// If P/V flag is set, the instruction loads operand NN to PC, and the program continues with the instruction
    /// beginning at address NN. If condition X is false, PC is incremented as usual, and the program continues with
    /// the next sequential instruction.
    /// 
    /// T-States: 4, 3, 3 (10)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpPE_NN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.PV) != 0)
        {
            Regs.PC = Regs.WZ;
        }
    }

    /// <summary>
    /// "ex de,hl" operation (0xEB)
    /// </summary>
    /// <remarks>
    /// The 2-byte contents of register pairs DE and HL are exchanged.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void ExDEHL()
    {
        Registers.Swap(ref Regs.DE, ref Regs.HL);
    }

    /// <summary>
    /// "call pe,NN" operation (0xEC)
    /// </summary>
    /// <remarks>
    /// If flag P/V is set, this instruction pushes the current contents of PC onto the top of the external memory
    /// stack, then loads the operands NN to PC to point to the address in memory at which the first op code of a
    /// subroutine is to be fetched. At the end of the subroutine, a RET instruction can be used to return to the
    /// original program flow by popping the top of the stack back to PC. If condition X is false, PC is incremented as
    /// usual, and the program continues with the next sequential instruction. The stack push is accomplished by first
    /// decrementing the current contents of SP, loading the high-order byte of the PC contents to the memory address
    /// now pointed to by SP; then decrementing SP again, and loading the low-order byte of the PC contents to the top
    /// of the stack.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3]
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,[1,sp-1:3,sp-2:3]
    /// </remarks>
    private void CallPE()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.PV) != 0)
        {
            CallCore();
        }
    }

    /// <summary>
    /// "xor N" operation (0xEE)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is performed between 8-bit integer N and the byte contained in the Accumulator; the
    /// result is stored in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// </remarks>
    private void XorAN()
    {
        Xor8(FetchCodeByte());
    }

    /// <summary>
    /// "rst 28h" operation (0xEF)
    /// </summary>
    /// <remarks>
    /// The current PC contents are pushed onto the external memory stack, and $28 is loaded to PC. Program execution
    /// then begins with the op code in the address now pointed to by PC. The push is performed by first decrementing
    /// the contents of SP, loading the high-order byte of PC to the memory address now pointed to by SP, decrementing
    /// SP again, and loading the low-order byte of PC to the address now pointed to by SP. The Restart instruction
    /// allows for a jump to address 0028H. Because all addresses are stored in Page 0 of memory, the high-order byte
    /// of PC is loaded with 0x0028.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void Rst28()
    {
        RstCore(0x0028);
    }

    /// <summary>
    /// "ret p" operation (0xF0)
    /// </summary>
    /// <remarks>
    /// If S flag is not set, the byte at the memory location specified by the contents of SP is moved to the low-order
    /// 8 bits of PC. SP is incremented and the byte at the memory location specified by the new contents of the SP are
    /// moved to the high-order eight bits of PC.The SP is incremented again. The next op code following this
    /// instruction is fetched from the memory location specified by the PC. This instruction is normally used to
    /// return to the main line program at the completion of a routine entered by a CALL instruction. If condition X is
    /// false, PC is simply incremented as usual, and the program continues with the next sequential instruction.
    /// 
    /// T-States:
    ///   If condition met: 11 (5, 3, 3)
    ///   Otherwise: 5
    /// Contention breakdown: pc:5,[sp:3,sp+1:3]
    /// </remarks>
    private void RetP()
    {
        TactPlus1(Regs.IR);
        if ((Regs.F & FlagsSetMask.S) == 0)
        {
            Ret();
        }
    }

    /// <summary>
    /// "pop af" operation (0xF1)
    /// </summary>
    /// <remarks>
    /// The top two bytes of the external memory last-in, first-out (LIFO) stack are popped to register pair AF. SP
    /// holds the 16-bit address of the current top of the stack. This instruction first loads to the low-order 
    /// portion of RR, the byte at the memory location corresponding to the contents of SP; then SP is incremented and
    /// the contents of the corresponding adjacent memory location are loaded to the high-order portion of RR and the
    /// SP is now incremented again.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,sp:3,sp+1:3
    /// </remarks>
    private void PopAF()
    {
        Regs.F = ReadMemory(Regs.SP);
        Regs.SP++;
        Regs.A = ReadMemory(Regs.SP);
        Regs.SP++;
    }

    /// <summary>
    /// "jp p,NN" operation (0xF2)
    /// </summary>
    /// <remarks>
    /// If S flag is not set, the instruction loads operand NN to PC, and the program continues with the instruction
    /// beginning at address NN. If condition X is false, PC is incremented as usual, and the program continues with
    /// the next sequential instruction.
    /// 
    /// T-States: 4, 3, 3 (10)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpP_NN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.S) == 0)
        {
            Regs.PC = Regs.WZ;
        }
    }

    /// <summary>
    /// "di" operation (0xF3)
    /// </summary>
    /// <remarks>
    /// Disables the maskable interrupt by resetting the interrupt enable flip-flops (IFF1 and IFF2).
    ///
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Di()
    {
        Iff2 = Iff1 = false;
    }

    /// <summary>
    /// "call p,NN" operation (0xF4)
    /// </summary>
    /// <remarks>
    /// If flag S is not set, this instruction pushes the current contents of PC onto the top of the external memory
    /// stack, then loads the operands NN to PC to point to the address in memory at which the first op code of a
    /// subroutine is to be fetched. At the end of the subroutine, a RET instruction can be used to return to the
    /// original program flow by popping the top of the stack back to PC. If condition X is false, PC is incremented as
    /// usual, and the program continues with the next sequential instruction. The stack push is accomplished by first
    /// decrementing the current contents of SP, loading the high-order byte of the PC contents to the memory address
    /// now pointed to by SP; then decrementing SP again, and loading the low-order byte of the PC contents to the top
    /// of the stack.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3]
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,[1,sp-1:3,sp-2:3]
    /// </remarks>
    private void CallP()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.S) == 0)
        {
            CallCore();
        }
    }

    /// <summary>
    /// "push af" operation (0xF5)
    /// </summary>
    /// <remarks>
    /// The contents of the register pair AF are pushed to the external  memory last-in, first-out (LIFO) stack. SP
    /// holds the 16-bit address of the current top of the Stack. This instruction first decrements SP and loads the
    /// high-order byte of register pair BC to the memory address specified by SP. Then SP is decremented again and
    /// loads the low-order byte of BC to the memory location corresponding to this new address in SP.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void PushAF()
    {
        TactPlus1(Regs.IR);
        Regs.SP--;
        WriteMemory(Regs.SP, Regs.A);
        Regs.SP--;
        WriteMemory(Regs.SP, Regs.F);
    }

    /// <summary>
    /// "or N" operation (0xF6)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is performed between 8-bit integer N and the byte contained in the Accumulator; the
    /// result is stored in the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is reset.
    /// C is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// </remarks>
    private void OrAN()
    {
        Or8(FetchCodeByte());
    }

    /// <summary>
    /// "rst 30h" operation (0xF7)
    /// </summary>
    /// <remarks>
    /// The current PC contents are pushed onto the external memory stack, and $30 is loaded to PC. Program execution
    /// then begins with the op code in the address now pointed to by PC. The push is performed by first decrementing
    /// the contents of SP, loading the high-order byte of PC to the memory address now pointed to by SP, decrementing
    /// SP again, and loading the low-order byte of PC to the address now pointed to by SP. The Restart instruction
    /// allows for a jump to address 0030H. Because all addresses are stored in Page 0 of memory, the high-order byte
    /// of PC is loaded with 0x0030.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void Rst30()
    {
        RstCore(0x0030);
    }

    /// <summary>
    /// "ret m" operation (0xF8)
    /// </summary>
    /// <remarks>
    /// If S flag is set, the byte at the memory location specified by the contents of SP is moved to the low-order
    /// 8 bits of PC. SP is incremented and the byte at the memory location specified by the new contents of the SP are
    /// moved to the high-order eight bits of PC.The SP is incremented again. The next op code following this
    /// instruction is fetched from the memory location specified by the PC. This instruction is normally used to
    /// return to the main line program at the completion of a routine entered by a CALL instruction. If condition X is
    /// false, PC is simply incremented as usual, and the program continues with the next sequential instruction.
    /// 
    /// T-States:
    ///   If condition met: 11 (5, 3, 3)
    ///   Otherwise: 5
    /// Contention breakdown: pc:5,[sp:3,sp+1:3]
    /// </remarks>
    private void RetM()
    {
        TactPlus1(Regs.IR);
        if ((Regs.F & FlagsSetMask.S) != 0)
        {
            Ret();
        }
    }

    /// <summary>
    /// "ld sp,hl" operation (0xF9)
    /// </summary>
    /// <remarks>
    /// The contents of HL are loaded to SP.
    /// 
    /// T-States: 4 (6)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void LdSPHL()
    {
        TactPlus2(Regs.IR);
        Regs.SP = Regs.HL;
    }

    /// <summary>
    /// "jp m,NN" operation (0xFA)
    /// </summary>
    /// <remarks>
    /// If S flag is set, the instruction loads operand NN to PC, and the program continues with the instruction
    /// beginning at address NN. If condition X is false, PC is incremented as usual, and the program continues with
    /// the next sequential instruction.
    /// 
    /// T-States: 4, 3, 3 (10)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void JpM_NN()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.S) != 0)
        {
            Regs.PC = Regs.WZ;
        }
    }


    /// <summary>
    /// "ei" operation (0xFB)
    /// </summary>
    /// <remarks>
    /// Sets both interrupt enable flip flops (IFFI and IFF2) to a logic 1 value, allowing recognition of any maskable
    /// interrupt.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Ei()
    {
        Iff2 = Iff1 = true;
        EiBacklog = 2;
    }

    /// <summary>
    /// "call m,NN" operation (0xFC)
    /// </summary>
    /// <remarks>
    /// If flag S is set, this instruction pushes the current contents of PC onto the top of the external memory
    /// stack, then loads the operands NN to PC to point to the address in memory at which the first op code of a
    /// subroutine is to be fetched. At the end of the subroutine, a RET instruction can be used to return to the
    /// original program flow by popping the top of the stack back to PC. If condition X is false, PC is incremented as
    /// usual, and the program continues with the next sequential instruction. The stack push is accomplished by first
    /// decrementing the current contents of SP, loading the high-order byte of the PC contents to the memory address
    /// now pointed to by SP; then decrementing SP again, and loading the low-order byte of the PC contents to the top
    /// of the stack.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3]
    /// Gate array contention breakdown: pc:4,pc+1:3,pc+2:3,[1,sp-1:3,sp-2:3]
    /// </remarks>
    private void CallM()
    {
        Regs.WL = FetchCodeByte();
        Regs.WH = FetchCodeByte();
        if ((Regs.F & FlagsSetMask.S) != 0)
        {
            CallCore();
        }
    }

    /// <summary>
    /// "cp N" operation (0xFE)
    /// </summary>
    /// <remarks>
    /// The contents of the byte N are compared with the contents of the Accumulator. If there is a true compare, the Z
    /// flag is set.The execution of this instruction does not affect the contents of the Accumulator.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4; otherwise, it is reset.
    /// P/V is set if overflow; otherwise, it is reset.
    /// N is set.
    /// C is set if borrow; otherwise, it is reset.
    /// 
    /// T-States: 7 (4, 3)
    /// </remarks>
    private void CpAN()
    {
        Cp8(FetchCodeByte());
    }

    /// <summary>
    /// "rst 38h" operation (0xFF)
    /// </summary>
    /// <remarks>
    /// The current PC contents are pushed onto the external memory stack, and $38 is loaded to PC. Program execution
    /// then begins with the op code in the address now pointed to by PC. The push is performed by first decrementing
    /// the contents of SP, loading the high-order byte of PC to the memory address now pointed to by SP, decrementing
    /// SP again, and loading the low-order byte of PC to the address now pointed to by SP. The Restart instruction
    /// allows for a jump to address 0038H. Because all addresses are stored in Page 0 of memory, the high-order byte
    /// of PC is loaded with 0x0038.
    /// 
    /// T-States: 11 (5, 3, 3)
    /// Contention breakdown: pc:5,sp-1:3,sp-2:3
    /// </remarks>
    private void Rst38()
    {
        RstCore(0x0038);
    }

}