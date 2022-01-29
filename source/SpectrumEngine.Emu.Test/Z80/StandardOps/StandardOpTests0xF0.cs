namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0xF0
{
    /// <summary>
    /// RET P: 0xF0
    /// </summary>
    [Fact]
    public void RET_P_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x32,       // LD A,32H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0x87,             // ADD A
            0xF0,             // RET P
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x64);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(43UL);
    }

    /// <summary>
    /// RET P: 0xF0
    /// </summary>
    [Fact]
    public void RET_P_DoesNotReturnWhenM()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0xC0,       // LD A,C0H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0x87,             // ADD A
            0xF0,             // RET P
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x24);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(54UL);
    }

    /// <summary>
    /// POP AF: 0xF1
    /// </summary>
    [Fact]
    public void POP_AF_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x01, 0x52, 0x23, // LD BC,2352H
            0xC5,             // PUSH BC
            0xF1              // POP AF
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.AF.ShouldBe((ushort)0x2352);
        m.ShouldKeepRegisters(except: "AF, BC");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// JP P,NN: 0xF2
    /// </summary>
    [Fact]
    public void JP_P_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x32,       // LD A,32H
            0x87,             // ADD A
            0xF2, 0x07, 0x00, // JP P,0007H
            0x76,             // HALT
            0x3E, 0xAA,       // LD A,AAH
            0x76              // HALT
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xAA);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0009);
        m.Cpu.Tacts.ShouldBe(32UL);
    }

    /// <summary>
    /// JP P,NN: 0xF2
    /// </summary>
    [Fact]
    public void JP_P_DoesNotJumpWhenM()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0xC0,       // LD A,C0H
            0x87,             // ADD A
            0xF2, 0x07, 0x00, // JP P,0007H
            0x76,             // HALT
            0x3E, 0xAA,       // LD A,AAH
            0x76              // HALT
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x80);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(25UL);
    }

    /// <summary>
    /// DI: 0xF3
    /// </summary>
    [Fact]
    public void DI_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0xF3 // DI
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Iff1 = true;
        m.Cpu.Iff2 = true;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBeFalse();
        m.Cpu.Iff2.ShouldBeFalse();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0001);
        m.Cpu.Tacts.ShouldBe(4UL);
    }

    /// <summary>
    /// CALL P: 0xF4
    /// </summary>
    [Fact]
    public void CALL_P_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x32,       // LD A,32H
            0x87,             // ADD A
            0xF4, 0x07, 0x00, // CALL P,0007H
            0x76,             // HALT
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x24);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(49UL);
    }

    /// <summary>
    /// CALL P: 0xF4
    /// </summary>
    [Fact]
    public void CALL_P_DoesNotCallWhenM()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0xC0,       // LD A,C0H
            0x87,             // ADD A
            0xF4, 0x07, 0x00, // CALL P,0007H
            0x76,             // HALT
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x80);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(25UL);
    }

    /// <summary>
    /// PUSH AF: 0xF5
    /// </summary>
    [Fact]
    public void PUSH_AF_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
                0xF5,             // PUSH AF
                0xC1              // POP BC
        });
        var regs = m.Cpu.Regs;
        regs.SP = 0;
        regs.AF = 0x3456;

        // --- Act
        m.Run();

        // --- Assert
        regs.BC.ShouldBe((ushort)0x3456);
        m.ShouldKeepRegisters(except: "BC");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(21UL);
    }

    /// <summary>
    /// OR N: 0xF6
    /// </summary>
    [Fact]
    public void OR_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xF6, 0x23  // OR 23H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x33);
        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeTrue();

        regs.HFlag.ShouldBeFalse();
        regs.NFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(14UL);
    }

    /// <summary>
    /// RST 30H: 0xF7
    /// </summary>
    [Fact]
    public void RST_30_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xF7        // RST 30H
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x12);

        regs.SP.ShouldBe((ushort)0xFFFE);
        regs.PC.ShouldBe((ushort)0x30);
        m.Memory[0xFFFE].ShouldBe((byte)0x03);
        m.Memory[0xFFFf].ShouldBe((byte)0x00);

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RET M: 0xF8
    /// </summary>
    [Fact]
    public void RET_M_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0xC0,       // LD A,C0H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0x87,             // ADD A
            0xF8,             // RET M
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x80);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(43UL);
    }

    /// <summary>
    /// RET M: 0xF8
    /// </summary>
    [Fact]
    public void RET_M_DoesNotReturnWhenP()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x32,       // LD A,32H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0x87,             // ADD A
            0xF8,             // RET M
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x24);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(54UL);
    }

    /// <summary>
    /// LD SP,HL: 0xF9
    /// </summary>
    [Fact]
    public void LD_SP_HL_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0xF9              // LD SP,HL
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.SP.ShouldBe((ushort)0x1000);
        m.ShouldKeepRegisters(except: "HL, SP");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(16UL);
    }

    /// <summary>
    /// JP M,NN: 0xFA
    /// </summary>
    [Fact]
    public void JP_M_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0xC0,       // LD A,C0H
            0x87,             // ADD A
            0xFA, 0x07, 0x00, // JP M,0007H
            0x76,             // HALT
            0x3E, 0xAA,       // LD A,AAH
            0x76              // HALT
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xAA);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0009);
        m.Cpu.Tacts.ShouldBe(32UL);
    }

    /// <summary>
    /// JP M,NN: 0xFA
    /// </summary>
    [Fact]
    public void JP_M_DoesNotJumpWhenP()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x32,       // LD A,32H
            0x87,             // ADD A
            0xFA, 0x07, 0x00, // JP M,0007H
            0x76,             // HALT
            0x3E, 0xAA,       // LD A,AAH
            0x76              // HALT
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x64);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(25UL);
    }

    /// <summary>
    /// EI: 0xFB
    /// </summary>
    [Fact]
    public void EI_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0xFB // EI
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.Cpu.Iff1.ShouldBeTrue();
        m.Cpu.Iff2.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0001);
        m.Cpu.Tacts.ShouldBe(4UL);
    }

    /// <summary>
    /// CALL M: 0xFC
    /// </summary>
    [Fact]
    public void CALL_M_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0xC0,       // LD A,C0H
            0x87,             // ADD A
            0xFC, 0x07, 0x00, // CALL M,0007H
            0x76,             // HALT
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x24);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(49UL);
    }

    /// <summary>
    /// CALL M: 0xFC
    /// </summary>
    [Fact]
    public void CALL_M_DoesNotCallWhenP()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x32,       // LD A,32H
            0x87,             // ADD A
            0xFC, 0x07, 0x00, // CALL M,0007H
            0x76,             // HALT
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x64);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(25UL);
    }

    /// <summary>
    /// CP N: 0xFE
    /// </summary>
    [Fact]
    public void CP_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
                0xFE, 0x24 // CP 24H
        });
        m.Cpu.Regs.A = 0x36;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.SFlag.ShouldBeFalse();
        regs.ZFlag.ShouldBeFalse();
        regs.HFlag.ShouldBeFalse();
        regs.PFlag.ShouldBeFalse();
        regs.CFlag.ShouldBeFalse();

        regs.NFlag.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(7UL);
    }

    /// <summary>
    /// RST 38H: 0xFF
    /// </summary>
    [Fact]
    public void RST_38_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xFF        // RST 38H
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x12);

        regs.SP.ShouldBe((ushort)0xFFFE);
        regs.PC.ShouldBe((ushort)0x38);
        m.Memory[0xFFFE].ShouldBe((byte)0x03);
        m.Memory[0xFFFf].ShouldBe((byte)0x00);

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        m.Cpu.Tacts.ShouldBe(18UL);
    }
}