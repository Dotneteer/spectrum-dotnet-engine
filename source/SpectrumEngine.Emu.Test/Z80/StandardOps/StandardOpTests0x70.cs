﻿namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0x70
{
    /// <summary>
    /// LD (HL),B: 0x70
    /// </summary>
    [Fact]
    public void LD_HLi_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x06, 0xB9,       // LD B,B9H
            0x70              // LD (HL),B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.Memory[0x1000].ShouldBe((byte)0xB9);
        m.ShouldKeepRegisters(except: "HL, B");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// LD (HL),C: 0x71
    /// </summary>
    [Fact]
    public void LD_HLi_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x0E, 0xB9,       // LD C,B9H
            0x71              // LD (HL),C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.Memory[0x1000].ShouldBe((byte)0xB9);
        m.ShouldKeepRegisters(except: "HL, C");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// LD (HL),D: 0x72
    /// </summary>
    [Fact]
    public void LD_HLi_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x16, 0xB9,       // LD D,B9H
            0x72              // LD (HL),D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.Memory[0x1000].ShouldBe((byte)0xB9);
        m.ShouldKeepRegisters(except: "HL, D");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// LD (HL),E: 0x73
    /// </summary>
    [Fact]
    public void LD_HLi_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x1E, 0xB9,       // LD E,B9H
            0x73              // LD (HL),E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.Memory[0x1000].ShouldBe((byte)0xB9);
        m.ShouldKeepRegisters(except: "HL, E");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// LD (HL),H: 0x74
    /// </summary>
    [Fact]
    public void LD_HLi_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x10, 0x22, // LD HL,2210H
            0x74              // LD (HL),H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.Memory[0x2210].ShouldBe((byte)0x22);
        m.ShouldKeepRegisters(except: "HL");
        m.ShouldKeepMemory(except: "2210");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }


    /// <summary>
    /// LD (HL),L: 0x75
    /// </summary>
    [Fact]
    public void LD_HLi_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x22, 0x10, // LD HL,1022H
            0x75              // LD (HL),L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.Memory[0x1022].ShouldBe((byte)0x22);
        m.ShouldKeepRegisters(except: "HL");
        m.ShouldKeepMemory(except: "1022");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }

    /// <summary>
    /// HALT: 0x76
    /// </summary>
    [Fact]
    public void HALTWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x76        // HALT
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.Halted.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0000);
        m.Cpu.Tacts.ShouldBe(4UL);
    }

    /// <summary>
    /// LD (HL),A: 0x77
    /// </summary>
    [Fact]
    public void LD_HLi_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x3E, 0xB9,       // LD A,B9H
            0x77              // LD (HL),A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.Memory[0x1000].ShouldBe((byte)0xB9);
        m.ShouldKeepRegisters(except: "A, HL");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// LD A,B: 0x78
    /// </summary>
    [Fact]
    public void LD_A_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0xB9, // LD B,B9H
            0x78        // LD A,B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xB9);

        m.ShouldKeepRegisters(except: "A, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD A,C: 0x79
    /// </summary>
    [Fact]
    public void LD_A_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x0E, 0xB9, // LD C,B9H
            0x79        // LD A,C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xB9);

        m.ShouldKeepRegisters(except: "A, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD A,D: 0x7A
    /// </summary>
    [Fact]
    public void LD_A_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x16, 0xB9, // LD D,B9H
            0x7A        // LD A,D
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xB9);

        m.ShouldKeepRegisters(except: "A, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD A,E: 0x7B
    /// </summary>
    [Fact]
    public void LD_A_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x1E, 0xB9, // LD E,B9H
            0x7B        // LD A,E
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xB9);

        m.ShouldKeepRegisters(except: "A, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD A,H: 0x7C
    /// </summary>
    [Fact]
    public void LD_A_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x26, 0xB9, // LD H,B9H
            0x7C        // LD A,H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xB9);

        m.ShouldKeepRegisters(except: "A, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD A,L: 0x7D
    /// </summary>
    [Fact]
    public void LD_A_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x2E, 0xB9, // LD L,B9H
            0x7D        // LD A,L
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xB9);

        m.ShouldKeepRegisters(except: "A, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD A,(HL): 0x7E
    /// </summary>
    [Fact]
    public void LD_A_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x7E              // LD A,(HL)
        });
        m.Memory[0x1000] = 0xB9;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xB9);

        m.ShouldKeepRegisters(except: "A, HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }
}