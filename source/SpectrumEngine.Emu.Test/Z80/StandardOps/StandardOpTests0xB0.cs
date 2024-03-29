﻿namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0xB0
{
    /// <summary>
    /// OR B: 0xB0
    /// </summary>
    [Fact]
    public void OR_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x52, // LD A,52H
            0x06, 0x23, // LD B,23H
            0xB0        // OR B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x73);
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
    /// OR B: 0xB0
    /// </summary>
    [Fact]
    public void OR_B_WorksWithSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x82, // LD A,82H
            0x06, 0x22, // LD B,22H
            0xB0        // OR B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xA2);
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
    /// OR B: 0xB0
    /// </summary>
    [Fact]
    public void OR_B_WorksWithZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x00, // LD A,00H
            0x06, 0x00, // LD B,00H
            0xB0        // OR B
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
    /// OR B: 0xB0
    /// </summary>
    [Fact]
    public void OR_B_WorksWithPFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x32, // LD A,32H
            0x06, 0x11, // LD B,11H
            0xB0        // OR B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x33);
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
    /// OR C: 0xB1
    /// </summary>
    [Fact]
    public void OR_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x52, // LD A,52H
            0x0E, 0x23, // LD C,23H
            0xB1        // OR C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x73);
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
    /// OR D: 0xB2
    /// </summary>
    [Fact]
    public void OR_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x52, // LD A,52H
            0x16, 0x23, // LD D,23H
            0xB2        // OR D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x73);
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
    /// OR E: 0xB3
    /// </summary>
    [Fact]
    public void OR_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x52, // LD A,52H
            0x1E, 0x23, // LD E,23H
            0xB3        // OR E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x73);
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
    /// OR H: 0xB4
    /// </summary>
    [Fact]
    public void OR_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x52, // LD A,52H
            0x26, 0x23, // LD H,23H
            0xB4        // OR H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x73);
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
    /// OR L: 0xB5
    /// </summary>
    [Fact]
    public void OR_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x52, // LD A,52H
            0x2E, 0x23, // LD L,23H
            0xB5        // OR L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x73);
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
    /// OR (HUL): 0xB6
    /// </summary>
    [Fact]
    public void OR_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x52,       // LD A,52H
            0x21, 0x00, 0x10, // LD HL,1000H
            0xB6              // OR (HUL)
        });
        m.Memory[0x1000] = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x73);
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
    /// OR A: 0xB7
    /// </summary>

    [Fact]
    public void OR_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x52, // LD A,52H
            0xB7        // OR A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x52);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP B: 0xB8
    /// </summary>
    [Fact]
    public void CP_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x24, // LD B,24H
            0xB8        // CP B
        });
        m.Cpu.Regs.A = 0x36;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP B: 0xB8
    /// </summary>
    [Fact]
    public void CP_B_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x60, // LD B,60H
            0xB8        // CP B
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP B: 0xB8
    /// </summary>
    [Fact]
    public void CP_B_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x40, // LD B,40H
            0xB8        // CP B
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP B: 0xB8
    /// </summary>
    [Fact]
    public void CP_B_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x43, // LD B,43H
            0xB8        // CP B
        });
        m.Cpu.Regs.A = 0x41;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP B: 0xB8
    /// </summary>
    [Fact]
    public void CP_B_HandlesHPlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0xB3, // LD B,B3H
            0xB8        // CP B
        });
        m.Cpu.Regs.A = 0x61;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP C: 0xB9
    /// </summary>
    [Fact]
    public void CP_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x0E, 0x24, // LD C,24H
            0xB9        // CP C
        });
        m.Cpu.Regs.A = 0x36;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP C: 0xB9
    /// </summary>
    [Fact]
    public void CP_C_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x0E, 0x60, // LD C,60H
            0xB9        // CP C
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP C: 0xB9
    /// </summary>
    [Fact]
    public void CP_C_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x0E, 0x40, // LD C,40H
            0xB9        // CP B
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP C: 0xB9
    /// </summary>
    [Fact]
    public void CP_C_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x0E, 0x43, // LD C,43H
            0xB9        // CP C
        });
        m.Cpu.Regs.A = 0x41;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP C: 0xB9
    /// </summary>
    [Fact]
    public void CP_C_HandlesHPlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x0E, 0xB3, // LD C,B3H
            0xB9        // CP C
        });
        m.Cpu.Regs.A = 0x61;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP D: 0xBA
    /// </summary>
    [Fact]
    public void CP_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x16, 0x24, // LD D,24H
            0xBA        // CP D
        });
        m.Cpu.Regs.A = 0x36;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP D: 0xBA
    /// </summary>
    [Fact]
    public void CP_D_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x16, 0x60, // LD D,60H
            0xBA        // CP D
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP D: 0xBA
    /// </summary>
    [Fact]
    public void CP_D_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x16, 0x40, // LD D,40H
            0xBA        // CP D
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP D: 0xBA
    /// </summary>
    [Fact]
    public void CP_D_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x16, 0x43, // LD D,43H
            0xBA        // CP D
        });
        m.Cpu.Regs.A = 0x41;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP D: 0xBA
    /// </summary>
    [Fact]
    public void CP_D_HandlesHPlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x16, 0xB3, // LD D,B3H
            0xBA        // CP D
        });
        m.Cpu.Regs.A = 0x61;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP E: 0xBB
    /// </summary>
    [Fact]
    public void CP_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x1E, 0x24, // LD E,24H
            0xBB        // CP E
        });
        m.Cpu.Regs.A = 0x36;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP E: 0xBB
    /// </summary>
    [Fact]
    public void CP_E_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x1E, 0x60, // LD E,60H
            0xBB        // CP E
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP E: 0xBB
    /// </summary>
    [Fact]
    public void CP_E_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x1E, 0x40, // LD E,40H
            0xBB        // CP E
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP E: 0xBB
    /// </summary>
    [Fact]
    public void CP_E_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x1E, 0x43, // LD E,43H
            0xBB        // CP E
        });
        m.Cpu.Regs.A = 0x41;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP E: 0xBB
    /// </summary>
    [Fact]
    public void CP_E_HandlesHPlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x1E, 0xB3, // LD E,B3H
            0xBB        // CP E
        });
        m.Cpu.Regs.A = 0x61;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP H: 0xBC
    /// </summary>
    [Fact]
    public void CP_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x26, 0x24, // LD H,24H
            0xBC        // CP H
        });
        m.Cpu.Regs.A = 0x36;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP H: 0xBC
    /// </summary>
    [Fact]
    public void CP_H_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x26, 0x60, // LD H,60H
            0xBC        // CP H
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP H: 0xBC
    /// </summary>
    [Fact]
    public void CP_H_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x26, 0x40, // LD H,40H
            0xBC        // CP H
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP H: 0xBC
    /// </summary>
    [Fact]
    public void CP_H_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x26, 0x43, // LD H,43H
            0xBC        // CP H
        });
        m.Cpu.Regs.A = 0x41;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP H: 0xBC
    /// </summary>
    [Fact]
    public void CP_H_HandlesHPlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x26, 0xB3, // LD H,B3H
            0xBC        // CP H
        });
        m.Cpu.Regs.A = 0x61;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP L: 0xBD
    /// </summary>
    [Fact]
    public void CP_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x2E, 0x24, // LD L,24H
            0xBD        // CP L
        });
        m.Cpu.Regs.A = 0x36;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP L: 0xBD
    /// </summary>
    [Fact]
    public void CP_L_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x2E, 0x60, // LD L,60H
            0xBD        // CP L
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP L: 0xBD
    /// </summary>
    [Fact]
    public void CP_L_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x2E, 0x40, // LD L,40H
            0xBD        // CP L
        });
        m.Cpu.Regs.A = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP L: 0xBD
    /// </summary>
    [Fact]
    public void CP_L_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x2E, 0x43, // LD L,43H
            0xBD        // CP L
        });
        m.Cpu.Regs.A = 0x41;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP L: 0xBD
    /// </summary>
    [Fact]
    public void CP_L_HandlesHPlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x2E, 0xB3, // LD L,B3H
            0xBD        // CP L
        });
        m.Cpu.Regs.A = 0x61;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// CP (HUL): 0xBE
    /// </summary>
    [Fact]
    public void CP_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0xBE              // CP (HUL)
        });
        m.Cpu.Regs.A = 0x36;
        m.Memory[0x1000] = 0x24;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }

    /// <summary>
    /// CP (HUL): 0xBE
    /// </summary>
    [Fact]
    public void CP_HLi_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0xBE              // CP (HUL)
        });
        m.Cpu.Regs.A = 0x40;
        m.Memory[0x1000] = 0x60;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }

    /// <summary>
    /// CP (HUL): 0xBE
    /// </summary>
    [Fact]
    public void CP_HLi_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0xBE              // CP (HUL)
        });
        m.Cpu.Regs.A = 0x40;
        m.Memory[0x1000] = 0x40;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }

    /// <summary>
    /// CP (HUL): 0xBE
    /// </summary>
    [Fact]
    public void CP_HLi_HandlesHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0xBE              // CP (HUL)
        });
        m.Cpu.Regs.A = 0x41;
        m.Memory[0x1000] = 0x43;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }

    /// <summary>
    /// CP (HUL): 0xBE
    /// </summary>
    [Fact]
    public void CP_HLi_HandlesHPlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0xBE              // CP (HUL)
        });
        m.Cpu.Regs.A = 0x61;
        m.Memory[0x1000] = 0xB3;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }

    /// <summary>
    /// CP A: 0xBF
    /// </summary>
    [Fact]
    public void CP_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0xBF        // CP A
        });
        m.Cpu.Regs.A = 0x36;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0001);
        m.Cpu.Tacts.ShouldBe(4UL);
    }
}