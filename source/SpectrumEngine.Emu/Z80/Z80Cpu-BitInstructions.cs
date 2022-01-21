namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This file contains the code for processing bit manipulation instructions (with `$CB` prefix).
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// This array contains the 256 function references, each executing a  standard Z80 instruction.
    /// </summary>
    private Action[]? _bitInstrs;

    /// <summary>
    /// Initialize the table of standard instructions.
    /// </summary>
    private void InitializeBitInstructionsTable()
    {
        _bitInstrs = new Action[]
        {
            Rlc_B,      Rlc_C,      Rlc_D,      Rlc_E,      Rlc_H,      Rlc_L,      Rlc_HLi,    Rlc_A,      // 00-07
            Rrc_B,      Rrc_C,      Rrc_D,      Rrc_E,      Rrc_H,      Rrc_L,      Rrc_HLi,    Rrc_A,      // 08-0f
            Rl_B,       Rl_C,       Rl_D,       Rl_E,       Rl_H,       Rl_L,       Rl_HLi,     Rl_A,       // 10-17
            Rr_B,       Rr_C,       Rr_D,       Rr_E,       Rr_H,       Rr_L,       Rr_HLi,     Rr_A,       // 18-1f
            Sla_B,      Sla_C,      Sla_D,      Sla_E,      Sla_H,      Sla_L,      Sla_HLi,    Sla_A,      // 20-27
            Sra_B,      Sra_C,      Sra_D,      Sra_E,      Sra_H,      Sra_L,      Sra_HLi,    Sra_A,      // 28-2f
            Sll_B,      Sll_C,      Sll_D,      Sll_E,      Sll_H,      Sll_L,      Sll_HLi,    Sll_A,      // 30-37
            Srl_B,      Srl_C,      Srl_D,      Srl_E,      Srl_H,      Srl_L,      Srl_HLi,    Srl_A,      // 38-3f

            Bit0_B,     Bit0_C,     Bit0_D,     Bit0_E,     Bit0_H,     Bit0_L,     Bit0_HLi,   Bit0_A,     // 40-47
            Bit1_B,     Bit1_C,     Bit1_D,     Bit1_E,     Bit1_H,     Bit1_L,     Bit1_HLi,   Bit1_A,     // 48-4f
            Bit2_B,     Bit2_C,     Bit2_D,     Bit2_E,     Bit2_H,     Bit2_L,     Bit2_HLi,   Bit2_A,     // 50-57
            Bit3_B,     Bit3_C,     Bit3_D,     Bit3_E,     Bit3_H,     Bit3_L,     Bit3_HLi,   Bit3_A,     // 58-5f
            Bit4_B,     Bit4_C,     Bit4_D,     Bit4_E,     Bit4_H,     Bit4_L,     Bit4_HLi,   Bit4_A,     // 60-67
            Bit5_B,     Bit5_C,     Bit5_D,     Bit5_E,     Bit5_H,     Bit5_L,     Bit5_HLi,   Bit5_A,     // 68-6f
            Bit6_B,     Bit6_C,     Bit6_D,     Bit6_E,     Bit6_H,     Bit6_L,     Bit6_HLi,   Bit6_A,     // 70-77
            Bit7_B,     Bit7_C,     Bit7_D,     Bit7_E,     Bit7_H,     Bit7_L,     Bit7_HLi,   Bit7_A,     // 78-7f

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
    /// "rlc b" operation (0x00)
    /// </summary>
    /// <remarks>
    /// The contents of register B are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag
    /// and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rlc_B()
    {
        Regs.B = Rlc8(Regs.B);
    }

    /// <summary>
    /// "rlc c" operation (0x01)
    /// </summary>
    /// <remarks>
    /// The contents of register C are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag
    /// and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rlc_C()
    {
        Regs.C = Rlc8(Regs.C);
    }

    /// <summary>
    /// "rlc d" operation (0x02)
    /// </summary>
    /// <remarks>
    /// The contents of register D are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag
    /// and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rlc_D()
    {
        Regs.D = Rlc8(Regs.D);
    }

    /// <summary>
    /// "rlc e" operation (0x03)
    /// </summary>
    /// <remarks>
    /// The contents of register E are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag
    /// and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rlc_E()
    {
        Regs.E = Rlc8(Regs.E);
    }

    /// <summary>
    /// "rlc h" operation (0x04)
    /// </summary>
    /// <remarks>
    /// The contents of register H are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag
    /// and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rlc_H()
    {
        Regs.H = Rlc8(Regs.H);
    }

    /// <summary>
    /// "rlc l" operation (0x05)
    /// </summary>
    /// <remarks>
    /// The contents of register L are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag
    /// and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rlc_L()
    {
        Regs.L = Rlc8(Regs.L);
    }

    /// <summary>
    /// "rlc (hl)" operation (0x06)
    /// </summary>
    /// <remarks>
    /// The contents of the memory address specified by the contents of HL are rotated left 1 bit position.The contents
    /// of bit 7 are copied to the Carry flag and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4,hl(write):3
    /// </remarks>
    private void Rlc_HLi()
    {
        var tmp = Rlc8(ReadMemory(Regs.HL));
        TactPlus1(Regs.HL);
        WriteMemory(Regs.HL, tmp);
    }

    /// <summary>
    /// "rlc a" operation (0x07)
    /// </summary>
    /// <remarks>
    /// The contents of register A are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag
    /// and also to bit 0.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rlc_A()
    {
        Regs.A = Rlc8(Regs.A);
    }

    /// <summary>
    /// "rrc b" operation (0x08)
    /// </summary>
    /// <remarks>
    /// The contents of register B are rotated right 1 bit position. The contents of bit 0 are copied to the Carry flag
    /// and also  to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rrc_B()
    {
        Regs.B = Rrc8(Regs.B);
    }

    /// <summary>
    /// "rrc c" operation (0x09)
    /// </summary>
    /// <remarks>
    /// The contents of register C are rotated right 1 bit position. The contents of bit 0 are copied to the Carry flag
    /// and also  to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rrc_C()
    {
        Regs.C = Rrc8(Regs.C);
    }

    /// <summary>
    /// "rrc d" operation (0x0A)
    /// </summary>
    /// <remarks>
    /// The contents of register D are rotated right 1 bit position. The contents of bit 0 are copied to the Carry flag
    /// and also  to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rrc_D()
    {
        Regs.D = Rrc8(Regs.D);
    }

    /// <summary>
    /// "rrc e" operation (0x0B)
    /// </summary>
    /// <remarks>
    /// The contents of register E are rotated right 1 bit position. The contents of bit 0 are copied to the Carry flag
    /// and also  to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rrc_E()
    {
        Regs.E = Rrc8(Regs.E);
    }

    /// <summary>
    /// "rrc h" operation (0x0C)
    /// </summary>
    /// <remarks>
    /// The contents of register H are rotated right 1 bit position. The contents of bit 0 are copied to the Carry flag
    /// and also  to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rrc_H()
    {
        Regs.H = Rrc8(Regs.H);
    }

    /// <summary>
    /// "rrc l" operation (0x0D)
    /// </summary>
    /// <remarks>
    /// The contents of register B are rotated right 1 bit position. The contents of bit 0 are copied to the Carry flag
    /// and also  to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rrc_L()
    {
        Regs.L = Rrc8(Regs.L);
    }

    /// <summary>
    /// "rrc (hl)" operation (0x0E)
    /// </summary>
    /// <remarks>
    /// The contents of the memory address specified by the contents of HL are rotated right 1 bit position. The
    /// contents of bit 0  are copied to the Carry flag and also to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the source byte.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4,hl(write):3
    /// </remarks>
    private void Rrc_HLi()
    {
        var tmp = Rrc8(ReadMemory(Regs.HL));
        TactPlus1(Regs.HL);
        WriteMemory(Regs.HL, tmp);
    }

    /// <summary>
    /// "rrc a" operation (0x0F)
    /// </summary>
    /// <remarks>
    /// The contents of register A are rotated right 1 bit position. The contents of bit 0 are copied to the Carry flag
    /// and also  to bit 7.
    /// 
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// P/V is set if parity even; otherwise, it is reset.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rrc_A()
    {
        Regs.A = Rrc8(Regs.A);
    }

    /// <summary>
    /// "rl b" operation (0x10)
    /// </summary>
    /// <remarks>
    /// The contents of Register B are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag,
    /// and the previous contents of the Carry flag are copied to bit 0.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rl_B()
    {
        Regs.B = Rl8(Regs.B);
    }

    /// <summary>
    /// "rl c" operation (0x11)
    /// </summary>
    /// <remarks>
    /// The contents of Register C are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag,
    /// and the previous contents of the Carry flag are copied to bit 0.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rl_C()
    {
        Regs.C = Rl8(Regs.C);
    }

    /// <summary>
    /// "rl d" operation (0x12)
    /// </summary>
    /// <remarks>
    /// The contents of Register D are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag,
    /// and the previous contents of the Carry flag are copied to bit 0.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rl_D()
    {
        Regs.D = Rl8(Regs.D);
    }

    /// <summary>
    /// "rl e" operation (0x13)
    /// </summary>
    /// <remarks>
    /// The contents of Register E are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag,
    /// and the previous contents of the Carry flag are copied to bit 0.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rl_E()
    {
        Regs.E = Rl8(Regs.E);
    }

    /// <summary>
    /// "rl h" operation (0x14)
    /// </summary>
    /// <remarks>
    /// The contents of Register B are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag,
    /// and the previous contents of the Carry flag are copied to bit 0.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rl_H()
    {
        Regs.H = Rl8(Regs.H);
    }

    /// <summary>
    /// "rl l" operation (0x15)
    /// </summary>
    /// <remarks>
    /// The contents of Register L are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag,
    /// and the previous contents of the Carry flag are copied to bit 0.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rl_L()
    {
        Regs.L = Rl8(Regs.L);
    }

    /// <summary>
    /// "rl (hl)" operation (0x16)
    /// </summary>
    /// <remarks>
    /// The contents of the memory address specified by the contents of HL are rotated left 1 bit position. The
    /// contents of bit 7 are copied to the Carry flag, and the previous contents of the Carry flag are copied to
    /// bit 0.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4,hl(write):3
    /// </remarks>
    private void Rl_HLi()
    {
        var tmp = Rl8(ReadMemory(Regs.HL));
        TactPlus1(Regs.HL);
        WriteMemory(Regs.HL, tmp);
    }

    /// <summary>
    /// "rl a" operation (0x17)
    /// </summary>
    /// <remarks>
    /// The contents of Register B are rotated left 1 bit position. The contents of bit 7 are copied to the Carry flag,
    /// and the previous contents of the Carry flag are copied to bit 0.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rl_A()
    {
        Regs.A = Rl8(Regs.A);
    }

    /// <summary>
    /// "rr b" operation (0x18)
    /// </summary>
    /// <remarks>
    /// The contents of register B are rotated right 1 bit position through the Carry flag. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of the Carry flag are copied to bit 7.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rr_B()
    {
        Regs.B = Rr8(Regs.B);
    }

    /// <summary>
    /// "rr c" operation (0x19)
    /// </summary>
    /// <remarks>
    /// The contents of register C are rotated right 1 bit position through the Carry flag. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of the Carry flag are copied to bit 7.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rr_C()
    {
        Regs.C = Rr8(Regs.C);
    }

    /// <summary>
    /// "rr d" operation (0x1A)
    /// </summary>
    /// <remarks>
    /// The contents of register D are rotated right 1 bit position through the Carry flag. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of the Carry flag are copied to bit 7.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rr_D()
    {
        Regs.D = Rr8(Regs.D);
    }

    /// <summary>
    /// "rr e" operation (0x1B)
    /// </summary>
    /// <remarks>
    /// The contents of register E are rotated right 1 bit position through the Carry flag. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of the Carry flag are copied to bit 7.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rr_E()
    {
        Regs.E = Rr8(Regs.E);
    }

    /// <summary>
    /// "rr h" operation (0x1C)
    /// </summary>
    /// <remarks>
    /// The contents of register H are rotated right 1 bit position through the Carry flag. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of the Carry flag are copied to bit 7.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rr_H()
    {
        Regs.H = Rr8(Regs.H);
    }

    /// <summary>
    /// "rr l" operation (0x1D)
    /// </summary>
    /// <remarks>
    /// The contents of register L are rotated right 1 bit position through the Carry flag. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of the Carry flag are copied to bit 7.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rr_L()
    {
        Regs.L = Rr8(Regs.L);
    }

    /// <summary>
    /// "rr (hl)" operation (0x1E)
    /// </summary>
    /// <remarks>
    /// The contents of the memory address specified by the contents of HL are rotated right 1 bit position through the
    /// Carry flag. The contents of bit 0 are copied to the Carry flag and the previous contents of the Carry flag are
    /// copied to bit 7.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of (HL).
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4,hl(write):3
    /// </remarks>
    private void Rr_HLi()
    {
        var tmp = Rr8(ReadMemory(Regs.HL));
        TactPlus1(Regs.HL);
        WriteMemory(Regs.HL, tmp);
    }

    /// <summary>
    /// "rr a" operation (0x1F)
    /// </summary>
    /// <remarks>
    /// The contents of register A are rotated right 1 bit position through the Carry flag. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of the Carry flag are copied to bit 7.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Rr_A()
    {
        Regs.A = Rr8(Regs.A);
    }

    /// <summary>
    /// "sla b" operation (0x20)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register B. The contents of bit 7 are
    /// copied to the Carry flag.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sla_B()
    {
        Regs.B = Sla8(Regs.B);
    }

    /// <summary>
    /// "sla c" operation (0x21)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register C. The contents of bit 7 are
    /// copied to the Carry flag.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sla_C()
    {
        Regs.C = Sla8(Regs.C);
    }

    /// <summary>
    /// "sla d" operation (0x22)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register D. The contents of bit 7 are
    /// copied to the Carry flag.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sla_D()
    {
        Regs.D = Sla8(Regs.D);
    }

    /// <summary>
    /// "sla e" operation (0x23)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register E. The contents of bit 7 are
    /// copied to the Carry flag.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sla_E()
    {
        Regs.E = Sla8(Regs.E);
    }

    /// <summary>
    /// "sla h" operation (0x24)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register H. The contents of bit 7 are
    /// copied to the Carry flag.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sla_H()
    {
        Regs.H = Sla8(Regs.H);
    }

    /// <summary>
    /// "sla l" operation (0x25)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register L. The contents of bit 7 are
    /// copied to the Carry flag.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sla_L()
    {
        Regs.L = Sla8(Regs.L);
    }

    /// <summary>
    /// "sla (hl)" operation (0x26)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the 
    /// contents the memory address specified by the contents of HL.
    /// The contents of bit 7 are copied to the Carry flag.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of (HL).
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4,hl(write):3
    /// </remarks>
    private void Sla_HLi()
    {
        var tmp = Sla8(ReadMemory(Regs.HL));
        TactPlus1(Regs.HL);
        WriteMemory(Regs.HL, tmp);
    }

    /// <summary>
    /// "sla a" operation (0x27)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register A. The contents of bit 7 are
    /// copied to the Carry flag.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sla_A()
    {
        Regs.A = Sla8(Regs.A);
    }

    /// <summary>
    /// "sra b" operation (0x28)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of register B. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of bit 7 remain unchanged.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sra_B()
    {
        Regs.B = Sra8(Regs.B);
    }

    /// <summary>
    /// "sra c" operation (0x29)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of register C. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of bit 7 remain unchanged.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sra_C()
    {
        Regs.C = Sra8(Regs.C);
    }

    /// <summary>
    /// "sra d" operation (0x2A)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of register D. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of bit 7 remain unchanged.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sra_D()
    {
        Regs.D = Sra8(Regs.D);
    }

    /// <summary>
    /// "sra e" operation (0x2B)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of register E. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of bit 7 remain unchanged.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sra_E()
    {
        Regs.E = Sra8(Regs.E);
    }

    /// <summary>
    /// "sra h" operation (0x2C)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of register H. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of bit 7 remain unchanged.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sra_H()
    {
        Regs.H = Sra8(Regs.H);
    }

    /// <summary>
    /// "sra l" operation (0x2D)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of register L. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of bit 7 remain unchanged.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sra_L()
    {
        Regs.L = Sra8(Regs.L);
    }

    /// <summary>
    /// "sra (hl)" operation (0x2E)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the 
    /// contents the memory address specified by the contents of HL. 
    /// The contents of bit 0 are copied to the Carry flag and the 
    /// previous contents of bit 7 remain unchanged.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the source byte.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4,hl(write):3
    /// </remarks>
    private void Sra_HLi()
    {
        var tmp = Sra8(ReadMemory(Regs.HL));
        TactPlus1(Regs.HL);
        WriteMemory(Regs.HL, tmp);
    }

    /// <summary>
    /// "sra a" operation (0x2F)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift right 1 bit position is performed on the contents of register A. The contents of bit 0 are
    /// copied to the Carry flag and the previous contents of bit 7 remain unchanged.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sra_A()
    {
        Regs.A = Sra8(Regs.A);
    }

    /// <summary>
    /// "sll b" operation (0x30)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register B. The contents of bit 7 are
    /// copied to the Carry flag. Bit 0 is set to 1.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sll_B()
    {
        Regs.B = Sll8(Regs.B);
    }

    /// <summary>
    /// "sll c" operation (0x31)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register C. The contents of bit 7 are
    /// copied to the Carry flag. Bit 0 is set to 1.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sll_C()
    {
        Regs.C = Sll8(Regs.C);
    }

    /// <summary>
    /// "sll d" operation (0x32)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register D. The contents of bit 7 are
    /// copied to the Carry flag. Bit 0 is set to 1.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sll_D()
    {
        Regs.D = Sll8(Regs.D);
    }

    /// <summary>
    /// "sll e" operation (0x33)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register E. The contents of bit 7 are
    /// copied to the Carry flag. Bit 0 is set to 1.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sll_E()
    {
        Regs.E = Sll8(Regs.E);
    }

    /// <summary>
    /// "sll h" operation (0x34)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register H. The contents of bit 7 are
    /// copied to the Carry flag. Bit 0 is set to 1.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sll_H()
    {
        Regs.H = Sll8(Regs.H);
    }

    /// <summary>
    /// "sll l" operation (0x35)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register L. The contents of bit 7 are
    /// copied to the Carry flag. Bit 0 is set to 1.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sll_L()
    {
        Regs.L = Sll8(Regs.L);
    }

    /// <summary>
    /// "sll (hl)" operation (0x36)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents the memory address specified by the
    /// contents of HL. The contents of bit 7 are copied to the Carry flag. Bit 0 is set to 1.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte.
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4,hl(write):3
    /// </remarks>
    private void Sll_HLi()
    {
        var tmp = Sll8(ReadMemory(Regs.HL));
        TactPlus1(Regs.HL);
        WriteMemory(Regs.HL, tmp);
    }

    /// <summary>
    /// "sll a" operation (0x37)
    /// </summary>
    /// <remarks>
    /// An arithmetic shift left 1 bit position is performed on the contents of register A. The contents of bit 7 are
    /// copied to the Carry flag. Bit 0 is set to 1.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Sll_A()
    {
        Regs.A = Sll8(Regs.A);
    }

    /// <summary>
    /// "srl b" operation (0x38)
    /// </summary>
    /// <remarks>
    /// The contents of register B are shifted right 1 bit position.The contents of bit 0 are copied to the Carry flag,
    /// and bit 7 is reset.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Srl_B()
    {
        Regs.B = Srl8(Regs.B);
    }

    /// <summary>
    /// "srl c" operation (0x39)
    /// </summary>
    /// <remarks>
    /// The contents of register C are shifted right 1 bit position.The contents of bit 0 are copied to the Carry flag,
    /// and bit 7 is reset.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Srl_C()
    {
        Regs.C = Srl8(Regs.C);
    }

    /// <summary>
    /// "srl d" operation (0x3A)
    /// </summary>
    /// <remarks>
    /// The contents of register D are shifted right 1 bit position.The contents of bit 0 are copied to the Carry flag,
    /// and bit 7 is reset.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Srl_D()
    {
        Regs.D = Srl8(Regs.D);
    }

    /// <summary>
    /// "srl e" operation (0x3B)
    /// </summary>
    /// <remarks>
    /// The contents of register E are shifted right 1 bit position.The contents of bit 0 are copied to the Carry flag,
    /// and bit 7 is reset.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Srl_E()
    {
        Regs.E = Srl8(Regs.E);
    }

    /// <summary>
    /// "srl h" operation (0x3C)
    /// </summary>
    /// <remarks>
    /// The contents of register H are shifted right 1 bit position.The contents of bit 0 are copied to the Carry flag,
    /// and bit 7 is reset.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Srl_H()
    {
        Regs.H = Srl8(Regs.H);
    }

    /// <summary>
    /// "srl l" operation (0x3D)
    /// </summary>
    /// <remarks>
    /// The contents of register L are shifted right 1 bit position.The contents of bit 0 are copied to the Carry flag,
    /// and bit 7 is reset.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Srl_L()
    {
        Regs.L = Srl8(Regs.L);
    }

    /// <summary>
    /// "srl (hl)" operation (0x3E)
    /// </summary>
    /// <remarks>
    /// The contents the memory address specified by the contents of HL are shifted right 1 bit position. The contents
    /// of bit 0 are copied to the Carry flag, and bit 7 is reset.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the source byte
    /// 
    /// T-States: 15 (4, 4, 4, 3)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1,hl(write):3
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4,hl(write):3
    /// </remarks>
    private void Srl_HLi()
    {
        var tmp = Srl8(ReadMemory(Regs.HL));
        TactPlus1(Regs.HL);
        WriteMemory(Regs.HL, tmp);
    }

    /// <summary>
    /// "srl a" operation (0x3F)
    /// </summary>
    /// <remarks>
    /// The contents of register A are shifted right 1 bit position.The contents of bit 0 are copied to the Carry flag,
    /// and bit 7 is reset.
    /// 
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of the original register value.
    /// 
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Srl_A()
    {
        Regs.A = Srl8(Regs.A);
    }

    /// <summary>
    /// "bit N,Q" operations (0x40-0x7F)
    /// </summary>
    /// <remarks>
    /// 
    /// This instruction tests bit N in register Q and sets the Z flag accordingly.
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
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Bit0_B()
    {
        Bit8(0, Regs.B);
    }

    private void Bit0_C()
    {
        Bit8(0, Regs.C);
    }

    private void Bit0_D()
    {
        Bit8(0, Regs.D);
    }

    private void Bit0_E()
    {
        Bit8(0, Regs.E);
    }

    private void Bit0_H()
    {
        Bit8(0, Regs.H);
    }

    private void Bit0_L()
    {
        Bit8(0, Regs.L);
    }

    private void Bit0_A()
    {
        Bit8(0, Regs.A);
    }

    private void Bit1_B()
    {
        Bit8(1, Regs.B);
    }

    private void Bit1_C()
    {
        Bit8(1, Regs.C);
    }

    private void Bit1_D()
    {
        Bit8(1, Regs.D);
    }

    private void Bit1_E()
    {
        Bit8(1, Regs.E);
    }

    private void Bit1_H()
    {
        Bit8(1, Regs.H);
    }

    private void Bit1_L()
    {
        Bit8(1, Regs.L);
    }

    private void Bit1_A()
    {
        Bit8(1, Regs.A);
    }

    private void Bit2_B()
    {
        Bit8(2, Regs.B);
    }

    private void Bit2_C()
    {
        Bit8(2, Regs.C);
    }

    private void Bit2_D()
    {
        Bit8(2, Regs.D);
    }

    private void Bit2_E()
    {
        Bit8(2, Regs.E);
    }

    private void Bit2_H()
    {
        Bit8(2, Regs.H);
    }

    private void Bit2_L()
    {
        Bit8(2, Regs.L);
    }

    private void Bit2_A()
    {
        Bit8(2, Regs.A);
    }

    private void Bit3_B()
    {
        Bit8(3, Regs.B);
    }

    private void Bit3_C()
    {
        Bit8(3, Regs.C);
    }

    private void Bit3_D()
    {
        Bit8(3, Regs.D);
    }

    private void Bit3_E()
    {
        Bit8(3, Regs.E);
    }

    private void Bit3_H()
    {
        Bit8(3, Regs.H);
    }

    private void Bit3_L()
    {
        Bit8(3, Regs.L);
    }

    private void Bit3_A()
    {
        Bit8(3, Regs.A);
    }

    private void Bit4_B()
    {
        Bit8(4, Regs.B);
    }

    private void Bit4_C()
    {
        Bit8(4, Regs.C);
    }

    private void Bit4_D()
    {
        Bit8(4, Regs.D);
    }

    private void Bit4_E()
    {
        Bit8(4, Regs.E);
    }

    private void Bit4_H()
    {
        Bit8(4, Regs.H);
    }

    private void Bit4_L()
    {
        Bit8(4, Regs.L);
    }

    private void Bit4_A()
    {
        Bit8(4, Regs.A);
    }

    private void Bit5_B()
    {
        Bit8(5, Regs.B);
    }

    private void Bit5_C()
    {
        Bit8(5, Regs.C);
    }

    private void Bit5_D()
    {
        Bit8(5, Regs.D);
    }

    private void Bit5_E()
    {
        Bit8(5, Regs.E);
    }

    private void Bit5_H()
    {
        Bit8(5, Regs.H);
    }

    private void Bit5_L()
    {
        Bit8(5, Regs.L);
    }

    private void Bit5_A()
    {
        Bit8(5, Regs.A);
    }

    private void Bit6_B()
    {
        Bit8(6, Regs.B);
    }

    private void Bit6_C()
    {
        Bit8(6, Regs.C);
    }

    private void Bit6_D()
    {
        Bit8(6, Regs.D);
    }

    private void Bit6_E()
    {
        Bit8(6, Regs.E);
    }

    private void Bit6_H()
    {
        Bit8(6, Regs.H);
    }

    private void Bit6_L()
    {
        Bit8(6, Regs.L);
    }

    private void Bit6_A()
    {
        Bit8(6, Regs.A);
    }

    private void Bit7_B()
    {
        Bit8(7, Regs.B);
    }

    private void Bit7_C()
    {
        Bit8(7, Regs.C);
    }

    private void Bit7_D()
    {
        Bit8(7, Regs.D);
    }

    private void Bit7_E()
    {
        Bit8(7, Regs.E);
    }

    private void Bit7_H()
    {
        Bit8(7, Regs.H);
    }

    private void Bit7_L()
    {
        Bit8(7, Regs.L);
    }

    private void Bit7_A()
    {
        Bit8(7, Regs.A);
    }

    /// <summary>
    /// "bit N,(hl)" operation
    /// </summary>
    /// <remarks>
    /// 
    /// This instruction tests bit b in the memory location specified by the contents of HL and sets the Z flag
    /// accordingly.
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
    /// T-States: 12 (4, 4, 4)
    /// Contention breakdown: pc:4,pc+1:4,hl:3,hl:1
    /// Gate array contention breakdown: pc:4,pc+1:4,hl:4
    /// </remarks>
    private void Bit0_HLi()
    {
        byte tmp = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Bit8W(0, tmp);
    }

    private void Bit1_HLi()
    {
        byte tmp = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Bit8W(1, tmp);
    }

    private void Bit2_HLi()
    {
        byte tmp = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Bit8W(2, tmp);
    }

    private void Bit3_HLi()
    {
        byte tmp = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Bit8W(3, tmp);
    }

    private void Bit4_HLi()
    {
        byte tmp = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Bit8W(4, tmp);
    }

    private void Bit5_HLi()
    {
        byte tmp = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Bit8W(5, tmp);
    }

    private void Bit6_HLi()
    {
        byte tmp = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Bit8W(6, tmp);
    }

    private void Bit7_HLi()
    {
        byte tmp = ReadMemory(Regs.HL);
        TactPlus1(Regs.HL);
        Bit8W(7, tmp);
    }

    /// <summary>
    /// "res N,Q" operation
    /// </summary>
    /// <remarks>
    /// Bit N in register Q is reset.
    /// 
    /// Q: 000=B, 001=C, 010=D, 011=E
    ///    100=H, 101=L, 110=N/A, 111=A
    /// T-States: 8 (4, 4)
    /// Contention breakdown: pc:4,pc+1:4
    /// </remarks>
    private void Res0B()
    {
        Regs.B &= 0xfe;
    }

    private void Res0C()
    {
        Regs.C &= 0xfe;
    }

    private void Res0D()
    {
        Regs.D &= 0xfe;
    }

    private void Res0E()
    {
        Regs.E &= 0xfe;
    }

    private void Res0H()
    {
        Regs.H &= 0xfe;
    }

    private void Res0L()
    {
        Regs.L &= 0xfe;
    }

    private void Res0A()
    {
        Regs.A &= 0xfe;
    }

    private void Res1B()
    {
        Regs.B &= 0xfd;
    }

    private void Res1C()
    {
        Regs.C &= 0xfd;
    }

    private void Res1D()
    {
        Regs.D &= 0xfd;
    }

    private void Res1E()
    {
        Regs.E &= 0xfd;
    }

    private void Res1H()
    {
        Regs.H &= 0xfd;
    }

    private void Res1L()
    {
        Regs.L &= 0xfd;
    }

    private void Res1A()
    {
        Regs.B &= 0xfd;
    }

    private void Res2B()
    {
        Regs.B &= 0xfb;
    }

    private void Res2C()
    {
        Regs.C &= 0xfb;
    }

    private void Res2D()
    {
        Regs.D &= 0xfb;
    }

    private void Res2E()
    {
        Regs.E &= 0xfb;
    }

    private void Res2H()
    {
        Regs.H &= 0xfb;
    }

    private void Res2L()
    {
        Regs.L &= 0xfb;
    }

    private void Res2A()
    {
        Regs.A &= 0xfb;
    }

    private void Res3B()
    {
        Regs.B &= 0xf7;
    }

    private void Res3C()
    {
        Regs.C &= 0xf7;
    }

    private void Res3D()
    {
        Regs.D &= 0xf7;
    }

    private void Res3E()
    {
        Regs.E &= 0xf7;
    }

    private void Res3H()
    {
        Regs.H &= 0xf7;
    }

    private void Res3L()
    {
        Regs.L &= 0xf7;
    }

    private void Res3A()
    {
        Regs.B &= 0xf7;
    }

    private void Res4B()
    {
        Regs.B &= 0xef;
    }

    private void Res4C()
    {
        Regs.C &= 0xef;
    }

    private void Res4D()
    {
        Regs.D &= 0xef;
    }

    private void Res4E()
    {
        Regs.E &= 0xef;
    }

    private void Res4H()
    {
        Regs.H &= 0xef;
    }

    private void Res4L()
    {
        Regs.L &= 0xef;
    }

    private void Res4A()
    {
        Regs.A &= 0xef;
    }

    private void Res5B()
    {
        Regs.B &= 0xdf;
    }

    private void Res5C()
    {
        Regs.C &= 0xdf;
    }

    private void Res5D()
    {
        Regs.D &= 0xdf;
    }

    private void Res5E()
    {
        Regs.E &= 0xdf;
    }

    private void Res5H()
    {
        Regs.H &= 0xdf;
    }

    private void Res5L()
    {
        Regs.L &= 0xdf;
    }

    private void Res5A()
    {
        Regs.A &= 0xdf;
    }

    private void Res6B()
    {
        Regs.B &= 0xbf;
    }

    private void Res6C()
    {
        Regs.C &= 0xbf;
    }

    private void Res6D()
    {
        Regs.D &= 0xbf;
    }

    private void Res6E()
    {
        Regs.E &= 0xbf;
    }

    private void Res6H()
    {
        Regs.H &= 0xbf;
    }

    private void Res6L()
    {
        Regs.L &= 0xbf;
    }

    private void Res6A()
    {
        Regs.A &= 0xbf;
    }

    private void Res7B()
    {
        Regs.B &= 0x7f;
    }

    private void Res7C()
    {
        Regs.C &= 0x7f;
    }

    private void Res7D()
    {
        Regs.D &= 0x7f;
    }

    private void Res7E()
    {
        Regs.E &= 0x7f;
    }

    private void Res7H()
    {
        Regs.H &= 0x7f;
    }

    private void Res7L()
    {
        Regs.L &= 0x7f;
    }

    private void Res7A()
    {
        Regs.A &= 0x7f;
    }

}