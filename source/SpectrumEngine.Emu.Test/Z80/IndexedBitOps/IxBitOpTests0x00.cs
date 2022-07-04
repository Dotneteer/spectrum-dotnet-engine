namespace SpectrumEngine.Emu.Test.Z80;

public class IxBitOpTests0x00
{
    /// <summary>
    /// RLC (IX+D),B: 0xDD 0xCB 0x00
    /// </summary>
    [Fact]
    public void XRLC_B_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x00 // RLC (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.B.ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),B: 0xDD 0xCB 0x00
    /// </summary>
    [Fact]
    public void XRLC_B_WorksWithNegativeOffset()
    {
        // --- Arrange
        const byte OFFS = 0xFE;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x00 // RLC (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.IX = 0x1000;
        m.Memory[regs.IX - 256 + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX - 256 + OFFS]);
        regs.B.ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory(except: "0FFE");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),B: 0xDD 0xCB 0x00
    /// </summary>
    [Fact]
    public void XRLC_B_SetsCarry()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x00 // RLC (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x84;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.B.ShouldBe((byte)0x09);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),B: 0xDD 0xCB 0x00
    /// </summary>
    [Fact]
    public void XRLC_B_SetsZeroFlag()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x00 // RLC (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x00;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.B.ShouldBe((byte)0x00);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),B: 0xDD 0xCB 0x00
    /// </summary>
    [Fact]
    public void XRLC_B_SetsSignFlag()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x00 // RLC (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0xC0;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.B.ShouldBe((byte)0x81);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),C: 0xDD 0xCB 0x01
    /// </summary>
    [Fact]
    public void XRLC_C_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x01 // RLC (IX+32H),C
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.C.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.C.ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),D: 0xDD 0xCB 0x02
    /// </summary>
    [Fact]
    public void XRLC_D_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x02 // RLC (IX+32H),D
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.D.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.D.ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),E: 0xDD 0xCB 0x03
    /// </summary>
    [Fact]
    public void XRLC_E_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x03 // RLC (IX+32H),E
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.E.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.E.ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),H: 0xDD 0xCB 0x04
    /// </summary>
    [Fact]
    public void XRLC_H_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x04 // RLC (IX+32H),H
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.H.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.H.ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),L: 0xDD 0xCB 0x05
    /// </summary>
    [Fact]
    public void XRLC_L_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x05 // RLC (IX+32H),L
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.L.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.L.ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D): 0xDD 0xCB 0x06
    /// </summary>
    [Fact]
    public void XRLC_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x06 // RLC (IX+32H)
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[regs.IX + OFFS].ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RLC (IX+D),A: 0xDD 0xCB 0x07
    /// </summary>
    [Fact]
    public void XRLC_A_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x07 // RLC (IX+32H),A
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.A.ShouldBe((byte)0x10);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, A");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D),B: 0xDD 0xCB 0x08
    /// </summary>
    [Fact]
    public void XRRC_B_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x08 // RRC (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.B.ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D),B: 0xDD 0xCB 0x08
    /// </summary>
    [Fact]
    public void XRRC_B_WorksWithNegativeOffset()
    {
        // --- Arrange
        const byte OFFS = 0xFE;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x08 // RRC (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX - 256 + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX - 256 + OFFS]);
        regs.B.ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory(except: "0FFE");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D),C: 0xDD 0xCB 0x09
    /// </summary>
    [Fact]
    public void XRRC_C_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x09 // RRC (IX+32H),C
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.C.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.C.ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D),D: 0xDD 0xCB 0x0A
    /// </summary>
    [Fact]
    public void XRRC_D_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x0A // RRC (IX+32H),D
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.D.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.D.ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D),E: 0xDD 0xCB 0x0B
    /// </summary>
    [Fact]
    public void XRRC_E_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x0B // RRC (IX+32H),E
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.E.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.E.ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D),H: 0xDD 0xCB 0x0C
    /// </summary>
    [Fact]
    public void XRRC_H_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x0C // RRC (IX+32H),H
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.H.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.H.ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D),L: 0xDD 0xCB 0x0D
    /// </summary>
    [Fact]
    public void XRRC_L_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x0D // RRC (IX+32H),L
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.L.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.L.ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D): 0xDD 0xCB 0x0E
    /// </summary>
    [Fact]
    public void XRRC_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x0E // RLC (IX+32H)
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[regs.IX + OFFS].ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// RRC (IX+D),A: 0xDD 0xCB 0x0F
    /// </summary>
    [Fact]
    public void XRRC_A_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x0F // RRC (IX+32H),A
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.A.ShouldBe((byte)0x04);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, A");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

}
