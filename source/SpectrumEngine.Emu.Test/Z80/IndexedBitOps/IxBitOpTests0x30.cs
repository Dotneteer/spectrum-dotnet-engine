﻿using static SpectrumEngine.Emu.Z80Cpu;

namespace SpectrumEngine.Emu.Test.Z80;

public class IxBitOpTests0x30
{
    /// <summary>
    /// SLL (IX+D),B: 0xDD 0xCB 0x30
    /// </summary>
    [Fact]
    public void XSLL_B_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x30 // SLL (IX+32H),B
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.B.ShouldBe((byte)0x11);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
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
    /// SLL (IX+D),C: 0xDD 0xCB 0x31
    /// </summary>
    [Fact]
    public void XSLL_C_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x31 // SLL (IX+32H),C
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.C.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.C.ShouldBe((byte)0x11);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// SLL (IX+D),D: 0xDD 0xCB 0x32
    /// </summary>
    [Fact]
    public void XSLL_D_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x32 // SLL (IX+32H),D
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.D.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.D.ShouldBe((byte)0x11);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// SLL (IX+D),E: 0xDD 0xCB 0x33
    /// </summary>
    [Fact]
    public void XSLL_E_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x33 // SLL (IX+32H),E
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.E.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.E.ShouldBe((byte)0x11);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// SLL (IX+D),H: 0xDD 0xCB 0x34
    /// </summary>
    [Fact]
    public void XSLL_H_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x34 // SLL (IX+32H),H
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.H.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.H.ShouldBe((byte)0x11);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// SLL (IX+D),L: 0xDD 0xCB 0x35
    /// </summary>
    [Fact]
    public void XSLL_L_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x35 // SLL (IX+32H),L
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.L.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.L.ShouldBe((byte)0x11);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// SLL (IX+D): 0xDD 0xCB 0x36
    /// </summary>
    [Fact]
    public void XSLL_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x36 // SLL (IX+32H)
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[regs.IX + OFFS].ShouldBe((byte)0x11);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// SLL (IX+D),A: 0xDD 0xCB 0x37
    /// </summary>
    [Fact]
    public void XSLL_A_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x37 // SLL (IX+32H),A
        });
        var regs = m.Cpu.Regs;
        regs.IX = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.IX + OFFS] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe(m.Memory[regs.IX + OFFS]);
        regs.A.ShouldBe((byte)0x11);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, A");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// SRL (IX+D),B: 0xDD 0xCB 0x38
    /// </summary>
    [Fact]
    public void XSRL_B_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x38 // SRL (IX+32H),B
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
    /// SRL (IX+D),C: 0xDD 0xCB 0x39
    /// </summary>
    [Fact]
    public void XSRL_C_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x39 // SRL (IX+32H),C
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
    /// SRL (IX+D),D: 0xDD 0xCB 0x3A
    /// </summary>
    [Fact]
    public void XSRL_D_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x3A // SRL (IX+32H),D
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
    /// SRL (IX+D),E: 0xDD 0xCB 0x3B
    /// </summary>
    [Fact]
    public void XSRL_E_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x3B // SRL (IX+32H),E
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
    /// SRL (IX+D),H: 0xDD 0xCB 0x3C
    /// </summary>
    [Fact]
    public void XSRL_H_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x3C // SRL (IX+32H),H
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
    /// SRL (IX+D),L: 0xDD 0xCB 0x3D
    /// </summary>
    [Fact]
    public void XSRL_L_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x3D // SRL (IX+32H),L
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
    /// SRL (IX+D): 0xDD 0xCB 0x3E
    /// </summary>
    [Fact]
    public void XSRL_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x3E // SRL (IX+32H)
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
        m.ShouldKeepRegisters(except: "F, A");
        m.ShouldKeepMemory(except: "1032");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(23UL);
    }

    /// <summary>
    /// SRL (IX+D),A: 0xDD 0xCB 0x3F
    /// </summary>
    [Fact]
    public void XSRL_A_WorksAsExpected()
    {
        // --- Arrange
        const byte OFFS = 0x32;
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xDD, 0xCB, OFFS, 0x3F // SRL (IX+32H),A
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
