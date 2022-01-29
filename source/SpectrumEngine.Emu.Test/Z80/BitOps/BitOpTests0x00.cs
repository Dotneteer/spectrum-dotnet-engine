namespace SpectrumEngine.Emu.Test.Z80;

public class BitOpTests0x00
{
    /// <summary>
    /// RLC B: 0xCB 0x00
    /// </summary>
    [Fact]
    public void RLC_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0xCB, 0x00 // RLC B
        });
        var regs = m.Cpu.Regs;
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
    /// RLC B: 0xCB 0x00
    /// </summary>
    [Fact]
    public void RLC_B_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0xCB, 0x00 // RLC B
        });
        var regs = m.Cpu.Regs;
        regs.B = 0x84;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0x09);

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
    /// RLC B: 0xCB 0x00
    /// </summary>
    [Fact]
    public void RLC_B_SetsZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0xCB, 0x00 // RLC B
        });
        var regs = m.Cpu.Regs;
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
    /// RLC B: 0xCB 0x00
    /// </summary>
    [Fact]
    public void RLC_B_SetsSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x00 // RLC B
        });
        var regs = m.Cpu.Regs;
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
    /// RLC C: 0xCB 0x01
    /// </summary>
    [Fact]
    public void RLC_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x01 // RLC C
        });
        var regs = m.Cpu.Regs;
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
    /// RLC D: 0xCB 0x02
    /// </summary>
    [Fact]
    public void RLC_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x02 // RLC D
        });
        var regs = m.Cpu.Regs;
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
    /// RLC E: 0xCB 0x03
    /// </summary>
    [Fact]
    public void RLC_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x03 // RLC E
        });
        var regs = m.Cpu.Regs;
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
    /// RLC H: 0xCB 0x04
    /// </summary>
    [Fact]
    public void RLC_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x04 // RLC H
        });
        var regs = m.Cpu.Regs;
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
    /// RLC L: 0xCB 0x05
    /// </summary>
    [Fact]
    public void RLC_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x05 // RLC L
        });
        var regs = m.Cpu.Regs;
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
    /// RLC (HUL): 0xCB 0x06
    /// </summary>
    [Fact]
    public void RLC_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x06 // RLC (HUL)
        });
        var regs = m.Cpu.Regs;
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
    /// RLC A: 0xCB 0x07
    /// </summary>
    [Fact]
    public void RLC_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x07 // RLC A
        });
        var regs = m.Cpu.Regs;
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
    /// RRC B: 0xCB 0x08
    /// </summary>
    [Fact]
    public void RRC_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x08 // RRC B
        });
        var regs = m.Cpu.Regs;
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
    /// RRC B: 0xCB 0x08
    /// </summary>
    [Fact]
    public void RRC_B_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x08 // RRC B
        });
        var regs = m.Cpu.Regs;
        regs.B = 0x85;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0xC2);

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
    /// RRC B: 0xCB 0x08
    /// </summary>
    [Fact]
    public void RRC_B_SetsZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x00 // RLC B
        });
        var regs = m.Cpu.Regs;
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
    /// RRC B: 0xCB 0x08
    /// </summary>
    [Fact]
    public void RRC_B_SetsSignFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x08 // RRC B
        });
        var regs = m.Cpu.Regs;
        regs.B = 0x41;

        // --- Act
        m.Run();

        // --- Assert
        regs.B.ShouldBe((byte)0xA0);

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
    /// RRC C: 0xCB 0x09
    /// </summary>
    [Fact]
    public void RRC_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x09 // RRC C
        });
        var regs = m.Cpu.Regs;
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
    /// RRC D: 0xCB 0x0A
    /// </summary>
    [Fact]
    public void RRC_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x0A // RRC D
        });
        var regs = m.Cpu.Regs;
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
    /// RRC E: 0xCB 0x0B
    /// </summary>
    [Fact]
    public void RRC_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x0B // RRC E
        });
        var regs = m.Cpu.Regs;
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
    /// RRC H: 0xCB 0x0C
    /// </summary>
    [Fact]
    public void RRC_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x0C // RRC H
        });
        var regs = m.Cpu.Regs;
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
    /// RRC L: 0xCB 0x0D
    /// </summary>
    [Fact]
    public void RRC_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x0D // RRC L
        });
        var regs = m.Cpu.Regs;
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
    /// RRC (HUL): 0xCB 0x0E
    /// </summary>
    [Fact]
    public void RRC_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x0E // RRC (HUL)
        });
        var regs = m.Cpu.Regs;
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
    /// RRC A: 0xCB 0x0F
    /// </summary>
    [Fact]
    public void RRC_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xCB, 0x0F // RRC A
        });
        var regs = m.Cpu.Regs;
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