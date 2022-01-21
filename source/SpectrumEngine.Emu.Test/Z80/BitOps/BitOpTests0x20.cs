namespace SpectrumEngine.Emu.Test;

public class BitOpTests0x20
{
    /// <summary>
    /// SLA B: 0xCB 0x20
    /// </summary>
    [Fact]
    public void SLA_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x20 // SLA B
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
    /// SLA B: 0xCB 0x20
    /// </summary>
    [Fact]
    public void SLA_B_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x20 // SLA B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x88;

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
    /// SLA B: 0xCB 0x20
    /// </summary>
    [Fact]
    public void SLA_B_SetsSign()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x20 // SLA B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x48;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x90);

        regs.SFlag.ShouldBeTrue();
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
    /// SLA B: 0xCB 0x20
    /// </summary>
    [Fact]
    public void SLA_B_SetsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x20 // SLA B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.B = 0x80;

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
    /// SLA C: 0xCB 0x21
    /// </summary>
    [Fact]
    public void SLA_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x21 // SLA C
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
    /// SLA D: 0xCB 0x22
    /// </summary>
    [Fact]
    public void SLA_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x22 // SLA D
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
    /// SLA E: 0xCB 0x23
    /// </summary>
    [Fact]
    public void SLA_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x23 // SLA E
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
    /// SLA H: 0xCB 0x24
    /// </summary>
    [Fact]
    public void SLA_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x24 // SLA H
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
    /// SLA L: 0xCB 0x25
    /// </summary>
    [Fact]
    public void SLA_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x25 // SLA L
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
    /// SLA (HUL): 0xCB 0x26
    /// </summary>
    [Fact]
    public void SLA_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x26 // SLA (HUL)
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
    /// SLA A: 0xCB 0x27
    /// </summary>
    [Fact]
    public void SLA_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x27 // SLA A
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
    /// SRA B: 0xCB 0x28
    /// </summary>
    [Fact]
    public void SRA_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x28 // SRA B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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
    /// SRA B: 0xCB 0x28
    /// </summary>
    [Fact]
    public void SRA_B_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x28 // SRA B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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
    /// SRA B: 0xCB 0x28
    /// </summary>
    [Fact]
    public void SRA_B_SetsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x28 // SRA B
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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
    /// SRA C: 0xCB 0x29
    /// </summary>
    [Fact]
    public void SRA_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x29 // SRA C
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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
    /// SRA D: 0xCB 0x2A
    /// </summary>
    [Fact]
    public void SRA_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x2A // SRA D
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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
    /// SRA E: 0xCB 0x2B
    /// </summary>
    [Fact]
    public void SRA_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x2B // SRA E
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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
    /// SRA H: 0xCB 0x2C
    /// </summary>
    [Fact]
    public void SRA_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x2C // SRA H
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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
    /// SRA L: 0xCB 0x2D
    /// </summary>
    [Fact]
    public void SRA_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x2D // SRA L
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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
    /// SRA (HUL): 0xCB 0x2E
    /// </summary>
    [Fact]
    public void SRA_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x2E // SRA (HUL)
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
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
    /// SRA A: 0xCB 0x2F
    /// </summary>
    [Fact]
    public void SRA_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x2F // SRA A
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
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