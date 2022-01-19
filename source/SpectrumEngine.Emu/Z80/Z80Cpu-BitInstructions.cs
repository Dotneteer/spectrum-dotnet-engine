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
}
