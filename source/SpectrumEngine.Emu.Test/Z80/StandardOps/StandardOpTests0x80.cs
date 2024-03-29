﻿namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0x80
{
    /// <summary>
    /// ADD A,B: 0x80
    /// </summary>
    [Fact]
    public void ADD_A_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x06, 0x24, // LD B,24H
            0x80        // ADD A,B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x36);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,B: 0x80
    /// </summary>
    [Fact]
    public void ADD_A_B_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0xF0, // LD A,F0H
            0x06, 0xF0, // LD B,F0H
            0x80        // ADD A,B
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

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,B: 0x80
    /// </summary>
    [Fact]
    public void ADD_A_B_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x82, // LD A,82H
            0x06, 0x7E, // LD B,7EH
            0x80        // ADD A,B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x0);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,B: 0x80
    /// </summary>
    [Fact]
    public void ADD_A_B_HandlesSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x44, // LD A,44H
            0x06, 0x42, // LD B,42H
            0x80        // ADD A,B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x86);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,C: 0x81
    /// </summary>
    [Fact]
    public void ADD_A_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x0E, 0x24, // LD C,24H
            0x81        // ADD A,C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x36);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,D: 0x82
    /// </summary>
    [Fact]
    public void ADD_A_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x16, 0x24, // LD D,24H
            0x82        // ADD A,D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x36);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,E: 0x83
    /// </summary>
    [Fact]
    public void ADD_A_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x1E, 0x24, // LD E,24H
            0x83        // ADD A,E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x36);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,H: 0x84
    /// </summary>
    [Fact]
    public void ADD_A_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x26, 0x24, // LD H,24H
            0x84        // ADD A,H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x36);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,L: 0x85
    /// </summary>
    [Fact]
    public void ADD_A_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x2E, 0x24, // LD L,24H
            0x85        // ADD A,L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x36);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADD A,(HL): 0x86
    /// </summary>
    [Fact]
    public void ADD_A_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12,        // LD A,12H
            0x21, 0x00,  0x10, // LD HL,1000H
            0x86               // ADD A,(HL)
        });
        m.Memory[0x1000] = 0x24;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x36);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, HL");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// ADD A,A: 0x87
    /// </summary>
    [Fact]
    public void ADD_A_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x87        // ADD A,A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x24);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// ADC A,B: 0x88
    /// </summary>
    [Fact]
    public void ADC_A_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x06, 0x24, // LD B,24H
            0x88        // ADC A,B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x36);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADC A,B: 0x88
    /// </summary>
    [Fact]
    public void ADC_A_B_HandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0xF0, // LD A,F0H
            0x06, 0xF0, // LD B,F0H
            0x88        // ADC A,B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xE0);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADC A,B: 0x88
    /// </summary>
    [Fact]
    public void ADC_A_B_HandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x82, // LD A,82H
            0x06, 0x7E, // LD B,7EH
            0x88        // ADC A,B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x0);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADC A,B: 0x88
    /// </summary>
    [Fact]
    public void ADC_A_B_HandlesSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x44, // LD A,44H
            0x06, 0x42, // LD B,42H
            0x88        // ADC A,B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x86);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// ADC A,B: 0x88
    /// </summary>
    [Fact]
    public void ADC_A_B_WithCarryWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x06, 0x24, // LD B,24H
            0x37,       // SCF
            0x88        // ADC A,B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x37);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,B: 0x88
    /// </summary>
    [Fact]
    public void ADC_A_B_WithCarryHandlesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0xF0, // LD A,F0H
            0x06, 0xF0, // LD B,F0H
            0x37,       // SCF
            0x88        // ADC A,B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xE1);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,B: 0x88
    /// </summary>
    [Fact]
    public void ADC_A_B_WithCarryHandlesZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x82, // LD A,82H
            0x06, 0x7D, // LD B,7DH
            0x37,       // SCF
            0x88        // ADC A,B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x0);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,B: 0x88
    /// </summary>
    [Fact]
    public void ADC_A_B_WithCarryHandlesSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x44, // LD A,44H
            0x06, 0x42, // LD B,42H
            0x37,       // SCF
            0x88        // ADC A,B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x87);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,C: 0x89
    /// </summary>
    [Fact]
    public void ADC_A_C_WithCarryWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x0E, 0x24, // LD C,24H
            0x37,       // SCF
            0x89        // ADC A,C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x37);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,D: 0x8A
    /// </summary>
    [Fact]
    public void ADC_A_D_WithCarryWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x16, 0x24, // LD D,24H
            0x37,       // SCF
            0x8A        // ADC A,D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x37);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,E: 0x8B
    /// </summary>
    [Fact]
    public void ADC_A_E_WithCarryWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x1E, 0x24, // LD E,24H
            0x37,       // SCF
            0x8B        // ADC A,E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x37);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,H: 0x8C
    /// </summary>
    [Fact]
    public void ADC_A_H_WithCarryWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x26, 0x24, // LD H,24H
            0x37,       // SCF
            0x8C        // ADC A,H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x37);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,L: 0x8D
    /// </summary>
    [Fact]
    public void ADC_A_L_WithCarryWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x2E, 0x24, // LD L,24H
            0x37,       // SCF
            0x8D        // ADC A,L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x37);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADC A,(HL): 0x8E
    /// </summary>
    [Fact]
    public void ADC_A_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12,        // LD A,12H
            0x21, 0x00,  0x10, // LD HL,1000H
            0x37,              // SCF
            0x8E               // ADD A,(HL)
        });
        m.Memory[0x1000] = 0x24;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x37);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF, HL");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0007);
        m.Cpu.Tacts.ShouldBe(28UL);
    }

    /// <summary>
    /// ADC A,A: 0x8F
    /// </summary>
    [Fact]
    public void ADC_A_A_WithCarryWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x37,       // SCF
            0x8F        // ADC A,A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x25);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(15UL);
    }
}