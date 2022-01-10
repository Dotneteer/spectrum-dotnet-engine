namespace SpectrumEngine.Emu.Test;

public class ResetTests
{
    [Fact]
    public void HardReset()
    {
        // --- Arrange
        var cpu = new Z80Cpu();
        var regs = cpu.Regs;

        // --- Act
        cpu.HardReset();

        // --- Assert
        regs.AF.ShouldBe((ushort)0xffff);
        regs.A.ShouldBe((byte)0xff);
        regs.F.ShouldBe((byte)0xff);
        regs._AF_.ShouldBe((ushort)0xffff);

        regs.BC.ShouldBe((ushort)0x0000);
        regs.B.ShouldBe((byte)0x00);
        regs.C.ShouldBe((byte)0x00);
        regs._BC_.ShouldBe((ushort)0x0000);

        regs.DE.ShouldBe((ushort)0x0000);
        regs.D.ShouldBe((byte)0x00);
        regs.E.ShouldBe((byte)0x00);
        regs._DE_.ShouldBe((ushort)0x0000);

        regs.HL.ShouldBe((ushort)0x0000);
        regs.H.ShouldBe((byte)0x00);
        regs.L.ShouldBe((byte)0x00);
        regs._HL_.ShouldBe((ushort)0x0000);

        regs.IX.ShouldBe((ushort)0x0000);
        regs.XH.ShouldBe((byte)0x00);
        regs.XL.ShouldBe((byte)0x00);

        regs.IY.ShouldBe((ushort)0x0000);
        regs.YH.ShouldBe((byte)0x00);
        regs.YL.ShouldBe((byte)0x00);

        regs.IR.ShouldBe((ushort)0x0000);
        regs.I.ShouldBe((byte)0x00);
        regs.R.ShouldBe((byte)0x00);

        regs.PC.ShouldBe((ushort)0x0000);
        regs.SP.ShouldBe((ushort)0xffff);

        regs.WZ.ShouldBe((ushort)0x0000);
        regs.WH.ShouldBe((byte)0x00);
        regs.WL.ShouldBe((byte)0x00);

        cpu.SignalFlags.ShouldBe(Z80Cpu.Z80Signals.None);
        cpu.InterruptMode.ShouldBe(0);
        cpu.Iff1.ShouldBeFalse();
        cpu.Iff2.ShouldBeFalse();
        cpu.Tacts.ShouldBe(0ul);
        cpu.F53Updated.ShouldBeFalse();
        cpu.PrevF53Updated.ShouldBeFalse();

        cpu.OpCode.ShouldBe((byte)0);
        cpu.Prefix.ShouldBe(Z80Cpu.OpCodePrefix.None);
        cpu.EiBacklog.ShouldBe(0);
        cpu.RetExecuted.ShouldBeFalse();
    }

    [Fact]
    public void ResetHandlesMainRegs()
    {
        // --- Arrange
        var cpu = new Z80Cpu();
        var regs = cpu.Regs;

        // --- Act
        cpu.Reset();

        // --- Assert
        regs.AF.ShouldBe((ushort)0xffff);
        regs.A.ShouldBe((byte)0xff);
        regs.F.ShouldBe((byte)0xff);
        regs._AF_.ShouldBe((ushort)0xffff);

        regs.IR.ShouldBe((ushort)0x0000);
        regs.I.ShouldBe((byte)0x00);
        regs.R.ShouldBe((byte)0x00);

        regs.PC.ShouldBe((ushort)0x0000);
        regs.SP.ShouldBe((ushort)0xffff);

        regs.WZ.ShouldBe((ushort)0x0000);
        regs.WH.ShouldBe((byte)0x00);
        regs.WL.ShouldBe((byte)0x00);

        cpu.SignalFlags.ShouldBe(Z80Cpu.Z80Signals.None);
        cpu.InterruptMode.ShouldBe(0);
        cpu.Iff1.ShouldBeFalse();
        cpu.Iff2.ShouldBeFalse();
        cpu.Tacts.ShouldBe(0ul);
        cpu.F53Updated.ShouldBeFalse();
        cpu.PrevF53Updated.ShouldBeFalse();

        cpu.OpCode.ShouldBe((byte)0);
        cpu.Prefix.ShouldBe(Z80Cpu.OpCodePrefix.None);
        cpu.EiBacklog.ShouldBe(0);
        cpu.RetExecuted.ShouldBeFalse();
    }

    [Fact]
    public void ResetKeepsRegValues()
    {
        // --- Arrange
        var cpu = new Z80Cpu();
        var regs = cpu.Regs;
        regs.AF = 0x34ac;
        regs._AF_ = 0x34ac;
        regs.BC = 0x34ac;
        regs._BC_ = 0x34ac;
        regs.DE = 0x34ac;
        regs._DE_ = 0x34ac;
        regs.HL = 0x34ac;
        regs._HL_ = 0x34ac;
        regs.IX = 0x34ac;
        regs.IY = 0x34ac;
        regs.IR = 0x34ac;
        regs.PC = 0x34ac;
        regs.SP = 0x34ac;
        regs.WZ = 0x34ac;
        
        // --- Act
        cpu.Reset();

        // --- Assert
        regs.AF.ShouldBe((ushort)0xffff);
        regs.A.ShouldBe((byte)0xff);
        regs.F.ShouldBe((byte)0xff);
        regs._AF_.ShouldBe((ushort)0xffff);

        regs.BC.ShouldBe((ushort)0x34ac);
        regs.B.ShouldBe((byte)0x34);
        regs.C.ShouldBe((byte)0xac);
        regs._BC_.ShouldBe((ushort)0x34ac);

        regs.DE.ShouldBe((ushort)0x34ac);
        regs.D.ShouldBe((byte)0x34);
        regs.E.ShouldBe((byte)0xac);
        regs._DE_.ShouldBe((ushort)0x34ac);

        regs.HL.ShouldBe((ushort)0x34ac);
        regs.H.ShouldBe((byte)0x34);
        regs.L.ShouldBe((byte)0xac);
        regs._HL_.ShouldBe((ushort)0x34ac);

        regs.IX.ShouldBe((ushort)0x34ac);
        regs.XH.ShouldBe((byte)0x34);
        regs.XL.ShouldBe((byte)0xac);

        regs.IY.ShouldBe((ushort)0x34ac);
        regs.YH.ShouldBe((byte)0x34);
        regs.YL.ShouldBe((byte)0xac);

        regs.IR.ShouldBe((ushort)0x0000);
        regs.I.ShouldBe((byte)0x00);
        regs.R.ShouldBe((byte)0x00);

        regs.PC.ShouldBe((ushort)0x0000);
        regs.SP.ShouldBe((ushort)0xffff);

        regs.WZ.ShouldBe((ushort)0x0000);
        regs.WH.ShouldBe((byte)0x00);
        regs.WL.ShouldBe((byte)0x00);

        cpu.SignalFlags.ShouldBe(Z80Cpu.Z80Signals.None);
        cpu.InterruptMode.ShouldBe(0);
        cpu.Iff1.ShouldBeFalse();
        cpu.Iff2.ShouldBeFalse();
        cpu.Tacts.ShouldBe(0ul);
        cpu.F53Updated.ShouldBeFalse();
        cpu.PrevF53Updated.ShouldBeFalse();

        cpu.OpCode.ShouldBe((byte)0);
        cpu.Prefix.ShouldBe(Z80Cpu.OpCodePrefix.None);
        cpu.EiBacklog.ShouldBe(0);
        cpu.RetExecuted.ShouldBeFalse();
    }
}
