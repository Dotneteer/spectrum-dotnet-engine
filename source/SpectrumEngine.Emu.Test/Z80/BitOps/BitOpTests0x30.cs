using static SpectrumEngine.Emu.Z80Cpu;

namespace SpectrumEngine.Emu.Test;

public class BitOpTests0x30
{
    /// <summary>
    /// SLL B: 0xCB 0x30
    /// </summary>
    [Fact]
    public void SLL_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x30 // SLL B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SLL B: 0xCB 0x30
    /// </summary>
    [Fact]
    public void SLL_B_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x30 // SLL B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x88;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeTrue();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SLL B: 0xCB 0x30
    /// </summary>
    [Fact]
    public void SLL_B_SetsSign()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x30 // SLL B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x48;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x91);

        regs.SFlag.ShouldBeTrue();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SLL C: 0xCB 0x31
    /// </summary>
    [Fact]
    public void SLL_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x31 // SLL C
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.C = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.C.ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SLL D: 0xCB 0x32
    /// </summary>
    [Fact]
    public void SLL_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x32 // SLL D
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.D = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.D.ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SLL E: 0xCB 0x33
    /// </summary>
    [Fact]
    public void SLL_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x33 // SLL E
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.E = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.E.ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SLL H: 0xCB 0x34
    /// </summary>
    [Fact]
    public void SLL_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x34 // SLL H
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.H = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.H.ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SLL L: 0xCB 0x35
    /// </summary>
    [Fact]
    public void SLL_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x35 // SLL L
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.L = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.L.ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SLL (HUL): 0xCB 0x36
    /// </summary>
    [Fact]
    public void SLL_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x36 // SLL (HUL)
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[regs.HL] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[regs.HL].ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// SLL A: 0xCB 0x37
    /// </summary>
    [Fact]
    public void SLL_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x37 // SLL A
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.A = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x11);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, A");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL B: 0xCB 0x38
    /// </summary>
    [Fact]
    public void SRL_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x38 // SRL B
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.B = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x08);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL B: 0xCB 0x38
    /// </summary>
    [Fact]
    public void SRL_B_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x38 // SRL B
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.B = 0x21;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x10);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeTrue();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL B: 0xCB 0x38
    /// </summary>
    [Fact]
    public void SRL_B_SetsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x38 // SRL B
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.B = 0x01;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x00);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeTrue();
        regs.CFlag.ShouldBeTrue();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL C: 0xCB 0x39
    /// </summary>
    [Fact]
    public void SRL_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x39 // SRL C
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.C = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.C.ShouldBe((byte)0x08);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL D: 0xCB 0x3A
    /// </summary>
    [Fact]
    public void SRL_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x3A // SRL D
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.D = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.D.ShouldBe((byte)0x08);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL E: 0xCB 0x3B
    /// </summary>
    [Fact]
    public void SRL_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x3B // SRL E
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.E = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.E.ShouldBe((byte)0x08);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL H: 0xCB 0x3C
    /// </summary>
    [Fact]
    public void SRL_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x3C // SRL H
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.H = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.H.ShouldBe((byte)0x08);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL L: 0xCB 0x3D
    /// </summary>
    [Fact]
    public void SRL_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x3D // SRL L
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.L = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.L.ShouldBe((byte)0x08);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// SRL (HUL): 0xCB 0x3E
    /// </summary>
    [Fact]
    public void SRL_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x3E // SRL (HUL)
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x1000;
        regs.F |= FlagsSetMask.C;
        m.Memory[regs.HL] = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[regs.HL].ShouldBe((byte)0x08);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// SRL A: 0xCB 0x3F
    /// </summary>
    [Fact]
    public void SRL_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x3F // SRL A
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.A = 0x10;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x08);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, A");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }
}