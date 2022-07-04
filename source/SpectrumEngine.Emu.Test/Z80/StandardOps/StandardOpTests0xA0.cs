namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0xA0
{
    /// <summary>
    /// AND B: 0xA0
    /// </summary>
    [Fact]
    public void AND_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x06, 0x23, // LD B,23H
            0xA0        // AND B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x02);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND B: 0xA0
    /// </summary>
    [Fact]
    public void AND_B_HandlesSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0xF2, // LD A,F2H
            0x06, 0xF3, // LD B,F3H
            0xA0        // AND B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xF2);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND B: 0xA0
    /// </summary>
    [Fact]
    public void AND_B_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0xC3, // LD A,C3H
            0x06, 0x3C, // LD B,37H
            0xA0        // AND B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x00);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND B: 0xA0
    /// </summary>
    [Fact]
    public void AND_B_HandlesPFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x33, // LD A,33H
            0x06, 0x22, // LD B,22H
            0xA0        // AND B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x22);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND C: 0xA1
    /// </summary>
    [Fact]
    public void AND_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x0E, 0x23, // LD C,23H
            0xA1        // AND C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x02);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND D: 0xA2
    /// </summary>
    [Fact]
    public void AND_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x16, 0x23, // LD D,23H
            0xA2        // AND D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x02);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND E: 0xA3
    /// </summary>
    [Fact]
    public void AND_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x1E, 0x23, // LD E,23H
            0xA3        // AND E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x02);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND H: 0xA4
    /// </summary>
    [Fact]
    public void AND_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x26, 0x23, // LD H,23H
            0xA4        // AND H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x02);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND L: 0xA5
    /// </summary>
    [Fact]
    public void AND_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x2E, 0x23, // LD L,23H
            0xA5        // AND L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x02);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// AND (HL): 0xA6
    /// </summary>
    [Fact]
    public void AND_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12,        // LD A,12H
            0x21, 0x00, 0x10,  // LD HL,1000H
            0xA6               // AND (HL)
        });
        m.Memory[0x1000] = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x02);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// AND A: 0xA7
    /// </summary>
    [Fact]
    public void AND_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xA7        // AND A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// XOR B: 0xA8
    /// </summary>
    [Fact]
    public void XOR_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x06, 0x23, // LD B,23H
            0xA8        // XOR B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x31);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR B: 0xA0
    /// </summary>
    [Fact]
    public void XOR_B_HandlesSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0xF2, // LD A,F2H
            0x06, 0x03, // LD B,F3H
            0xA8        // XOR B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xF1);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR B: 0xA8
    /// </summary>
    [Fact]
    public void XOR_B_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x43, // LD A,C3H
            0x06, 0x43, // LD B,C3H
            0xA8        // XOR B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x00);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR B: 0xA8
    /// </summary>
    [Fact]
    public void XOR_B_HandlesPFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x33, // LD A,33H
            0x06, 0x22, // LD B,22H
            0xA8        // XOR B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR C: 0xA9
    /// </summary>
    [Fact]
    public void XOR_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x0E, 0x23, // LD C,23H
            0xA9        // XOR C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x31);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR D: 0xAA
    /// </summary>
    [Fact]
    public void XOR_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x16, 0x23, // LD D,23H
            0xAA        // XOR D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x31);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR E: 0xAB
    /// </summary>
    [Fact]
    public void XOR_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x1E, 0x23, // LD E,23H
            0xAB        // XOR E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x31);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR H: 0xAC
    /// </summary>
    [Fact]
    public void XOR_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x26, 0x23, // LD H,23H
            0xAC        // XOR H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x31);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR L: 0xAD
    /// </summary>
    [Fact]
    public void XOR_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x2E, 0x23, // LD L,23H
            0xAD        // XOR L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x31);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// XOR (HL): 0xAE
    /// </summary>
    [Fact]
    public void XOR_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12,       // LD A,12H
            0x21, 0x00, 0x10, // LD HL,1000H
            0xAE              // XOR (HL)
        });
        m.Memory[0x1000] = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x31);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// XOR A: 0xAF
    /// </summary>
    [Fact]
    public void XOR_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xAF        // XOR A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x00);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }
}