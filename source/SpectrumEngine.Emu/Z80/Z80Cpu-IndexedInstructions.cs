namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition contains the code for processing IX- or IY-indexed Z80 instructions (with `$DD` or `$FD` prefix).
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// This array contains the 256 function references, each executing a  standard Z80 instruction.
    /// </summary>
    private Action[]? _indexedInstrs;

    /// <summary>
    /// Initialize the table of standard instructions.
    /// </summary>
    private void InitializeIndexedInstructionsTable()
    {
        _indexedInstrs = new Action[]
        {
            Nop,        LdBCNN,     LdBCiA,     IncBC,      IncB,       DecB,       LdBN,       Rlca,       // 00-07
            ExAF,       AddIXBC,    LdABCi,     DecBC,      IncC,       DecC,       LdCN,       Rrca,       // 08-0f
            Djnz,       LdDENN,     LdDEiA,     IncDE,      IncD,       DecD,       LdDN,       Rla,        // 10-17
            JrE,        AddIXDE,    LdADEi,     DecDE,      IncE,       DecE,       LdEN,       Rra,        // 18-1f
            JrNZ,       LdIXNN,     LdNNiIX,    IncIX,      IncXH,      DecXH,      LdXHN,      Daa,        // 20-27
            JrZ,        AddIXIX,    LdIXNNi,    DecIX,      IncXL,      DecXL,      LdXLN,      Cpl,        // 28-2f
            JrNC,       LdSPNN,     LdNNiA,     IncSP,      IncIXi,     DecIXi,     LdIXiN,     Scf,        // 30-37
            JrC,        AddIXSP,    LdANNi,     DecSP,      IncA,       DecA,       LdAN,       Ccf,        // 38-3f

            Nop,        LdB_C,      LdB_D,      LdB_E,      LdBXH,      LdBXL,      LdBIXi,     LdB_A,      // 40-47
            LdC_B,      Nop,        LdC_D,      LdC_E,      LdCXH,      LdCXL,      LdCIXi,     LdC_A,      // 48-4f
            LdD_B,      LdD_C,      Nop,        LdD_E,      LdDXH,      LdDXL,      LdDIXi,     LdD_A,      // 50-57
            LdE_B,      LdE_C,      LdE_D,      Nop,        LdEXH,      LdEXL,      LdEIXi,     LdE_A,      // 58-5f
            LdXHB,      LdXHC,      LdXHD,      LdXHE,      Nop,        LdXHXL,     LdHIXi,     LdXHA,      // 60-67
            LdXLB,      LdXLC,      LdXLD,      LdXLE,      LdXLXH,     Nop,        LdLIXi,     LdXLA,      // 68-6f
            LdIXiB,     LdIXiC,     LdIXiD,     LdIXiE,     LdIXiH,     LdIXiL,     Halt,       LdIXiA,     // 70-77
            LdA_B,      LdA_C,      LdA_D,      LdA_E,      LdAXH,      LdAXL,      LdAIXi,     Nop,        // 78-7f

            AddA_B,     AddA_C,     AddA_D,     AddA_E,     AddAXH,     AddAXL,     AddAIXi,    AddA_A,     // 80-87
            AdcA_B,     AdcA_C,     AdcA_D,     AdcA_E,     AdcAXH,     AdcAXL,     AdcAIXi,    AdcA_A,     // 88-8f
            SubB,       SubC,       SubD,       SubE,       SubAXH,     SubAXL,     SubAIXi,    SubA,       // 90-97
            SbcB,       SbcC,       SbcD,       SbcE,       SbcAXH,     SbcAXL,     SbcAIXi,    SbcA,       // 98-9f
            AndB,       AndC,       AndD,       AndE,       AndAXH,     AndAXL,     AndAIXi,    AndA,       // a0-a7
            XorB,       XorC,       XorD,       XorE,       XorAXH,     XorAXL,     XorAIXi,    XorA,       // a8-af
            OrB,        OrC,        OrD,        OrE,        OrAXH,      OrAXL,      OrAIXi,     OrA,        // b0-b7
            CpB,        CpC,        CpD,        CpE,        CpAXH,      CpAXL,      CpAIXi,     CpA,        // b8-bf

            RetNZ,      PopBC,      JpNZ_NN,    JpNN,       CallNZ,     PushBC,     AddAN,      Rst00,      // c0-c7
            RetZ,       Ret,        JpZ_NN,     Nop,        CallZ,      CallNN,     AdcAN,      Rst08,      // c8-cf
            RetNC,      PopDE,      JpNC_NN,    OutNA,      CallNC,     PushDE,     SubAN,      Rst10,      // d0-d7
            RetC,       Exx,        JpC_NN,     InAN,       CallC,      Nop,        SbcAN,      Rst18,      // d8-df
            RetPO,      PopIX,      JpPO_NN,    ExSPiIX,    CallPO,     PushIX,     AndAN,      Rst20,      // e0-e7
            RetPE,      JpIX,       JpPE_NN,    ExDEHL,     CallPE,     Nop,        XorAN,      Rst28,      // e8-ef
            RetP,       PopAF,      JpP_NN,     Di,         CallP,      PushAF,     OrAN,       Rst30,      // f0-f7
            RetM,       LdSPIX,     JpM_NN,     Ei,         CallM,      Nop,        CpAN,       Rst38,      // f8-ff
        };
    }

    /// <summary>
    /// Get or set the value of the current index register
    /// </summary>
    private ushort IndexReg
    {
        get => Prefix == OpCodePrefix.DD || Prefix == OpCodePrefix.DDCB ? Regs.IX : Regs.IY;
        set 
        { 
            if (Prefix == OpCodePrefix.DD || Prefix == OpCodePrefix.DDCB)
            {
                Regs.IX = value;
            }
            else
            {
                Regs.IY = value;
            }
        }
    }

    /// <summary>
    /// Get or set the LSB value of the current index register
    /// </summary>
    private byte IndexL
    {
        get => Prefix == OpCodePrefix.DD || Prefix == OpCodePrefix.DDCB ? Regs.XL : Regs.YL;
        set
        {
            if (Prefix == OpCodePrefix.DD || Prefix == OpCodePrefix.DDCB)
            {
                Regs.XL = value;
            }
            else
            {
                Regs.YL = value;
            }
        }
    }

    /// <summary>
    /// Get or set the MSB value of the current index register
    /// </summary>
    private byte IndexH
    {
        get => Prefix == OpCodePrefix.DD || Prefix == OpCodePrefix.DDCB ? Regs.XH : Regs.YH;
        set
        {
            if (Prefix == OpCodePrefix.DD || Prefix == OpCodePrefix.DDCB)
            {
                Regs.XH = value;
            }
            else
            {
                Regs.YH = value;
            }
        }
    }

    /// <summary>
    /// "add ix,bc" operation (0x09)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of BC register pair are added to the contents of IX/IY, and the results are stored in IX/IY.
    /// 
    /// S, Z, P/V is not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void AddIXBC()
    {
        TactPlus7(Regs.IR);
        IndexReg = Add16(IndexReg, Regs.BC);
    }

    /// <summary>
    /// "add ix,de" operation (0x19)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of DE register pair are added to the contents of IX/IY, and the results are stored in IX/IY.
    /// 
    /// S, Z, P/V is not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void AddIXDE()
    {
        TactPlus7(Regs.IR);
        IndexReg = Add16(IndexReg, Regs.DE);
    }

    /// <summary>
    /// "ld ix,NN" operation (0x21)
    /// </summary>
    /// <remarks>
    /// The 16-bit integer is loaded to IX.
    /// 
    /// T-States: 14 (4, 4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:3
    /// </remarks>
    private void LdIXNN()
    {
        IndexL = FetchCodeByte();
        IndexH = FetchCodeByte();
    }

    /// <summary>
    /// "ld (NN),ix" operation (0x22)
    /// </summary>
    /// <remarks>
    /// The low-order byte in IX/IY is loaded to memory address (NN); the upper order byte is loaded to the next
    /// highest address (NN + 1).
    /// 
    /// T-States: 20 (4, 4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,nn:3,nn+1:3
    /// </remarks>
    private void LdNNiIX()
    {
        Store16(IndexL, IndexH);
    }

    /// <summary>
    /// "inc ix" operation (0x23)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of IX/IY are incremented.
    /// 
    /// T-States: 10 (4, 6)
    /// Contention breakdown: pc:4,pc+1:6
    /// </remarks>
    private void IncIX()
    {
        IndexReg++;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc xh" operation (0x24)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are incremented.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void IncXH()
    {
        Regs.F = (byte)(s_8BitIncFlags[IndexH++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec xh" operation (0x25)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of XH/YH are decremented.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void DecXH()
    {
        Regs.F = (byte)(s_8BitDecFlags[IndexH--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld xh,N" operation (0x26)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to XH/YH
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3
    /// </remarks>
    private void LdXHN()
    {
        IndexH = FetchCodeByte();
    }

    /// <summary>
    /// "add ix,ix" operation (0x29)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of IX/IY register pair are added to the contents of IX/IY, and the results are stored in IX/IY.
    /// 
    /// S, Z, P/V is not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void AddIXIX()
    {
        TactPlus7(Regs.IR);
        IndexReg = Add16(IndexReg, IndexReg);
    }

    /// <summary>
    /// "ld ix,(NN)" operation (0x2A)
    /// </summary>
    /// <remarks>
    /// The contents of the address (NN) are loaded to the low-order portion of IX/IY, and the contents of the next
    /// highest memory address (NN + 1) are loaded to the high-orderp ortion of IX/IY.
    /// 
    /// T-States: 20 (4, 4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,nn:3,nn+1:3
    /// </remarks>
    private void LdIXNNi()
    {
        var addr = (ushort)(FetchCodeByte() + (FetchCodeByte() << 8));
        IndexL = ReadMemory(addr);
        Regs.WZ = addr = (ushort)(addr + 1);
        IndexH = ReadMemory(addr);
    }

    /// <summary>
    /// "dec ix" operation (0x2B)
    /// </summary>
    /// <remarks>
    /// The contents of IX/IY are decremented.
    ///
    /// T-States: 10 (4, 6)
    /// Contention breakdown: pc:4,pc+1:6
    /// </remarks>
    private void DecIX()
    {
        IndexReg--;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc xl" operation (0x2C)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are incremented.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void IncXL()
    {
        Regs.F = (byte)(s_8BitIncFlags[IndexL++] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "dec xl" operation (0x2D)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of XL/YL are decremented.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void DecXL()
    {
        Regs.F = (byte)(s_8BitDecFlags[IndexL--] | (Regs.F & FlagsSetMask.C));
        F53Updated = true;
    }

    /// <summary>
    /// "ld xl,N" operation (0x2E)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to XH/YH
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3
    /// </remarks>
    private void LdXLN()
    {
        IndexL = FetchCodeByte();
    }

    /// <summary>
    /// "inc (ix+D)" operation (0x34)
    /// </summary>
    /// <remarks>
    /// The contents of IX/IY are added to the two's-complement displacement integer, D, to point to an address in
    /// memory. The contents of this address are then incremented.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if (IX+D) was 0x7F before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:4,ii+n(write):3
    /// </remarks>
    private void IncIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Regs.F = (byte)(s_8BitIncFlags[tmp++] | Regs.F & FlagsSetMask.C);
        F53Updated = true;
        WriteMemory(Regs.WZ, tmp);
    }

    /// <summary>
    /// "dec (ix+D)" operation (0x35)
    /// </summary>
    /// <remarks>
    /// The contents of IX/IY are added to the two's-complement displacement integer, D, to point to an address in
    /// memory. The contents of this address are then decremented.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if (IX+D) was 0x80 before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 23 (4, 4, 3, 5, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3,ii+n:1,ii+n(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:4,ii+n(write):3
    /// </remarks>
    private void DecIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        var tmp = ReadMemory(Regs.WZ);
        TactPlus1(Regs.WZ);
        Regs.F = (byte)(s_8BitDecFlags[tmp--] | Regs.F & FlagsSetMask.C);
        F53Updated = true;
        WriteMemory(Regs.WZ, tmp);
    }

    /// <summary>
    /// "ld (ix+D),N" operation (0x36)
    /// </summary>
    /// <remarks>
    /// 
    /// The N operand is loaded to the memory address specified by the sum of IX and the two's complement displacement
    /// operand D.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 ×2,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:3,pc+3:5,ii+n:3
    /// </remarks>
    private void LdIXiN()
    {
        byte dist = FetchCodeByte();
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        byte value = FetchCodeByte();
        TactPlus2(Regs.PC);
        WriteMemory(Regs.WZ, value);
    }

    /// <summary>
    /// "add ix,sp" operation (0x39)
    /// </summary>
    /// <remarks>
    /// 
    /// The contents of SP register pair are added to the contents of IX/IY, and the results are stored in IX/IY.
    /// 
    /// S, Z, P/V is not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:11
    /// </remarks>
    private void AddIXSP()
    {
        TactPlus7(Regs.IR);
        IndexReg = Add16(IndexReg, Regs.SP);
    }

    /// <summary>
    /// "ld b,xh" operation (0x44)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are moved to B
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdBXH()
    {
        Regs.B = IndexH;
    }

    /// <summary>
    /// "ld b,xl" operation (0x45)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are moved to B
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdBXL()
    {
        Regs.B = IndexL;
    }

    /// <summary>
    /// "ld b,(ix+D)" operation (0x46)
    /// </summary>
    /// <remarks>
    /// The contents of IX summed with two's-complement displacement D is loaded to B.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdBIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Regs.B = ReadMemory(Regs.WZ);
    }

    /// <summary>
    /// "ld c,xh" operation (0x4C)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are moved to C
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdCXH()
    {
        Regs.C = IndexH;
    }

    /// <summary>
    /// "ld c,xl" operation (0x4D)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are moved to C
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdCXL()
    {
        Regs.C = IndexL;
    }

    /// <summary>
    /// "ld c,(ix+D)" operation (0x4E)
    /// </summary>
    /// <remarks>
    /// The contents of IX summed with two's-complement displacement D is loaded to C.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdCIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Regs.C = ReadMemory(Regs.WZ);
    }

    /// <summary>
    /// "ld d,xh" operation (0x54)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are moved to D
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdDXH()
    {
        Regs.D = IndexH;
    }

    /// <summary>
    /// "ld d,xl" operation (0x55)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are moved to D
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdDXL()
    {
        Regs.D = IndexL;
    }

    /// <summary>
    /// "ld d,(ix+D)" operation (0x56)
    /// </summary>
    /// <remarks>
    /// The contents of IX summed with two's-complement displacement D is loaded to D.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdDIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Regs.D = ReadMemory(Regs.WZ);
    }

    /// <summary>
    /// "ld e,xh" operation (0x5C)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are moved to E
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdEXH()
    {
        Regs.E = IndexH;
    }

    /// <summary>
    /// "ld e,xl" operation (0x5D)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are moved to E
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdEXL()
    {
        Regs.E = IndexL;
    }

    /// <summary>
    /// "ld e,(ix+D)" operation (0x5E)
    /// </summary>
    /// <remarks>
    /// The contents of IX summed with two's-complement displacement D is loaded to E.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdEIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Regs.E = ReadMemory(Regs.WZ);
    }

    /// <summary>
    /// "ld xh,b" operation (0x60)
    /// </summary>
    /// <remarks>
    /// The contents of B are moved to XH/YH.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXHB()
    {
        IndexH = Regs.B;
    }

    /// <summary>
    /// "ld xh,c" operation (0x61)
    /// </summary>
    /// <remarks>
    /// The contents of C are moved to XH/YH.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXHC()
    {
        IndexH = Regs.C;
    }

    /// <summary>
    /// "ld xh,d" operation (0x62)
    /// </summary>
    /// <remarks>
    /// The contents of D are moved to XH/YH.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXHD()
    {
        IndexH = Regs.D;
    }

    /// <summary>
    /// "ld xh,e" operation (0x63)
    /// </summary>
    /// <remarks>
    /// The contents of E are moved to XH/YH.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXHE()
    {
        IndexH = Regs.E;
    }

    /// <summary>
    /// "ld xh,xl" operation (0x65)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are moved to XH/YH
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXHXL()
    {
        IndexH = IndexL;
    }

    /// <summary>
    /// "ld h,(ix+D)" operation (0x66)
    /// </summary>
    /// <remarks>
    /// The contents of IX summed with two's-complement displacement D is loaded to H.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdHIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Regs.H = ReadMemory(Regs.WZ);
    }

    /// <summary>
    /// "ld xh,a" operation (0x67)
    /// </summary>
    /// <remarks>
    /// The contents of A are moved to XH/YH.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXHA()
    {
        IndexH = Regs.A;
    }

    /// <summary>
    /// "ld xl,b" operation (0x68)
    /// </summary>
    /// <remarks>
    /// The contents of B are moved to XL/YL.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXLB()
    {
        IndexL = Regs.B;
    }

    /// <summary>
    /// "ld xl,c" operation (0x69)
    /// </summary>
    /// <remarks>
    /// The contents of C are moved to XL/YL.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXLC()
    {
        IndexL = Regs.C;
    }

    /// <summary>
    /// "ld xl,d" operation (0x6A)
    /// </summary>
    /// <remarks>
    /// The contents of D are moved to XL/YL.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXLD()
    {
        IndexL = Regs.D;
    }

    /// <summary>
    /// "ld xl,e" operation (0x6B)
    /// </summary>
    /// <remarks>
    /// The contents of B are moved to XL/YL.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXLE()
    {
        IndexL = Regs.E;
    }

    /// <summary>
    /// "ld xl,xh" operation (0x6C)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are moved to XL/YL
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXLXH()
    {
        IndexL = IndexH;
    }

    /// <summary>
    /// "ld l,(ix+D)" operation (0x6E)
    /// </summary>
    /// <remarks>
    /// The contents of IX summed with two's-complement displacement D is loaded to L.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdLIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Regs.L = ReadMemory(Regs.WZ);
    }

    /// <summary>
    /// "ld xl,a" operation (0x6F)
    /// </summary>
    /// <remarks>
    /// The contents of A are moved to XL/YL.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdXLA()
    {
        IndexL = Regs.A;
    }

    /// <summary>
    /// "ld (ix+D),b" operation (0x70)
    /// </summary>
    /// <remarks>
    /// The contents of B are loaded to the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdIXiB()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        WriteMemory(Regs.WZ, Regs.B);
    }

    /// <summary>
    /// "ld (ix+D),c" operation (0x71)
    /// </summary>
    /// <remarks>
    /// The contents of C are loaded to the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdIXiC()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        WriteMemory(Regs.WZ, Regs.C);
    }

    /// <summary>
    /// "ld (ix+D),d" operation (0x72)
    /// </summary>
    /// <remarks>
    /// The contents of D are loaded to the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdIXiD()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        WriteMemory(Regs.WZ, Regs.D);
    }

    /// <summary>
    /// "ld (ix+D),e" operation (0x73)
    /// </summary>
    /// <remarks>
    /// The contents of E are loaded to the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdIXiE()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        WriteMemory(Regs.WZ, Regs.E);
    }

    /// <summary>
    /// "ld (ix+D),h" operation (0x74)
    /// </summary>
    /// <remarks>
    /// The contents of H are loaded to the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdIXiH()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        WriteMemory(Regs.WZ, Regs.H);
    }

    /// <summary>
    /// "ld (ix+D),l" operation (0x75)
    /// </summary>
    /// <remarks>
    /// The contents of L are loaded to the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdIXiL()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        WriteMemory(Regs.WZ, Regs.L);
    }

    /// <summary>
    /// "ld (ix+D),a" operation (0x77)
    /// </summary>
    /// <remarks>
    /// The contents of A are loaded to the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdIXiA()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        WriteMemory(Regs.WZ, Regs.A);
    }

    /// <summary>
    /// "ld a,xh" operation (0x7C)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are moved to A
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdAXH()
    {
        Regs.A = IndexH;
    }

    /// <summary>
    /// "ld a,xl" operation (0x7D)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are moved to A
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void LdAXL()
    {
        Regs.A = IndexL;
    }

    /// <summary>
    /// "ld a,(ix+D)" operation (0x7E)
    /// </summary>
    /// <remarks>
    /// The contents of IX summed with two's-complement displacement D is loaded to A.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void LdAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Regs.A = ReadMemory(Regs.WZ);
    }

    /// <summary>
    /// "add a,xh" operation (0x84)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are added to the contents of A, and the result is stored in A.
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
    private void AddAXH()
    {
        Add8(IndexH);
    }

    /// <summary>
    /// "add a,xl" operation (0x85)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are added to the contents of A, and the result is stored in A.
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
    private void AddAXL()
    {
        Add8(IndexL);
    }

    /// <summary>
    /// "add a,(ix+D)" operation (0x86)
    /// </summary>
    /// <remarks>
    /// The contents the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer, is added to A.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void AddAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Add8(ReadMemory(Regs.WZ));
    }

    /// <summary>
    /// "adc a,xh" operation (0x8C)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are added to the contents of A (along with the Carry), and the result is stored in A.
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
    private void AdcAXH()
    {
        Adc8(IndexH);
    }

    /// <summary>
    /// "adc a,xl" operation (0x8D)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are added to the contents of A (along with the Carry), and the result is stored in A.
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
    private void AdcAXL()
    {
        Adc8(IndexL);
    }

    /// <summary>
    /// "adc a,(ix+D)" operation (0x8E)
    /// </summary>
    /// <remarks>
    /// The contents the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer, is added to A (along with the Carry).
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void AdcAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Adc8(ReadMemory(Regs.WZ));
    }

    /// <summary>
    /// "sub xh" operation (0x94)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are subtracted from the contents of A, and the result is stored in A.
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
    private void SubAXH()
    {
        Sub8(IndexH);
    }

    /// <summary>
    /// "sub xl" operation (0x95)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are subtracted from the contents of A, and the result is stored in A.
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
    private void SubAXL()
    {
        Sub8(IndexL);
    }

    /// <summary>
    /// "sub a,(ix+D)" operation (0x96)
    /// </summary>
    /// <remarks>
    /// The contents the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer, is subtracted from A.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void SubAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Sub8(ReadMemory(Regs.WZ));
    }

    /// <summary>
    /// "sbc xh" operation (0x9C)
    /// </summary>
    /// <remarks>
    /// The contents of XH/YH are subtracted from the contents of A (along with the Carry), and the result is stored
    /// in A.
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
    private void SbcAXH()
    {
        Sbc8(IndexH);
    }

    /// <summary>
    /// "sub xl" operation (0x9D)
    /// </summary>
    /// <remarks>
    /// The contents of XL/YL are subtracted from the contents of A (along with the Carry), and the result is stored
    /// in A.
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
    private void SbcAXL()
    {
        Sbc8(IndexL);
    }

    /// <summary>
    /// "sbc a,(ix+D)" operation (0x9E)
    /// </summary>
    /// <remarks>
    /// The contents the memory address specified by the contents of IX summed with D, a 
    /// two's-complement displacement integer, is subtracted from A (along with the Carry).
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void SbcAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Sbc8(ReadMemory(Regs.WZ));
    }

    /// <summary>
    /// "and xh" operation (0xA4)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is executed on the contents of XH/YH and the contents of A, and the result is stored
    /// in A.
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
    private void AndAXH()
    {
        And8(IndexH);
    }

    /// <summary>
    /// "and xl" operation (0xA5)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is executed on the contents of XL/YL and the contents of A, and the result is stored
    /// in A.
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
    private void AndAXL()
    {
        And8(IndexL);
    }

    /// <summary>
    /// "and (ix+D)" operation (0xA6)
    /// </summary>
    /// <remarks>
    /// A logical AND operation is executed on the contents of the memory address specified by the contents of IX
    /// summed with D, a two's-complement displacement integer and the contents of A, and the result is stored in A.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void AndAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        And8(ReadMemory(Regs.WZ));
    }

    /// <summary>
    /// "xor xh" operation (0xAC)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is executed on the contents of XH/YH and the contents of A, and the result is stored
    /// in A.
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
    private void XorAXH()
    {
        Xor8(IndexH);
    }

    /// <summary>
    /// "xor xl" operation (0xAD)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is executed on the contents of XL/YL and the contents of A, and the result is stored
    /// in A.
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
    private void XorAXL()
    {
        Xor8(IndexL);
    }

    /// <summary>
    /// "xor (ix+D)" operation (0xAE)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is executed on the contents of the memory address specified by the contents of IX
    /// summed with D, a two's-complement displacement integer and the contents of A, and the result is stored in A.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void XorAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Xor8(ReadMemory(Regs.WZ));
    }

    /// <summary>
    /// "or xh" operation (0xB4)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is executed on the contents of XH/YH and the contents of A, and the result is stored
    /// in A.
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
    private void OrAXH()
    {
        Or8(IndexH);
    }

    /// <summary>
    /// "or xl" operation (0xB5)
    /// </summary>
    /// <remarks>
    /// A logical OR operation is executed on the contents of XL/YL and the contents of A, and the result is stored
    /// in A.
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
    private void OrAXL()
    {
        Or8(IndexL);
    }

    /// <summary>
    /// "or (ix+D)" operation (0xB6)
    /// </summary>
    /// <remarks>
    /// A logical XOR operation is executed on the contents of the memory address specified by the contents of IX
    /// summed with D, a two's-complement displacement integer and the contents of A, and the result is stored in A.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void OrAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Or8(ReadMemory(Regs.WZ));
    }

    /// <summary>
    /// "cp xh" operation (0xBC)
    /// </summary>
    /// <remarks>
    /// Comparison operation is executed on the contents of XH/YH and the contents of A, and the result is stored in A.
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
    private void CpAXH()
    {
        Cp8(IndexH);
    }

    /// <summary>
    /// "cp xl" operation (0xBD)
    /// </summary>
    /// <remarks>
    /// A comparison operation is executed on the contents of XL/YL and the contents of A, and the result is stored in A.
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
    private void CpAXL()
    {
        Cp8(IndexL);
    }

    /// <summary>
    /// "cp (ix+D)" operation (0xBE)
    /// </summary>
    /// <remarks>
    /// A comparison operation is executed on the contents of the memory address specified by the contents of IX
    /// summed with D, a two's-complement displacement integer and the contents of A, and the result is stored in A.
    /// 
    /// T-States: 19 (4, 4, 3, 5, 3)
    /// Contention breakdown: pc:4,pc+1:4,pc+2:3,pc+2:1 ×5,ii+n:3
    /// Gate array contention breakdown: pc:4,pc+1:4,pc+2:8,ii+n:3
    /// </remarks>
    private void CpAIXi()
    {
        byte dist = ReadMemory(Regs.PC);
        TactPlus5(Regs.PC);
        Regs.PC++;
        Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
        Cp8(ReadMemory(Regs.WZ));
    }

    /// <summary>
    /// "pop ix" operation (0xE1)
    /// </summary>
    /// <remarks>
    /// The top two bytes of the external memory last-in, first-out (LIFO) stack are popped to IX/IY. SP holds the 
    /// 16-bit address of the current top of the Stack. This instruction first loads to the low-order portion of 
    /// IX/IY the byte at the memory location corresponding to the contents of SP; then SP is incremented and the
    /// contents of the corresponding adjacent memory location are loaded to the high-order portion of IX/IY.
    /// SP is incremented again.
    /// 
    /// T-States: 14 (4, 4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:4,sp:3,sp+1:3
    /// </remarks>
    private void PopIX()
    {
        IndexL = ReadMemory(Regs.SP);
        Regs.SP++;
        IndexH = ReadMemory(Regs.SP);
        Regs.SP++;
    }

    /// <summary>
    /// "ex (sp),ix" operation (0xE3)
    /// </summary>
    /// <remarks>
    /// The low-order byte in IX/IY is exchanged with the contents of the  memory address specified by the contents
    /// of SP, and the high-order byte of IX is exchanged with the next highest memory address (SP+1).
    /// 
    /// T-States: 23 (4, 4, 3, 4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:4,sp:3,sp+1:3,sp+1:1,sp+1(write):3,sp(write):3,sp(write):1 ×2
    /// Gate array contention breakdown: pc:4,pc+1:4,sp:3,sp+1:4,sp+1(write):3,sp(write):5
    /// </remarks>
    private void ExSPiIX()
    {
        var sp1 = (ushort)(Regs.SP + 1);
        var tempL = ReadMemory(Regs.SP);
        var tempH = ReadMemory(sp1);
        TactPlus1(Regs.SP);
        WriteMemory(sp1, IndexH);
        WriteMemory(Regs.SP, IndexL);
        TactPlus2(Regs.SP);
        Regs.WL = tempL;
        Regs.WH = tempH;
        IndexReg = Regs.WZ;
    }

    /// <summary>
    /// "push ix" operation (0xE5)
    /// </summary>
    /// <remarks>
    /// The contents of IX/IY are pushed to the external memory last-in, first-out (LIFO) stack. SP holds the 16-bit
    /// address of the current top of the Stack. This instruction first decrements SP and loads the high-order byte of
    /// IX/IY to the memory address specified by SP; then decrements SP again and loads the low-order byte to the
    /// memory location corresponding to this new address in SP.
    /// 
    /// T-States: 15 (4, 5, 3, 3)
    /// Contention breakdown: pc:4,pc+1:5,sp-1:3,sp-2:3
    /// </remarks>
    private void PushIX()
    {
        TactPlus1(Regs.IR);
        Regs.SP--;
        WriteMemory(Regs.SP, IndexH);
        Regs.SP--;
        WriteMemory(Regs.SP, IndexL);
    }

    /// <summary>
    /// "jp (ix)" operation (0xE9)
    /// </summary>
    /// <remarks>
    /// The PC is loaded with the contents of IX. The next instruction is fetched from the location designated by the
    /// new contents of PC.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void JpIX()
    {
        Regs.PC = IndexReg;
    }

    /// <summary>
    /// "ld sp,ix" operation (0xF9)
    /// </summary>
    /// <remarks>
    /// The 2-byte contents of IX/IY are loaded to SP.
    /// 
    /// T-States: 10 (4, 6)
    /// Contention breakdown: pc:4,pc+1:6
    /// </remarks>
    private void LdSPIX()
    {
        TactPlus2(Regs.IR);
        Regs.SP = IndexReg;
    }
}