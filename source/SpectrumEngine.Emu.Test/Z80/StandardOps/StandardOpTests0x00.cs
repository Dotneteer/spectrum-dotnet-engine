﻿namespace SpectrumEngine.Emu.Test.Z80;

public class StandardOpTests0x00
{
    /// <summary>
    /// NOP: 0x00
    /// </summary>
    [Fact]
    public void NopWorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x00, // NOP
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0001);
        m.Cpu.Tacts.ShouldBe(4UL);
    }

    /// <summary>
    /// LD BC,NN: 0x01
    /// </summary>
    [Fact]
    public void LD_BC_NN_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x01, 0x26, 0xA9 // LD BC,A926H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "BC");
        m.ShouldKeepMemory();

        regs.BC.ShouldBe((ushort)0xA926);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(10UL);
    }

    /// <summary>
    /// LD (BC),A: 0x02
    /// </summary>
    [Fact]
    public void LD_BCi_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x01, 0x26, 0xA9, // LD BC,A926H
            0x3E, 0x94,       // LD A,94H
            0x02              // LD (BC),A
        });

        // --- Act
        var valueBefore = m.Memory[0xA926];
        m.Run();
        var valueAfter = m.Memory[0xA926];

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "BC, A");
        m.ShouldKeepMemory(except: "A926");

        regs.BC.ShouldBe((ushort)0xA926);
        regs.A.ShouldBe((byte)0x94);
        valueBefore.ShouldBe((byte)0);
        valueAfter.ShouldBe((byte)0x94);
        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(24UL);
    }

    /// <summary>
    /// INC BC: 0x03
    /// </summary>
    [Fact]
    public void INC_BC_WorksAsExpected1()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x01, 0x26, 0xA9, // LD BC,A926H
            0x03              // INC BC
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "BC");
        m.ShouldKeepMemory();

        regs.BC.ShouldBe((ushort)0xA927);
        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(16UL);
    }

    /// <summary>
    /// INC BC: 0x03
    /// </summary>
    [Fact]
    public void INC_BC_WorksAsExpected2()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x01, 0xFF, 0xFF, // LD BC,FFFFH
            0x03              // INC BC
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "BC");
        m.ShouldKeepMemory();

        regs.BC.ShouldBe((ushort)0x0000);
        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(16UL);
    }

    /// <summary>
    /// INC B: 0x04
    /// </summary>
    [Fact]
    public void INC_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x43, // LD B,43H
            0x04        // INC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.B.ShouldBe((byte)0x44);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// INC B: 0x04
    /// </summary>
    [Fact]
    public void INC_B_SetsZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0xFF, // LD B,FFH
            0x04        // INC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsZFlagSet.ShouldBeTrue();

        regs.B.ShouldBe((byte)0x00);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// INC B: 0x04
    /// </summary>
    [Fact]
    public void INC_B_SetsSignAnOverflowFlags()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x7F, // LD B,7FH
            0x04        // INC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.B.ShouldBe((byte)0x80);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// INC B: 0x04
    /// </summary>
    [Fact]
    public void INC_B_SetsHalfCarryFlags()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x2F, // LD B,2FH
            0x04        // INC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeTrue();

        regs.B.ShouldBe((byte)0x30);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// DEC B: 0x05
    /// </summary>
    [Fact]
    public void DEC_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x43, // LD B,43H
            0x05        // DEC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeTrue();

        regs.B.ShouldBe((byte)0x42);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// DEC B: 0x05
    /// </summary>
    [Fact]
    public void DEC_B_SetsZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x01, // LD B,01H
            0x05        // DEC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeTrue();

        regs.IsZFlagSet.ShouldBeTrue();

        regs.B.ShouldBe((byte)0x00);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// DEC B: 0x05
    /// </summary>
    [Fact]
    public void DEC_B_SetsSignAnOverflowFlags()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x80, // LD B,80H
            0x05        // DEC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeTrue();

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.B.ShouldBe((byte)0x7F);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// DEC B: 0x05
    /// </summary>
    [Fact]
    public void DEC_B_SetsHalfCarryFlags()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x06, 0x20, // LD B,20H
            0x05        // DEC B
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeTrue();

        regs.B.ShouldBe((byte)0x1F);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD B,N: 0x06
    /// </summary>
    [Fact]
    public void LD_B_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x06, 0x26 // LD B,26H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "B");
        m.ShouldKeepMemory();

        regs.B.ShouldBe((byte)0x26);
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(7UL);
    }

    /// <summary>
    /// RLCA: 0x07
    /// </summary>
    [Fact]
    public void RLCA_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x71, // LD A,71H
            0x07        // RLCA
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A, F");
        m.ShouldKeepMemory();
        m.ShouldKeepSFlag();
        m.ShouldKeepZFlag();
        m.ShouldKeepPVFlag();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsCFlagSet.ShouldBeFalse();

        regs.A.ShouldBe((byte)0xE2);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// RLCA: 0x07
    /// </summary>
    [Fact]
    public void RLCA_GeneratesCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x80, // LD A,80H
            0x07        // RLCA
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A, F");
        m.ShouldKeepMemory();
        m.ShouldKeepSFlag();
        m.ShouldKeepZFlag();
        m.ShouldKeepPVFlag();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsCFlagSet.ShouldBeTrue();

        regs.A.ShouldBe((byte)0x01);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// EX AF,AF': 0x08
    /// </summary>
    [Fact]
    public void EX_AF_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x34,  // LD A,34H
            0x08,        // EX AF,AF'
            0x3E, 0x56 , // LD A,56H
            0x08         // EX AF,AF'
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "AF, AF'");
        m.ShouldKeepMemory();

        regs.A.ShouldBe((byte)0x34);
        regs.PC.ShouldBe((ushort)0x0006);
        m.Cpu.Tacts.ShouldBe(22UL);
    }

    /// <summary>
    /// ADD HL,BC: 0x09
    /// </summary>
    [Fact]
    public void ADD_HL_BC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x34, 0x12, // LD HL,1234H
            0x01, 0x02, 0x11, // LD BC,1102H
            0x09              // ADD HL,BC
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F, BC, HL");
        m.ShouldKeepMemory();
        m.ShouldKeepSFlag();
        m.ShouldKeepZFlag();
        m.ShouldKeepPVFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();

        regs.HL.ShouldBe((ushort)0x2336);
        regs.PC.ShouldBe((ushort)0x0007);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// ADD HL,BC: 0x09
    /// </summary>
    [Fact]
    public void ADD_HL_BC_GeneratesCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x34, 0xF2, // LD HL,F234H
            0x01, 0x02, 0x11, // LD BC,1102H
            0x09              // ADD HL,BC
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F, BC, HL");
        m.ShouldKeepMemory();
        m.ShouldKeepSFlag();
        m.ShouldKeepZFlag();
        m.ShouldKeepPVFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsCFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();

        regs.HL.ShouldBe((ushort)0x0336);
        regs.PC.ShouldBe((ushort)0x0007);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// ADD HL,BC: 0x09
    /// </summary>
    [Fact]
    public void ADD_HL_BC_GeneratesHalfOverflow()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x21, 0x34, 0x1F, // LD HL,1F34H
            0x01, 0x02, 0x11, // LD BC,1102H
            0x09              // ADD HL,BC
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "F, BC, HL");
        m.ShouldKeepMemory();
        m.ShouldKeepSFlag();
        m.ShouldKeepZFlag();
        m.ShouldKeepPVFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsCFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();

        regs.HL.ShouldBe((ushort)0x3036);
        regs.PC.ShouldBe((ushort)0x0007);
        m.Cpu.Tacts.ShouldBe(31UL);
    }

    /// <summary>
    /// LD A,(BC): 0x0A
    /// </summary>
    [Fact]
    public void LD_A_BCi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x01, 0x03, 0x00, // LD BC,0003H
            0x0A              // LD A,(BC)
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "BC, A");
        m.ShouldKeepMemory();

        regs.A.ShouldBe((byte)0x0A);
        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(17UL);
    }

    /// <summary>
    /// DEC BC: 0x0B
    /// </summary>
    [Fact]
    public void DEC_BC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x01, 0x26, 0xA9, // LD BC,A926H
            0x0B              // DEC BC
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "BC");
        m.ShouldKeepMemory();

        regs.BC.ShouldBe((ushort)0xA925);
        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(16UL);
    }

    /// <summary>
    /// INC C: 0x0C
    /// </summary>
    [Fact]
    public void INC_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x0E, 0x43, // LD C,43H
            0x0C        // INC C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "C, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.C.ShouldBe((byte)0x44);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// DEC C: 0x0D
    /// </summary>
    [Fact]
    public void DEC_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x0E, 0x43, // LD C,43H
            0x0D        // DEC C
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "C, F");
        m.ShouldKeepMemory();
        m.ShouldKeepCFlag();
        regs.IsNFlagSet.ShouldBeTrue();

        regs.C.ShouldBe((byte)0x42);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// LD C,N: 0x0E
    /// </summary>
    [Fact]
    public void LD_C_N_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
            0x0E, 0x26 // LD B,26H
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "C");
        m.ShouldKeepMemory();

        regs.C.ShouldBe((byte)0x26);
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(7UL);
    }

    /// <summary>
    /// RRCA: 0x0F
    /// </summary>
    [Fact]
    public void RRCA_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x70, // LD A,70H
            0x0F        // RRCA
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A, F");
        m.ShouldKeepMemory();
        m.ShouldKeepSFlag();
        m.ShouldKeepZFlag();
        m.ShouldKeepPVFlag();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsCFlagSet.ShouldBeFalse();

        regs.A.ShouldBe((byte)0x38);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }

    /// <summary>
    /// RRCA: 0x0F
    /// </summary>
    [Fact]
    public void RRCA_GeneratesCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilEnd);
        m.InitCode(new byte[]
        {
            0x3E, 0x41, // LD A,01H
            0x0F        // RRCA
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;

        m.ShouldKeepRegisters(except: "A, F");
        m.ShouldKeepMemory();
        m.ShouldKeepSFlag();
        m.ShouldKeepZFlag();
        m.ShouldKeepPVFlag();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();

        regs.IsCFlagSet.ShouldBeTrue();

        regs.A.ShouldBe((byte)0xA0);
        regs.PC.ShouldBe((ushort)0x0003);
        m.Cpu.Tacts.ShouldBe(11UL);
    }
}

