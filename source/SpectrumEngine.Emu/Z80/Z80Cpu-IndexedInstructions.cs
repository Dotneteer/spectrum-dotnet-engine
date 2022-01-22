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
            JrNZ,       LdIXNN,     LdNNiHL,    IncHL,      IncH,       DecH,       LdHN,       Daa,        // 20-27
            JrZ,        AddIXIX,    LdHLNNi,    DecHL,      IncL,       DecL,       LdLN,       Cpl,        // 28-2f
            JrNC,       LdSPNN,     LdNNiA,     IncSP,      IncHLi,     DecHLi,     LdHLiN,     Scf,        // 30-37
            JrC,        AddIXSP,    LdANNi,     DecSP,      IncA,       DecA,       LdAN,       Ccf,        // 38-3f

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
            RetPE,      JpIX,       JpPE_NN,    ExDEHL,     CallPE,     Nop,        XorAN,      Rst28,      // e8-ef
            RetP,       PopAF,      JpP_NN,     Di,         CallP,      PushAF,     OrAN,       Rst30,      // f0-f7
            RetM,       LdSPHL,     JpM_NN,     Ei,         CallM,      Nop,        CpAN,       Rst38,      // f8-ff
        };
    }

    /// <summary>
    /// Get or set the value of the current index register
    /// </summary>
    private ushort IndexReg
    {
        get => Prefix == OpCodePrefix.DD ? Regs.IX : Regs.IY;
        set 
        { 
            if (Prefix == OpCodePrefix.DD)
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
        get => Prefix == OpCodePrefix.DD ? Regs.XL : Regs.YL;
        set
        {
            if (Prefix == OpCodePrefix.DD)
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
        get => Prefix == OpCodePrefix.DD ? Regs.XH : Regs.YH;
        set
        {
            if (Prefix == OpCodePrefix.DD)
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
        IndexL = ReadCodeMemory();
        IndexH = ReadCodeMemory();
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
    /// "add ix,sp" operation (0x29)
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
}