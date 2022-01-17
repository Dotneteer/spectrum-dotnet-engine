namespace SpectrumEngine.Emu.Test;

public class RegisterAccessTests
{
    [Fact]
    public void WriteA()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.AF = 0;

        // --- Act
        regs.A = 0x4a;

        // --- Assert
        regs.A.ShouldBe((byte)0x4a);
        regs.AF.ShouldBe((ushort)0x4a00);
    }

    [Fact]
    public void WriteF()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.AF = 0;

        // --- Act
        regs.F = 0x4a;

        // --- Assert
        regs.F.ShouldBe((byte)0x4a);
        regs.AF.ShouldBe((ushort)0x004a);
    }

    [Fact]
    public void WriteAF()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.AF = 0x4a28;

        // --- Assert
        regs.AF.ShouldBe((ushort)0x4a28);
        regs.A.ShouldBe((byte)0x4a);
        regs.F.ShouldBe((byte)0x28);
    }

    [Fact]
    public void WriteAFSec()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs._AF_ = 0x4a28;

        // --- Assert
        regs._AF_.ShouldBe((ushort)0x4a28);
    }

    [Fact]
    public void WriteB()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.BC = 0;

        // --- Act
        regs.B = 0x4a;

        // --- Assert
        regs.B.ShouldBe((byte)0x4a);
        regs.BC.ShouldBe((ushort)0x4a00);
    }

    [Fact]
    public void WriteC()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.BC = 0;

        // --- Act
        regs.C = 0x4a;

        // --- Assert
        regs.C.ShouldBe((byte)0x4a);
        regs.BC.ShouldBe((ushort)0x004a);
    }

    [Fact]
    public void WriteBC()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.BC = 0x4a28;

        // --- Assert
        regs.BC.ShouldBe((ushort)0x4a28);
        regs.B.ShouldBe((byte)0x4a);
        regs.C.ShouldBe((byte)0x28);
    }

    [Fact]
    public void WriteBCSec()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs._BC_ = 0x4a28;

        // --- Assert
        regs._BC_.ShouldBe((ushort)0x4a28);
    }

    [Fact]
    public void WriteD()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.DE = 0;

        // --- Act
        regs.D = 0x4a;

        // --- Assert
        regs.D.ShouldBe((byte)0x4a);
        regs.DE.ShouldBe((ushort)0x4a00);
    }

    [Fact]
    public void WriteE()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.DE = 0;

        // --- Act
        regs.E = 0x4a;

        // --- Assert
        regs.E.ShouldBe((byte)0x4a);
        regs.DE.ShouldBe((ushort)0x004a);
    }

    [Fact]
    public void WriteDE()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.DE = 0x4a28;

        // --- Assert
        regs.DE.ShouldBe((ushort)0x4a28);
        regs.D.ShouldBe((byte)0x4a);
        regs.E.ShouldBe((byte)0x28);
    }

    [Fact]
    public void WriteDESec()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs._DE_ = 0x4a28;

        // --- Assert
        regs._DE_.ShouldBe((ushort)0x4a28);
    }

    [Fact]
    public void WriteH()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.HL = 0;

        // --- Act
        regs.H = 0x4a;

        // --- Assert
        regs.H.ShouldBe((byte)0x4a);
        regs.HL.ShouldBe((ushort)0x4a00);
    }

    [Fact]
    public void WriteL()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.HL = 0;

        // --- Act
        regs.L = 0x4a;

        // --- Assert
        regs.L.ShouldBe((byte)0x4a);
        regs.HL.ShouldBe((ushort)0x004a);
    }

    [Fact]
    public void WriteHL()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.HL = 0x4a28;

        // --- Assert
        regs.HL.ShouldBe((ushort)0x4a28);
        regs.H.ShouldBe((byte)0x4a);
        regs.L.ShouldBe((byte)0x28);
    }

    [Fact]
    public void WriteHLSec()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs._HL_ = 0x4a28;

        // --- Assert
        regs._HL_.ShouldBe((ushort)0x4a28);
    }

    [Fact]
    public void WriteXH()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.IX = 0;

        // --- Act
        regs.XH = 0x4a;

        // --- Assert
        regs.XH.ShouldBe((byte)0x4a);
        regs.IX.ShouldBe((ushort)0x4a00);
    }

    [Fact]
    public void WriteXL()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.IX = 0;

        // --- Act
        regs.XL = 0x4a;

        // --- Assert
        regs.XL.ShouldBe((byte)0x4a);
        regs.IX.ShouldBe((ushort)0x004a);
    }

    [Fact]
    public void WriteIX()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.IX = 0x4a28;

        // --- Assert
        regs.IX.ShouldBe((ushort)0x4a28);
        regs.XH.ShouldBe((byte)0x4a);
        regs.XL.ShouldBe((byte)0x28);
    }

    [Fact]
    public void WriteYH()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.IY = 0;

        // --- Act
        regs.YH = 0x4a;

        // --- Assert
        regs.YH.ShouldBe((byte)0x4a);
        regs.IY.ShouldBe((ushort)0x4a00);
    }

    [Fact]
    public void WriteYL()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.IY = 0;

        // --- Act
        regs.YL = 0x4a;

        // --- Assert
        regs.YL.ShouldBe((byte)0x4a);
        regs.IY.ShouldBe((ushort)0x004a);
    }

    [Fact]
    public void WriteIY()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.IY = 0x4a28;

        // --- Assert
        regs.IY.ShouldBe((ushort)0x4a28);
        regs.YH.ShouldBe((byte)0x4a);
        regs.YL.ShouldBe((byte)0x28);
    }

    [Fact]
    public void WriteWH()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.WZ = 0;

        // --- Act
        regs.WH = 0x4a;

        // --- Assert
        regs.WH.ShouldBe((byte)0x4a);
        regs.WZ.ShouldBe((ushort)0x4a00);
    }

    [Fact]
    public void WriteWL()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.WZ = 0;

        // --- Act
        regs.WL = 0x4a;

        // --- Assert
        regs.WL.ShouldBe((byte)0x4a);
        regs.WZ.ShouldBe((ushort)0x004a);
    }

    [Fact]
    public void WriteWZ()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.WZ = 0x4a28;

        // --- Assert
        regs.WZ.ShouldBe((ushort)0x4a28);
        regs.WH.ShouldBe((byte)0x4a);
        regs.WL.ShouldBe((byte)0x28);
    }

    [Fact]
    public void WriteI()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.IR = 0;

        // --- Act
        regs.I = 0x4a;

        // --- Assert
        regs.I.ShouldBe((byte)0x4a);
        regs.IR.ShouldBe((ushort)0x4a00);
    }

    [Fact]
    public void WriteR()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;
        regs.IR = 0;

        // --- Act
        regs.R = 0x4a;

        // --- Assert
        regs.R.ShouldBe((byte)0x4a);
        regs.IR.ShouldBe((ushort)0x004a);
    }

    [Fact]
    public void WriteIR()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.IR = 0x4a28;

        // --- Assert
        regs.IR.ShouldBe((ushort)0x4a28);
        regs.I.ShouldBe((byte)0x4a);
        regs.R.ShouldBe((byte)0x28);
    }

    [Fact]
    public void WritePC()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.PC = 0x4a28;

        // --- Assert
        regs.PC.ShouldBe((ushort)0x4a28);
    }

    [Fact]
    public void WriteSP()
    {
        // --- Arrange
        var regs = new Z80Cpu().Regs;

        // --- Act
        regs.SP = 0x4a28;

        // --- Assert
        regs.SP.ShouldBe((ushort)0x4a28);
    }
}

