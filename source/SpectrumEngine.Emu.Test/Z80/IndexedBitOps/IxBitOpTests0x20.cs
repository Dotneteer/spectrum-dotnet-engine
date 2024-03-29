﻿using static SpectrumEngine.Emu.Z80Cpu;

namespace SpectrumEngine.Emu.Test.Z80;

public class IxBitOpTests0x20
{
    /// <summary>
    /// SLA (IX+D),B: 0xDD 0xCB 0x20
    /// </summary>
    [Fact]
    public void XSLA_B_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x20 // SLA (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
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
    /// SLA (IX+D),C: 0xDD 0xCB 0x21
    /// </summary>
    [Fact]
    public void XSLA_C_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x21 // SLA (IX+32H),C
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
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
    /// SLA (IX+D),D: 0xDD 0xCB 0x22
    /// </summary>
    [Fact]
    public void XSLA_D_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x22 // SLA (IX+32H),D
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
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
    /// SLA (IX+D),E: 0xDD 0xCB 0x23
    /// </summary>
    [Fact]
    public void XSLA_E_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x23 // SLA (IX+32H),E
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
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
    /// SLA (IX+D),H: 0xDD 0xCB 0x24
    /// </summary>
    [Fact]
    public void XSLA_H_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x24 // SLA (IX+32H),H
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
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
    /// SLA (IX+D),L: 0xDD 0xCB 0x25
    /// </summary>
    [Fact]
    public void XSLA_L_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x25 // SLA (IX+32H),L
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
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
    /// SLA (IX+D): 0xDD 0xCB 0x26
    /// </summary>
    [Fact]
    public void XSLA_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x26 // SLA (IX+32H)
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
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
    /// SLA (IX+D),A: 0xDD 0xCB 0x27
    /// </summary>
    [Fact]
    public void XSLA_A_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x27 // SLA (IX+32H),A
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
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
    /// SRA (IX+D),B: 0xDD 0xCB 0x28
    /// </summary>
    [Fact]
    public void XSRA_B_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x28 // SRA (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.B.ShouldBe((byte)0x08);

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
    /// SRA (IX+D),C: 0xDD 0xCB 0x29
    /// </summary>
    [Fact]
    public void XSRA_C_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x29 // SRA (IX+32H),C
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.C.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.C.ShouldBe((byte)0x08);

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
    /// SRA (IX+D),D: 0xDD 0xCB 0x2A
    /// </summary>
    [Fact]
    public void XSRA_D_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x2A // SRA (IX+32H),D
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.D.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.D.ShouldBe((byte)0x08);

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
    /// SRA (IX+D),E: 0xDD 0xCB 0x2B
    /// </summary>
    [Fact]
    public void XSRA_E_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x2B // SRA (IX+32H),E
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.E.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.E.ShouldBe((byte)0x08);

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
    /// SRA (IX+D),H: 0xDD 0xCB 0x2C
    /// </summary>
    [Fact]
    public void XSRA_H_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x2C // SRA (IX+32H),H
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.H.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.H.ShouldBe((byte)0x08);

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
    /// SRA (IX+D),L: 0xDD 0xCB 0x2D
    /// </summary>
    [Fact]
    public void XSRA_L_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x2D // SRA (IX+32H),L
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.L.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.L.ShouldBe((byte)0x08);

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
    /// SRA (IX+D): 0xDD 0xCB 0x2E
    /// </summary>
    [Fact]
    public void XSRA_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x2E // SRA (IX+32H)
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[regs.IX + OFFS].ShouldBe((byte)0x08);

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
    /// SRA (IX+D),A: 0xDD 0xCB 0x2F
    /// </summary>
    [Fact]
    public void XSRA_A_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x2F // SRA (IX+32H),A
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.A.ShouldBe((byte)0x08);

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