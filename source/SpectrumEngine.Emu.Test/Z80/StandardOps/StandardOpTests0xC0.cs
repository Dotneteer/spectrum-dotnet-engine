namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0xC0
{
    /// <summary>
    /// RET NZ: 0xC0
    /// </summary>
    [Fact]
    public void RET_NZ_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0xB7,             // OR A
            0xC0,             // RET NZ
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x16);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(43UL);
    }

    /// <summary>
    /// RET NZ: 0xC0
    /// </summary>
    [Fact]
    public void RET_NZ_DoesNotReturnWhenZ()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x00,       // LD A,00H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0xB7,             // OR A
            0xC0,             // RET NZ
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
    /// POP BC: 0xC1
    /// </summary>
    [Fact]
    public void POP_BC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x52, 0x23, // LD HL,2352H
            0xE5,             // PUSH HL
            0xC1              // POP BC
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.BC.ShouldBe((ushort)0x2352);
        m.ShouldKeepRegisters(except: "HL, BC");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// JP NZ,NN: 0xC2
    /// </summary>
    [Fact]
    public void JP_NZ_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xB7,             // OR A
            0xC2, 0x07, 0x00, // JP NZ,0007H
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
    /// JP NZ,NN: 0xC2
    /// </summary>
    [Fact]
    public void JP_NZ_DoesNotJumpWhenZ()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x00,       // LD A,00H
            0xB7,             // OR A
            0xC2, 0x07, 0x00, // JP NZ,0007H
            0x76,             // HALT
            0x3E, 0xAA,       // LD A,AAH
            0x76              // HALT
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x00);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(25UL);
    }

    /// <summary>
    /// JP NN: 0xC3
    /// </summary>
    [Fact]
    public void JP_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xC3, 0x06, 0x00, // JP 0006H
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

        regs.PC.ShouldBe((ushort)0x0008);
        m.Cpu.Tacts.ShouldBe(28UL);
    }

    /// <summary>
    /// CALL NZ: 0xC4
    /// </summary>
    [Fact]
    public void CALL_NZ_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xB7,             // OR A
            0xC4, 0x07, 0x00, // CALL NZ,0007H
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
    /// CALL NZ: 0xC4
    /// </summary>
    [Fact]
    public void CALL_NZ_DoesNotCallWhenZ()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x00,       // LD A,00H
                0xB7,             // OR A
                0xC4, 0x07, 0x00, // CALL NZ,0007H
                0x76,             // HALT
                0x3E, 0x24,       // LD A,24H
                0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x00);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(25UL);
    }

    /// <summary>
    /// PUSH BC: 0xC5
    /// </summary>
    [Fact]
    public void PUSH_BC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x01, 0x52, 0x23, // LD BC,2352H
            0xC5,             // PUSH BC
            0xE1              // POP HL
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2352);
        m.ShouldKeepRegisters(except: "HL, BC");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// ADD A,N: 0xC6
    /// </summary>
    [Fact]
    public void ADD_A_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xC6, 0x24 // ADD,24H
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
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(14UL);
    }

    /// <summary>
    /// RST 00H: 0xC7
    /// </summary>
    [Fact]
    public void RST_0_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0x3E, 0x12, // LD A,12H
                0xC7        // RST 0
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x12);

        regs.SP.ShouldBe((ushort)0xFFFE);
        regs.PC.ShouldBe((ushort)0);
        m.Memory[0xFFFE].ShouldBe((byte)0x03);
        m.Memory[0xFFFf].ShouldBe((byte)0x00);

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RET Z: 0xC8
    /// </summary>
    [Fact]
    public void RET_Z_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0xAF,             // XOR A
            0xC8,             // RET Z
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x00);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(43UL);
    }

    /// <summary>
    /// RET Z: 0xC8
    /// </summary>
    [Fact]
    public void RET_Z_DoesNotReturnWhenNZ()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0xB7,             // OR A
            0xC8,             // RET Z
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
    /// RET: 0xC9
    /// </summary>
    [Fact]
    public void RET_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(38UL);
    }

    /// <summary>
    /// JP Z,NN: 0xCA
    /// </summary>
    [Fact]
    public void JP_Z_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xAF,             // XOR A
            0xCA, 0x07, 0x00, // JP Z,0007H
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
    /// JP Z,NN: 0xCA
    /// </summary>
    [Fact]
    public void JP_Z_DoesNotJumpWhenNZ()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xB7,             // OR A
            0xCA, 0x07, 0x00, // JP Z,0007H
            0x76,             // HALT
            0x3E, 0xAA,       // LD A,AAH
            0x76              // HALT
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x16);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(25UL);
    }

    /// <summary>
    /// CALL Z: 0xCC
    /// </summary>
    [Fact]
    public void CALL_Z_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xAF,             // XOR A
            0xCC, 0x07, 0x00, // CALL Z,0007H
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
    /// CALL Z: 0xCC
    /// </summary>
    [Fact]
    public void CALL_Z_DoesNotCallWhenNZ()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xB7,             // OR A
            0xCC, 0x07, 0x00, // CALL Z,0007H
            0x76,             // HALT
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x16);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(25UL);
    }

    /// <summary>
    /// CALL NN: 0xCD
    /// </summary>
    [Fact]
    public void CALL_NN_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0x3E, 0xA3,       // LD A,A3H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0xA3);
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(45UL);
    }

    /// <summary>
    /// ADC A,N: 0xCE
    /// </summary>
    [Fact]
    public void ADC_A_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0x37,       // SCF
            0xCE, 0x24  // ADC,24H
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x37);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RST 08h: 0xCF
    /// </summary>
    [Fact]
    public void RST_8_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xCF        // RST 8
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x12);

        regs.SP.ShouldBe((ushort)0xFFFE);
        regs.PC.ShouldBe((ushort)8);
        m.Memory[0xFFFE].ShouldBe((byte)0x03);
        m.Memory[0xFFFf].ShouldBe((byte)0x00);

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        m.Cpu.Tacts.ShouldBe(18UL);
    }
}