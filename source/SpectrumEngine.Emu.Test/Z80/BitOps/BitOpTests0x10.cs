using static SpectrumEngine.Emu.Z80Cpu;

namespace SpectrumEngine.Emu.Test.Z80;

public class BitOpTests0x10
{
    /// <summary>
    /// RL B: 0xCB 0x10
    /// </summary>
    [Fact]
    public void RL_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x10 // RL B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x10);

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
    /// RL B: 0xCB 0x10
    /// </summary>
    [Fact]
    public void RL_B_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x10 // RL B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x84;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x08);

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
    /// RL B: 0xCB 0x10
    /// </summary>
    [Fact]
    public void RL_B_SetsZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x10 // RL B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x00;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x00);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeTrue();
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
    /// RL B: 0xCB 0x10
    /// </summary>
    [Fact]
    public void RL_B_SetsSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x10 // RL B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0xC0;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x80);

        regs.SFlag.ShouldBeTrue();
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
    /// RL B: 0xCB 0x10
    /// </summary>
    [Fact]
    public void RL_B_UsesCarryFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x10 // RL B
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.B = 0xC0;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x81);

        regs.SFlag.ShouldBeTrue();
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
    /// RL C: 0xCB 0x11
    /// </summary>
    [Fact]
    public void RL_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x11 // RL C
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.C = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.C.ShouldBe((byte)0x10);

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
    /// RL D: 0xCB 0x11
    /// </summary>
    [Fact]
    public void RL_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x12 // RL D
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.D = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.D.ShouldBe((byte)0x10);

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
    /// RL E: 0xCB 0x13
    /// </summary>
    [Fact]
    public void RL_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x13 // RL E
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.E = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.E.ShouldBe((byte)0x10);

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
    /// RL H: 0xCB 0x14
    /// </summary>
    [Fact]
    public void RL_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x14 // RL H
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.H = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.H.ShouldBe((byte)0x10);

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
    /// RL L: 0xCB 0x15
    /// </summary>
    [Fact]
    public void RL_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x15 // RL L
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.L = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.L.ShouldBe((byte)0x10);

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
    /// RL (HUL): 0xCB 0x16
    /// </summary>
    [Fact]
    public void RL_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x16 // RL (HUL)
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[regs.HL] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[regs.HL].ShouldBe((byte)0x10);

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
    /// RL A: 0xCB 0x17
    /// </summary>
    [Fact]
    public void RL_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x17 // RL A
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.A = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x10);

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

    /// <summary>
    /// RR B: 0xCB 0x18
    /// </summary>
    [Fact]
    public void RR_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x18 // RR B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x04);

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
    /// RR B: 0xCB 0x10
    /// </summary>
    [Fact]
    public void RR_B_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x18 // RR B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x85;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x42);

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
    /// RR B: 0xCB 0x18
    /// </summary>
    [Fact]
    public void RR_B_SetsZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x18 // RR B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x00;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x00);

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeTrue();
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
    /// RR B: 0xCB 0x18
    /// </summary>
    [Fact]
    public void RR_B_SetsSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x18 // RR B
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.B = 0xC0;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0xE0);

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
    /// RR C: 0xCB 0x19
    /// </summary>
    [Fact]
    public void RR_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x19 // RR C
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.C = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.C.ShouldBe((byte)0x04);

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
    /// RR D: 0xCB 0x1A
    /// </summary>
    [Fact]
    public void RR_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x1A // RR D
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.D = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.D.ShouldBe((byte)0x04);

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
    /// RR E: 0xCB 0x1B
    /// </summary>
    [Fact]
    public void RR_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x1B // RR E
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.E = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.E.ShouldBe((byte)0x04);

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
    /// RR H: 0xCB 0x1C
    /// </summary>
    [Fact]
    public void RR_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x1C // RR H
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.H = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.H.ShouldBe((byte)0x04);

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
    /// RR L: 0xCB 0x1D
    /// </summary>
    [Fact]
    public void RR_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x1D // RR L
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.L = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.L.ShouldBe((byte)0x04);

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
    /// RR (HUL): 0xCB 0x1E
    /// </summary>
    [Fact]
    public void RR_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x1E // RR (HUL)
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[regs.HL] = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[regs.HL].ShouldBe((byte)0x04);

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
    /// RR A: 0xCB 0x1F
    /// </summary>
    [Fact]
    public void RR_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x1F // RR A
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.A = 0x08;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x04);

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