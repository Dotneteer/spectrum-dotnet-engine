﻿using static SpectrumEngine.Emu.Z80Cpu;

namespace SpectrumEngine.Emu.Test.Z80;

public class ExtendedOpTests
{
    /// <summary>
    /// IN B,(C): 0xED 0x40
    /// </summary>
    [Fact]
    public void IN_B_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x40 // IN B,(C)
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert

        regs.B.ShouldBe((byte)0xD5);
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, B");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// OUT (C),B: 0xED 0x41
    /// </summary>
    [Fact]
    public void OUT_C_B_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x41        // OUT (C),B
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0x12);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// SBC HL,BC: 0xED 0x42
    /// </summary>
    [Fact]
    public void SBC_HL_BC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x42        // SBC HL,BC
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x3456;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2222);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// SBC HL,BC: 0xED 0x42
    /// </summary>
    [Fact]
    public void SBC_HL_BC_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x42        // SBC HL,BC
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1234;
        regs.BC = 0x3456;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0xDDDE);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// SBC HL,BC: 0xED 0x42
    /// </summary>
    [Fact]
    public void SBC_HL_BC_SetsZeroFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x42        // SBC HL,BC
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1234;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x0000);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// SBC HL,BC: 0xED 0x42
    /// </summary>
    [Fact]
    public void SBC_HL_BC_UsesCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x42        // SBC HL,BC
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x3456;
        regs.BC = 0x1234;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2221);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// LD (NN),BC: 0xED 0x43
    /// </summary>
    [Fact]
    public void LD_NNi_BC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x43, 0x00, 0x10 // LD (1000H),BC
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[0x1000].ShouldBe((byte)0x34);
        m.Memory[0x1001].ShouldBe((byte)0x12);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory(except: "1000-1001");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// NEG: 0xED 0x44
    /// </summary>
    [Fact]
    public void NEG_WorksAsExpected1()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x44 // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x03;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xFD);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// NEG: 0xED 0x44
    /// </summary>
    [Fact]
    public void NEG_SetsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x44 // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x00;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0x00);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// NEG: 0xED 0x44
    /// </summary>
    [Fact]
    public void NEG_SetsPFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x44 // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x80;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0x80);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// NEG: 0xED 0x44
    /// </summary>
    [Fact]
    public void NEG_ResetsHFlag()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x44 // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0xD0;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0x30);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RETN: 0xED 0x45
    /// </summary>
    [Fact]
    public void RETN_WorksWithDI1()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x45        // RETN
        });
        m.Cpu.Iff1 = false;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// RETN: 0xED 0x45
    /// </summary>
    [Fact]
    public void RETN_WorksWithEI()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x45        // RETN
        });
        m.Cpu.Iff1 = true;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// IM 0: 0xED 0x46
    /// </summary>
    [Fact]
    public void IM_0_WorksAsExpected1()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x46 // IM 0
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.InterruptMode.ShouldBe((byte)0);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// LD I,A: 0xED 0x47
    /// </summary>
    [Fact]
    public void LD_I_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x47 // LD I,A
        });
        var regs = m.Cpu.Regs;
        regs.A = 0xD5;

        // --- Act
        m.Run();

        // --- Assert
        regs.I.ShouldBe((byte)0xD5);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// IN C,(C): 0xED 0x48
    /// </summary>
    [Fact]
    public void IN_C_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x48 // IN C,(C)
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert

        regs.C.ShouldBe((byte)0xD5);
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, C");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// OUT (C),C: 0xED 0x49
    /// </summary>
    [Fact]
    public void OUT_C_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x49        // OUT (C),C
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0x34);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// ADC HL,BC: 0xED 0x4A
    /// </summary>
    [Fact]
    public void ADC_HL_BC_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x4A // ADC HL,BC
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x1111;
        regs.BC = 0x1234;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2346);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// ADC HL,BC: 0xED 0x4A
    /// </summary>
    [Fact]
    public void ADC_HL_BC_SetsCarry()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x4A // ADC HL,BC
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x1111;
        regs.BC = 0xF234;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x0346);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// ADC HL,BC: 0xED 0x4A
    /// </summary>
    [Fact]
    public void ADC_HL_BC_SetsSign()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x4A // ADC HL,BC
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x1111;
        regs.BC = 0x7234;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x8346);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// ADC HL,BC: 0xED 0x4A
    /// </summary>
    [Fact]
    public void ADC_HL_BC_SetsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x4A // ADC HL,BC
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x0001;
        regs.BC = 0xFFFE;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x0000);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// LD BC,(NN): 0xED 0x4B
    /// </summary>
    [Fact]
    public void LD_BC_NNi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x4B, 0x00, 0x10 // LD BC,(1000H)
        });
        var regs = m.Cpu.Regs;
        m.Memory[0x1000] = 0x34;
        m.Memory[0x1001] = 0x12;

        // --- Act
        m.Run();

        // --- Assert
        regs.BC.ShouldBe((ushort)0x1234);

        m.ShouldKeepRegisters(except: "BC");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// NEG: 0xED 0x4C
    /// </summary>
    [Fact]
    public void NEG_WorksAsExpected2()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x4C // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x03;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xFD);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RETI: 0xED 0x4D
    /// </summary>
    [Fact]
    public void RETI_WorksWithAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x4D        // RETI
        });
        m.Cpu.Iff1 = false;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// IM 0: 0xED 0x4E
    /// </summary>
    [Fact]
    public void IM_0_WorksAsExpected2()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x4E // IM 0
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.InterruptMode.ShouldBe((byte)0);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// LD R,A: 0xED 0x4F
    /// </summary>
    [Fact]
    public void LD_R_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x4F // LD R,A
        });
        var regs = m.Cpu.Regs;
        regs.A = 0xD5;

        // --- Act
        m.Run();

        // --- Assert
        regs.R.ShouldBe((byte)0xD5);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// IN D,(C): 0xED 0x50
    /// </summary>
    [Fact]
    public void IN_D_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x50 // IN D,(C)
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert

        regs.D.ShouldBe((byte)0xD5);
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, D");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// OUT (C),D: 0xED 0x51
    /// </summary>
    [Fact]
    public void OUT_C_D_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x51        // OUT (C),D
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;
        regs.DE = 0xBA98;

        // --- Act
        m.Run();

        // --- Assert
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xBA);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// SBC HL,DE: 0xED 0x52
    /// </summary>
    [Fact]
    public void SBC_HL_DE_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x52        // SBC HL,DE
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x3456;
        regs.DE = 0x1234;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2221);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// LD (NN),BC: 0xED 0x53
    /// </summary>
    [Fact]
    public void LD_NNi_DE_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x53, 0x00, 0x10 // LD (1000H),DE
        });
        var regs = m.Cpu.Regs;
        regs.DE = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[0x1000].ShouldBe((byte)0x34);
        m.Memory[0x1001].ShouldBe((byte)0x12);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory(except: "1000-1001");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// NEG: 0xED 0x54
    /// </summary>
    [Fact]
    public void NEG_WorksAsExpected3()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x54 // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x03;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xFD);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RETN: 0xED 0x55
    /// </summary>
    [Fact]
    public void RETN_WorksWithDI2()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x55        // RETN
        });
        m.Cpu.Iff1 = false;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// IM 1: 0xED 0x56
    /// </summary>
    [Fact]
    public void IM_1_WorksAsExpected1()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x56 // IM 1
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.InterruptMode.ShouldBe((byte)1);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.I = 0xD5;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0xD5);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_ResetsHAndN()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.I = 0xD5;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0xD5);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_SetsSWhenINegative()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.I = 0xD5;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0xD5);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsSFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_ResetsSWhenINonNegative()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.I = 0x25;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsSFlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_SetsZWhenIIsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.I = 0x00;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x00);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsZFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_ResetsZWhenIIsNotZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.I = 0x25;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsZFlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_ResetsPVWhenIff2IsReset()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Iff2 = false;
        regs.I = 0x25;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsPFlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_SetsPVWhenIff2IsSet()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Iff2 = true;
        regs.I = 0x25;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsPFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_KeepsCWhenSet()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.I = 0x25;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsCFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_KeepsCWhenReset()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.I = 0x25;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsCFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_I_KeepsF3AndF5()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x57 // LD A,I
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.I = 0x09;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x09);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsR3FlagSet.ShouldBeTrue();
        regs.IsR5FlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// IN E,(C): 0xED 0x58
    /// </summary>
    [Fact]
    public void IN_E_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x58 // IN E,(C)
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert

        regs.E.ShouldBe((byte)0xD5);
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, E");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// OUT (C),E: 0xED 0x59
    /// </summary>
    [Fact]
    public void OUT_C_E_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x59        // OUT (C),E
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;
        regs.DE = 0xBA98;

        // --- Act
        m.Run();

        // --- Assert
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0x98);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// ADC HL,DE: 0xED 0x5A
    /// </summary>
    [Fact]
    public void ADC_HL_DE_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5A // ADC HL,DE
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x1111;
        regs.DE = 0x1234;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2346);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// LD DE,(NN): 0xED 0x5B
    /// </summary>
    [Fact]
    public void LD_DE_NNi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5B, 0x00, 0x10 // LD DE,(1000H)
        });
        var regs = m.Cpu.Regs;
        m.Memory[0x1000] = 0x34;
        m.Memory[0x1001] = 0x12;

        // --- Act
        m.Run();

        // --- Assert
        regs.DE.ShouldBe((ushort)0x1234);

        m.ShouldKeepRegisters(except: "DE");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// NEG: 0xED 0x5C
    /// </summary>
    [Fact]
    public void NEG_WorksAsExpected4()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5C // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x03;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xFD);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RETN: 0xED 0x5D
    /// </summary>
    [Fact]
    public void RETN_WorksWithDI3()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x5D        // RETN
        });
        m.Cpu.Iff1 = false;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// IM 2: 0xED 0x5E
    /// </summary>
    [Fact]
    public void IM_2_WorksAsExpected1()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5E // IM 2
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.InterruptMode.ShouldBe((byte)2);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// LD A,R: 0xED 0x5F
    /// </summary>
    [Fact]
    public void LD_A_R_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.R = 0xD5;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus instead of 0xD5 it gets 0xD7
        regs.A.ShouldBe((byte)0xD7);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_ResetsHAndN()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.R = 0xD3;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0xD5);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_SetsSWhenINegative()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.R = 0xD3;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0xD5);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsSFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_ResetsSWhenINonNegative()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.R = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsSFlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_SetsZWhenIIsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.R = 0x7E;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0x00);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsZFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_ResetsZWhenIIsNotZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.R = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsZFlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_ResetsPVWhenIff2IsReset()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Iff2 = false;
        regs.R = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsPFlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_SetsPVWhenIff2IsSet()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        m.Cpu.Iff2 = true;
        regs.R = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsPFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_KeepsCWhenSet()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.R = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsCFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_KeepsCWhenReset()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.R = 0x23;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0x25);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsCFlagSet.ShouldBeTrue();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// LD A,I: 0xED 0x57
    /// </summary>
    [Fact]
    public void LD_A_R_KeepsF3AndF5()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x5F // LD A,R
        });
        var regs = m.Cpu.Regs;
        regs.F |= FlagsSetMask.C;
        regs.R = 0x07;

        // --- Act
        m.Run();

        // --- Assert
        // --- R is incremented at both fetch cycle,
        // --- thus we test for the initial value + 2
        regs.A.ShouldBe((byte)0x09);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.IsR3FlagSet.ShouldBeTrue();
        regs.IsR5FlagSet.ShouldBeFalse();
        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(9UL);
    }

    /// <summary>
    /// IN H,(C): 0xED 0x60
    /// </summary>
    [Fact]
    public void IN_H_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x60 // IN H,(C)
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert

        regs.H.ShouldBe((byte)0xD5);
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, H");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// OUT (C),H: 0xED 0x61
    /// </summary>
    [Fact]
    public void OUT_C_H_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x61        // OUT (C),H
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;
        regs.HL = 0xBA98;

        // --- Act
        m.Run();

        // --- Assert
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xBA);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// SBC HL,DE: 0xED 0x62
    /// </summary>
    [Fact]
    public void SBC_HL_HL_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x62        // SBC HL,HL
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x3456;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0xFFFF);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// LD (NN),BC: 0xED 0x63
    /// </summary>
    [Fact]
    public void LD_NNi_HL_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x63, 0x00, 0x10 // LD (1000H),HL
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[0x1000].ShouldBe((byte)0x34);
        m.Memory[0x1001].ShouldBe((byte)0x12);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory(except: "1000-1001");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// NEG: 0xED 0x64
    /// </summary>
    [Fact]
    public void NEG_WorksAsExpected5()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x64 // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x03;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xFD);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RETN: 0xED 0x65
    /// </summary>
    [Fact]
    public void RETN_WorksWithDI4()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x65        // RETN
        });
        m.Cpu.Iff1 = false;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// IM 0: 0xED 0x66
    /// </summary>
    [Fact]
    public void IM_0_WorksAsExpected3()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x66 // IM 0
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.InterruptMode.ShouldBe((byte)0);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RRD: 0xED 0x67
    /// </summary>
    [Fact]
    public void RRD_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x67 // RRD
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[0x1000] = 0x56;
        regs.A = 0x34;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x36);
        m.Memory[0x1000].ShouldBe((byte)0x45);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RRD: 0xED 0x67
    /// </summary>
    [Fact]
    public void RRD_SetsSign()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x67 // RRD
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[0x1000] = 0x56;
        regs.A = 0xA4;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0xA6);
        m.Memory[0x1000].ShouldBe((byte)0x45);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RRD: 0xED 0x67
    /// </summary>
    [Fact]
    public void RRD_SetsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x67 // RRD
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[0x1000] = 0x50;
        regs.A = 0x04;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x00);
        m.Memory[0x1000].ShouldBe((byte)0x45);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RRD: 0xED 0x67
    /// </summary>
    [Fact]
    public void RRD_ResetsParity()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x67 // RRD
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[0x1000] = 0x50;
        regs.A = 0x14;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x10);
        m.Memory[0x1000].ShouldBe((byte)0x45);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// IN L,(C): 0xED 0x68
    /// </summary>
    [Fact]
    public void IN_L_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x68 // IN L,(C)
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert

        regs.L.ShouldBe((byte)0xD5);
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, L");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// OUT (C),L: 0xED 0x69
    /// </summary>
    [Fact]
    public void OUT_C_L_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x69        // OUT (C),L
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;
        regs.HL = 0xBA98;

        // --- Act
        m.Run();

        // --- Assert
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0x98);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// ADC HL,HL: 0xED 0x6A
    /// </summary>
    [Fact]
    public void ADC_HL_HL_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x6A // ADC HL,HL
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x1111;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2223);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// LD HL,(NN): 0xED 0x6B
    /// </summary>
    [Fact]
    public void LD_HL_NNi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x6B, 0x00, 0x10 // LD HL,(1000H)
        });
        var regs = m.Cpu.Regs;
        m.Memory[0x1000] = 0x34;
        m.Memory[0x1001] = 0x12;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x1234);

        m.ShouldKeepRegisters(except: "HL");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// NEG: 0xED 0x6C
    /// </summary>
    [Fact]
    public void NEG_WorksAsExpected6()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x6C // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x03;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xFD);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RETN: 0xED 0x6D
    /// </summary>
    [Fact]
    public void RETN_WorksWithDI5()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x6D        // RETN
        });
        m.Cpu.Iff1 = false;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// IM 0: 0xED 0x6E
    /// </summary>
    [Fact]
    public void IM_0_WorksAsExpected4()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x6E // IM 0
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.InterruptMode.ShouldBe((byte)0);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RLD: 0xED 0x6F
    /// </summary>
    [Fact]
    public void RLD_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x6F // RLD
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[0x1000] = 0x56;
        regs.A = 0x34;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x35);
        m.Memory[0x1000].ShouldBe((byte)0x64);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RLD: 0xED 0x6F
    /// </summary>
    [Fact]
    public void RLD_SetsSign()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x6F // RLD
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[0x1000] = 0x56;
        regs.A = 0xA4;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0xA5);
        m.Memory[0x1000].ShouldBe((byte)0x64);
        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RLD: 0xED 0x68
    /// </summary>
    [Fact]
    public void RLD_SetsZero()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x6F // RLD
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[0x1000] = 0x06;
        regs.A = 0x04;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x00);
        m.Memory[0x1000].ShouldBe((byte)0x64);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeTrue();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// RLD: 0xED 0x68
    /// </summary>
    [Fact]
    public void RLD_ResetsParity()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x6F // RLD
        });
        var regs = m.Cpu.Regs;
        regs.F &= 0xfe;
        regs.HL = 0x1000;
        m.Memory[0x1000] = 0x06;
        regs.A = 0x14;

        // --- Act
        m.Run();

        // --- Assert
        regs.A.ShouldBe((byte)0x10);
        m.Memory[0x1000].ShouldBe((byte)0x64);
        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();

        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsNFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "1000");

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(18UL);
    }

    /// <summary>
    /// IN (C): 0xED 0x70
    /// </summary>
    [Fact]
    public void IN_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x70 // IN (C)
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert

        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// OUT (C),0: 0xED 0x71
    /// </summary>
    [Fact]
    public void OUT_C_0_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x71        // OUT (C),0
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0x00);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// SBC HL,SP: 0xED 0x72
    /// </summary>
    [Fact]
    public void SBC_HL_SP_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x72        // SBC HL,SP
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x3456;
        regs.SP = 0x1234;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2221);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        regs.IsNFlagSet.ShouldBeTrue();
        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// LD (NN),BC: 0xED 0x73
    /// </summary>
    [Fact]
    public void LD_NNi_SP_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x73, 0x00, 0x10 // LD (1000H),SP
        });
        var regs = m.Cpu.Regs;
        regs.SP = 0x1234;

        // --- Act
        m.Run();

        // --- Assert
        m.Memory[0x1000].ShouldBe((byte)0x34);
        m.Memory[0x1001].ShouldBe((byte)0x12);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory(except: "1000-1001");

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// NEG: 0xED 0x74
    /// </summary>
    [Fact]
    public void NEG_WorksAsExpected7()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x74 // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x03;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xFD);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RETN: 0xED 0x75
    /// </summary>
    [Fact]
    public void RETN_WorksWithDI6()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x75        // RETN
        });
        m.Cpu.Iff1 = false;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// IM 1: 0xED 0x76
    /// </summary>
    [Fact]
    public void IM_1_WorksAsExpected2()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x76 // IM 1
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.InterruptMode.ShouldBe((byte)1);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// IN A,(C): 0xED 0x78
    /// </summary>
    [Fact]
    public void IN_A_C_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x78 // IN A,(C)
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xD5);
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0xD5);
        m.IoAccessLog[0].IsOutput.ShouldBeFalse();
        m.ShouldKeepRegisters(except: "F, A");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// OUT (C),A: 0xED 0x79
    /// </summary>
    [Fact]
    public void OUT_C_A_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x79        // OUT (C),A
        });
        var regs = m.Cpu.Regs;
        regs.BC = 0x1234;
        regs.A = 0x98;

        // --- Act
        m.Run();

        // --- Assert
        m.IoAccessLog.Count.ShouldBe(1);
        m.IoAccessLog[0].Address.ShouldBe((ushort)0x1234);
        m.IoAccessLog[0].Value.ShouldBe((byte)0x98);
        m.IoAccessLog[0].IsOutput.ShouldBeTrue();

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(12UL);
    }

    /// <summary>
    /// ADC HL,SP: 0xED 0x7A
    /// </summary>
    [Fact]
    public void ADC_HL_SP_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x7A // ADC HL,SP
        });
        var regs = m.Cpu.Regs;
        regs.HL = 0x1111;
        regs.SP = 0x1234;
        regs.F |= FlagsSetMask.C;

        // --- Act
        m.Run();

        // --- Assert
        regs.HL.ShouldBe((ushort)0x2346);

        regs.IsSFlagSet.ShouldBeFalse();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeFalse();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeFalse();

        m.ShouldKeepRegisters(except: "HL, F");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(15UL);
    }

    /// <summary>
    /// LD SP,(NN): 0xED 0x7B
    /// </summary>
    [Fact]
    public void LD_SP_NNi_WorksAsExpected()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x7B, 0x00, 0x10 // LD SP,(1000H)
        });
        var regs = m.Cpu.Regs;
        m.Memory[0x1000] = 0x34;
        m.Memory[0x1001] = 0x12;

        // --- Act
        m.Run();

        // --- Assert
        regs.SP.ShouldBe((ushort)0x1234);

        m.ShouldKeepRegisters(except: "SP");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0004);
        m.Cpu.Tacts.ShouldBe(20UL);
    }

    /// <summary>
    /// NEG: 0xED 0x7C
    /// </summary>
    [Fact]
    public void NEG_WorksAsExpected8()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x7C // NEG
        });
        m.IoInputSequence.Add(0xD5);
        var regs = m.Cpu.Regs;
        regs.A = 0x03;

        // --- Act
        m.Run();

        // --- Assert

        regs.A.ShouldBe((byte)0xFD);

        regs.IsSFlagSet.ShouldBeTrue();
        regs.IsZFlagSet.ShouldBeFalse();
        regs.IsHFlagSet.ShouldBeTrue();
        regs.IsPFlagSet.ShouldBeFalse();
        regs.IsCFlagSet.ShouldBeTrue();

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }

    /// <summary>
    /// RETN: 0xED 0x7D
    /// </summary>
    [Fact]
    public void RETN_WorksWithDI7()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.UntilHalt);
        m.InitCode(new byte[]
        {
                0x3E, 0x16,       // LD A,16H
                0xCD, 0x06, 0x00, // CALL 0006H
                0x76,             // HALT
                0xED, 0x7D        // RETN
        });
        m.Cpu.Iff1 = false;
        var regs = m.Cpu.Regs;
        regs.SP = 0x0000;

        // --- Act
        m.Run();

        // --- Assert
        m.Cpu.Iff1.ShouldBe(m.Cpu.Iff2);

        m.ShouldKeepRegisters(except: "AF");
        m.ShouldKeepMemory(except: "FFFE-FFFF");

        regs.PC.ShouldBe((ushort)0x0005);
        m.Cpu.Tacts.ShouldBe(42UL);
    }

    /// <summary>
    /// IM 2: 0xED 0x7E
    /// </summary>
    [Fact]
    public void IM_2_WorksAsExpected2()
    {
        // --- Arrange
        var m = new Z80TestMachine(RunMode.OneInstruction);
        m.InitCode(new byte[]
        {
                0xED, 0x7E // IM 2
        });

        // --- Act
        m.Run();

        // --- Assert
        var regs = m.Cpu.Regs;
        m.Cpu.InterruptMode.ShouldBe((byte)2);

        m.ShouldKeepRegisters();
        m.ShouldKeepMemory();

        regs.PC.ShouldBe((ushort)0x0002);
        m.Cpu.Tacts.ShouldBe(8UL);
    }
}