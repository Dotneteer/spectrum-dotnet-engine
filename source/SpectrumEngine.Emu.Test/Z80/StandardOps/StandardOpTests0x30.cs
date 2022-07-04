namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0x30
{
    /// <summary>
    /// JR NC,E: 0x30
    /// </summary>
    [Fact]
    public void JR_NC_E_WorksWithNoJump()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
                0x37,       // SCF 
                0x30, 0x02  // JR NC,02H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// JR NC,E: 0x30
    /// </summary>
    [Fact]
    public void JR_NC_E_WorksWithJump()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x37,       // SCF
            0x3F,       // CCF 
            0x30, 0x02  // JR NC,02H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// LD SP,NN: 0x31
    /// </summary>
    [Fact]
    public void LD_SP_NN_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x31, 0x26, 0xA9 // LD SP,A926H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory();

        regs.SP.ShouldBe((ushort)0xA926);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(10UL);
    }

    /// <summary>
    /// LD (NN),A: 0x32
    /// </summary>
    [Fact]
    public void LD_NNi_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0xA9,       // LD A,A9H
            0x32, 0x00, 0x10  // LD (1000H),A
        });

        // --- Act
        var before = m.Memory[0x1000];
        m.Run();
        var after = m.Memory[0x1000];

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A");
        m.ShouldKeepMemory(except: "1000");

        before.ShouldBe((byte)0x00);
        after.ShouldBe((byte)0xA9);

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// INC SP: 0x33
    /// </summary>
    [Fact]
    public void INC_SP_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x31, 0x26, 0xA9, // LD SP,A926H
            0x33              // INC SP
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory();

        regs.SP.ShouldBe((ushort)0xA927);
        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(16UL);
    }

    /// <summary>
    /// INC (HL): 0x34
    /// </summary>
    [Fact]
    public void INC_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x34              // INC (HL)
        });
        m.Memory[0x1000] = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory(except: "1000");
        m.Memory[0x1000].ShouldBe((byte)0x24);

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(21UL);
    }

    /// <summary>
    /// DEC (HL): 0x35
    /// </summary>
    [Fact]
    public void DEC_HLi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x35              // DEC (HL)
        });
        m.Memory[0x1000] = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory(except: "1000");
        m.Memory[0x1000].ShouldBe((byte)0x22);

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(21UL);
    }

    /// <summary>
    /// LD (HL),N: 0x36
    /// </summary>
    [Fact]
    public void LD_HLi_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x00, 0x10, // LD HL,1000H
            0x36, 0x56        // LD (HL),56H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory(except: "1000");
        m.Memory[0x1000].ShouldBe((byte)0x56);

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// SCF: 0x37
    /// </summary>
    [Fact]
    public void SCF_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x37 // SCF
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F");
        regs.IsCFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0001);
        m.Cpu.Tacts.ShouldBe(4UL);
    }

    /// <summary>
    /// JR C,E: 0x38
    /// </summary>
    [Fact]
    public void JR_C_E_WorksWithNoJump()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x37,       // SCF
            0x3F,       // CCF 
            0x38, 0x02  // JR C,02H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// JR C,E: 0x38
    /// </summary>
    [Fact]
    public void JR_C_E_WorksWithJump()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x37,       // SCF 
            0x38, 0x02  // JR C,02H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(16UL);
    }

    /// <summary>
    /// ADD HL,SP: 0x39
    /// </summary>
    [Fact]
    public void ADD_HL_SP_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x34, 0x12, // LD HL,1234H
            0x31, 0x45, 0x23, // LD SP,2345H
            0x39              // ADD HL,SP
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F, HL, SP");
        m.ShouldKeepMemory();

        regs.HL.ShouldBe((ushort)0x3579);
        regs.PC.ShouldBe((ushort)0x0007);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// LD A,(NN): 0x3A
    /// </summary>
    [Fact]
    public void LD_A_NNi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3A, 0x00, 0x10 // LD A,(1000H)
        });
        m.Memory[0x1000] = 0x34;

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A");
        m.ShouldKeepMemory();

        regs.A.ShouldBe((byte)0x34);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(13UL);
    }

    /// <summary>
    /// DEC SP: 0x3B
    /// </summary>
    [Fact]
    public void DEC_SP_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x31, 0x26, 0xA9, // LD SP,A926H
            0x3B              // DEC SP
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory();

        regs.SP.ShouldBe((ushort)0xA925);
        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(16UL);
    }

    /// <summary>
    /// INC A: 0x3C
    /// </summary>
    [Fact]
    public void INC_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x43, // LD L,43H
            0x3C        // INC A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.A.ShouldBe((byte)0x44);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// DEC A: 0x3D
    /// </summary>
    [Fact]
    public void DEC_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x43, // LD A,43H
            0x3D        // DEC A
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeTrue();

        regs.A.ShouldBe((byte)0x42);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD A,N: 0x3E
    /// </summary>
    [Fact]
    public void LD_L_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0x3E, 0x26 // LD A,26H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A");
        m.ShouldKeepMemory();

        regs.A.ShouldBe((byte)0x26);
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(7UL);
    }

    /// <summary>
    /// CCF: 0x3F
    /// </summary>
    [Fact]
    public void CCF_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x37, // SCF
            0x3F  // CCF
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }
}
