﻿namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0xD0
{
    /// <summary>
    /// RET NC: 0xD0
    /// </summary>
    [Fact]
    public void RET_NC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0xA7,             // AND A
            0xD0,             // RET NC
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
    /// RET NC: 0xD0
    /// </summary>
    [Fact]
    public void RET_NC_DoesNotReturnWhenC()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0x37,             // SCF
            0xD0,             // RET NC
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
    /// POP DE: 0xD1
    /// </summary>
    [Fact]
    public void POP_DE_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x52, 0x23, // LD HL,2352H
            0xE5,             // PUSH HL
            0xD1              // POP DE
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.DE.ShouldBe((ushort)0x2352);
        m.ShouldKeepRegisters(except: "HL, DE");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// JP NC,NN: 0xD2
    /// </summary>
    [Fact]
    public void JP_NC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xA7,             // AND A
            0xD2, 0x07, 0x00, // JP NC,0007H
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
    /// JP NC,NN: 0xD2
    /// </summary>
    [Fact]
    public void JP_NC_DoesNotJumpWhenC()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0x37,             // SCF
            0xD2, 0x07, 0x00, // JP NC,0007H
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
    /// OUT (N), A: 0xD3
    /// </summary>
    [Fact]
    public void OUT_N_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xD3, 0x28        // OUT (N),A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1628);
        m.IoAccessLog[0].Value.ShouldBe((byte)0x16);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// CALL NC: 0xD4
    /// </summary>
    [Fact]
    public void CALL_NC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xA7,             // AND A
            0xD4, 0x07, 0x00, // CALL NC,0007H
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
    /// CALL NC: 0xD4
    /// </summary>
    [Fact]
    public void CALL_NC_DoesNotCallWhenC()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0x37,             // SCF
            0xD4, 0x07, 0x00, // CALL NC,0007H
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
    /// PUSH DE: 0xD5
    /// </summary>
    [Fact]
    public void PUSH_DE_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x11, 0x52, 0x23, // LD DE,2352H
            0xD5,             // PUSH DE
            0xE1              // POP HL
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2352);
        m.ShouldKeepRegisters(except: "HL, DE");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// SUB N: 0xD6
    /// </summary>
    [Fact]
    public void SUB_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0xD6, 0x24  // SUB 24H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x12);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(14UL);
    }

    /// <summary>
    /// RST 10H: 0xD7
    /// </summary>
    [Fact]
    public void RST_10_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xD7        // RST 10H
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Regs.SP = 0;

        // --- Act
        m.Run();
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x12);

        regs.SP.ShouldBe((ushort)0xFFFE);
        regs.PC.ShouldBe((ushort)0x10);
        m.Memory[0xFFFE].ShouldBe((byte)0x03);
        m.Memory[0xFFFf].ShouldBe((byte)0x00);

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RET C: 0xD8
    /// </summary>
    [Fact]
    public void RET_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0x37,             // SCF
            0xD8,             // RET C
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
    /// RET C: 0xD8
    /// </summary>
    [Fact]
    public void RET_C_DoesNotReturnWhenNC()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xCD, 0x06, 0x00, // CALL 0006H
            0x76,             // HALT
            0xB7,             // OR A
            0xD8,             // RET C
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
    /// EXX: 0xD9
    /// </summary>
    [Fact]
    public void EXX_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xD9 // EXX
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0xABCD;
        regs._BC_ = 0x2345;
        regs.DE = 0xBCDE;
        regs._DE_ = 0x3456;
        regs.HL = 0xCDEF;
        regs._HL_ = 0x4567;

        // --- Act
        m.Run();

        // --- Assert
        regs.BC.ShouldBe((ushort)0x2345);
        regs._BC_.ShouldBe((ushort)0xABCD);
        regs.DE.ShouldBe((ushort)0x3456);
        regs._DE_.ShouldBe((ushort)0xBCDE);
        regs.HL.ShouldBe((ushort)0x4567);
        regs._HL_.ShouldBe((ushort)0xCDEF);

        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0001);
        m.Cpu.Tacts.ShouldBe(4UL);
    }

    /// <summary>
    /// JP C,NN: 0xDA
    /// </summary>
    [Fact]
    public void JP_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0x37,             // SCF
            0xDA, 0x07, 0x00, // JP C,0007H
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
    /// JP C,NN: 0xDA
    /// </summary>
    [Fact]
    public void JP_C_DoesNotJumpWhenNC()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xB7,             // OR A
            0xDA, 0x07, 0x00, // JP C,0007H
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
    /// IN A,(N): 0xDB
    /// </summary>
    [Fact]
    public void IN_A_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xDB, 0x34        // IN A,(34H)
        });
        m.IoInputSequence.Add(0xD5);

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0xD5);
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1634);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// CALL C: 0xDC
    /// </summary>
    [Fact]
    public void CALL_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0x37,             // SCF
            0xDC, 0x07, 0x00, // CALL C,0007H
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
    /// CALL C: 0xDC
    /// </summary>
    [Fact]
    public void CALL_C_DoesNotCallWhenNC()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
            0x3E, 0x16,       // LD A,16H
            0xB7,             // OR A
            0xDC, 0x07, 0x00, // CALL C,0007H
            0x76,             // HALT
            0x3E, 0x24,       // LD A,24H
            0xC9              // RET
        });
        var regs = m.Cpu.Regs;
        regs.SP = 0;

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
    /// SBC N: 0xDE
    /// </summary>
    [Fact]
    public void SBC_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x36, // LD A,36H
            0x37,       // SCF
            0xDE, 0x24  // SBC 24H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        regs.A.ShouldBe((byte)0x11);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RST 18H: 0xDF
    /// </summary>
    [Fact]
    public void RST_18_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x3E, 0x12, // LD A,12H
            0xDF        // RST 18H
        });
        var regs = m.Cpu.Regs;
        regs.SP = 0;

        // --- Act
        m.Run();
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x12);

        regs.SP.ShouldBe((ushort)0xFFFE);
        regs.PC.ShouldBe((ushort)0x18);
        m.Memory[0xFFFE].ShouldBe((byte)0x03);
        m.Memory[0xFFFf].ShouldBe((byte)0x00);

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        m.Cpu.Tacts.ShouldBe(18UL);
    }
}