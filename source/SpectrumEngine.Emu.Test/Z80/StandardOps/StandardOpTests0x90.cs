namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0x90
{
    /// <summary>
    /// SUB B: 0x90
    /// </summary>
    [Fact]
    public void SUB_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x06, 0x24, // LD B,24H
            0x90        // SUB B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB B: 0x90
    /// </summary>
    [Fact]
    public void SUB_B_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x40, // LD A,40H
            0x06, 0x60, // LD B,60H
            0x90        // SUB B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xE0);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB B: 0x90
    /// </summary>
    [Fact]
    public void SUB_B_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x40, // LD A,40H
            0x06, 0x40, // LD B,40H
            0x90        // SUB B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x00);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB B: 0x90
    /// </summary>
    [Fact]
    public void SUB_B_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x41, // LD A,41H
            0x06, 0x43, // LD B,43H
            0x90        // SUB B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xFE);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB B: 0x90
    /// </summary>
    [Fact]
    public void SUB_B_HandlesPFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x61, // LD A,61H
            0x06, 0xB3, // LD B,B3H
            0x90        // SUB B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xAE);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB C: 0x91
    /// </summary>
    [Fact]
    public void SUB_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
                0x3E, 0x36, // LD A,36H
                0x0E, 0x24, // LD C,24H
                0x91        // SUB C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB D: 0x92
    /// </summary>
    [Fact]
    public void SUB_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x16, 0x24, // LD D,24H
            0x92        // SUB D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB E: 0x93
    /// </summary>
    [Fact]
    public void SUB_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x1E, 0x24, // LD E,24H
            0x93        // SUB E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB H: 0x94
    /// </summary>
    [Fact]
    public void SUB_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x26, 0x24, // LD H,24H
            0x94        // SUB H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB L: 0x95
    /// </summary>
    [Fact]
    public void SUB_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x2E, 0x24, // LD L,24H
            0x95        // SUB L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// SUB (HL): 0x96
    /// </summary>
    [Fact]
    public void SUB_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36,       // LD A,36H
            0x21, 0x00, 0x10, // LD HL,1000H
            0x96              // SUB (HL)
        });
        m.Memory[0x1000] = 0x24;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, HL");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }


    /// <summary>
    /// SUB A: 0x97
    /// </summary>
    [Fact]
    public void SUB_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x97        // SUB A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x00);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// SBC B: 0x98
    /// </summary>
    [Fact]
    public void SBC_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
                0x3E, 0x36, // LD A,36H
                0x06, 0x24, // LD B,24H
                0x37,       // SCF
                0x98        // SBC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC B: 0x98
    /// </summary>
    [Fact]
    public void SBC_B_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
                0x3E, 0x40, // LD A,40H
                0x06, 0x60, // LD B,60H
                0x37,       // SCF
                0x98        // SBC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xDF);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC B: 0x98
    /// </summary>
    [Fact]
    public void SBC_B_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x40, // LD A,40H
            0x06, 0x3F, // LD B,3FH
            0x37,       // SCF
            0x98        // SBC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x00);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC B: 0x98
    /// </summary>
    [Fact]
    public void SBC_B_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x41, // LD A,41H
            0x06, 0x43, // LD B,43H
            0x37,       // SCF
            0x98        // SBC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xFD);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC B: 0x98
    /// </summary>
    [Fact]
    public void SBC_B_HandlesPFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x61, // LD A,61H
            0x06, 0xB3, // LD B,B3H
            0x37,       // SCF
            0x98        // SBC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xAD);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC C: 0x99
    /// </summary>
    [Fact]
    public void SBC_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x0E, 0x24, // LD C,24H
            0x37,       // SCF
            0x99        // SBC C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC D: 0x9A
    /// </summary>
    [Fact]
    public void SBC_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x16, 0x24, // LD D,24H
            0x37,       // SCF
            0x9A        // SBC D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC E: 0x9B
    /// </summary>
    [Fact]
    public void SBC_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x1E, 0x24, // LD E,24H
            0x37,       // SCF
            0x9B        // SBC E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC H: 0x9C
    /// </summary>
    [Fact]
    public void SBC_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x26, 0x24, // LD H,24H
            0x37,       // SCF
            0x9C        // SBC H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC L: 0x9D
    /// </summary>
    [Fact]
    public void SBC_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x2E, 0x24, // LD L,24H
            0x37,       // SCF
            0x9D        // SBC L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// SBC (HL): 0x9E
    /// </summary>
    [Fact]
    public void SBC_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36,       // LD A,36H
            0x21, 0x00, 0x10, // LD HL,1000H
            0x37,             // SCF
            0x9E              // SBC (HL)
        });
        m.Memory[0x1000] = 0x24;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0007);
        m.Cpu.Tacts.ShouldBe(28UL);
    }

    /// <summary>
    /// SBC A: 0x9F
    /// </summary>
    [Fact]
    public void SBC_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x37,       // SCF
            0x9F        // SBC A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xFF);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(15UL);
    }
}