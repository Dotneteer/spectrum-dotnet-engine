namespace SpectrumEngine.Emu.Test.Machines;

public class KeyboardDeviceTests
{
    [Fact]
    public void ResetWorks()
    {
        // --- Arrange
        var kd = new ZxSpectrum48Machine().KeyboardDevice;

        // --- Act
        kd.Reset();

        // --- Assert
        for (var i = 0; i < 40; i++)
        {
            var status = kd.GetStatus((SpectrumKeyCode)i);
            status.ShouldBeFalse();
        }
    }

    [Theory]
    [InlineData(SpectrumKeyCode.CShift, 0xfefe, 0)]
    [InlineData(SpectrumKeyCode.Z, 0xfefe, 1)]
    [InlineData(SpectrumKeyCode.X, 0xfefe, 2)]
    [InlineData(SpectrumKeyCode.C, 0xfefe, 3)]
    [InlineData(SpectrumKeyCode.V, 0xfefe, 4)]
    [InlineData(SpectrumKeyCode.A, 0xfdfe, 0)]
    [InlineData(SpectrumKeyCode.S, 0xfdfe, 1)]
    [InlineData(SpectrumKeyCode.D, 0xfdfe, 2)]
    [InlineData(SpectrumKeyCode.F, 0xfdfe, 3)]
    [InlineData(SpectrumKeyCode.G, 0xfdfe, 4)]
    [InlineData(SpectrumKeyCode.Q, 0xfbfe, 0)]
    [InlineData(SpectrumKeyCode.W, 0xfbfe, 1)]
    [InlineData(SpectrumKeyCode.E, 0xfbfe, 2)]
    [InlineData(SpectrumKeyCode.R, 0xfbfe, 3)]
    [InlineData(SpectrumKeyCode.T, 0xfbfe, 4)]
    [InlineData(SpectrumKeyCode.N1, 0xf7fe, 0)]
    [InlineData(SpectrumKeyCode.N2, 0xf7fe, 1)]
    [InlineData(SpectrumKeyCode.N3, 0xf7fe, 2)]
    [InlineData(SpectrumKeyCode.N4, 0xf7fe, 3)]
    [InlineData(SpectrumKeyCode.N5, 0xf7fe, 4)]
    [InlineData(SpectrumKeyCode.N0, 0xeffe, 0)]
    [InlineData(SpectrumKeyCode.N9, 0xeffe, 1)]
    [InlineData(SpectrumKeyCode.N8, 0xeffe, 2)]
    [InlineData(SpectrumKeyCode.N7, 0xeffe, 3)]
    [InlineData(SpectrumKeyCode.N6, 0xeffe, 4)]
    [InlineData(SpectrumKeyCode.P, 0xdffe, 0)]
    [InlineData(SpectrumKeyCode.O, 0xdffe, 1)]
    [InlineData(SpectrumKeyCode.I, 0xdffe, 2)]
    [InlineData(SpectrumKeyCode.U, 0xdffe, 3)]
    [InlineData(SpectrumKeyCode.Y, 0xdffe, 4)]
    [InlineData(SpectrumKeyCode.Enter, 0xbffe, 0)]
    [InlineData(SpectrumKeyCode.L, 0xbffe, 1)]
    [InlineData(SpectrumKeyCode.K, 0xbffe, 2)]
    [InlineData(SpectrumKeyCode.J, 0xbffe, 3)]
    [InlineData(SpectrumKeyCode.H, 0xbffe, 4)]
    [InlineData(SpectrumKeyCode.Space, 0x7ffe, 0)]
    [InlineData(SpectrumKeyCode.SShift, 0x7ffe, 1)]
    [InlineData(SpectrumKeyCode.M, 0x7ffe, 2)]
    [InlineData(SpectrumKeyCode.N, 0x7ffe, 3)]
    [InlineData(SpectrumKeyCode.B, 0x7ffe, 4)]
    public void SingleKeyDownWorks(SpectrumKeyCode key, int port, int bitPos)
    {
        // --- Arrange
        var kd = new ZxSpectrum48Machine().KeyboardDevice;
        kd.SetStatus(key, true);

        // --- Act
        var status = kd.GetKeyLineStatus((ushort)port);

        // --- Assert
        status.ShouldBe((byte)~(1 << bitPos));
    }

    [Theory]
    [InlineData(SpectrumKeyCode.CShift, 0xfefe, 1 << 0)]
    [InlineData(SpectrumKeyCode.Z, 0xfefe, 1 << 1 | 1 << 0)]
    [InlineData(SpectrumKeyCode.X, 0xfefe, 1 << 2 | 1 << 0)]
    [InlineData(SpectrumKeyCode.C, 0xfefe, 1 << 3 | 1 << 0)]
    [InlineData(SpectrumKeyCode.V, 0xfefe, 1 << 4 | 1 << 0)]
    [InlineData(SpectrumKeyCode.A, 0xfdfe, 1 << 0)]
    [InlineData(SpectrumKeyCode.S, 0xfdfe, 1 << 1)]
    [InlineData(SpectrumKeyCode.D, 0xfdfe, 1 << 2)]
    [InlineData(SpectrumKeyCode.F, 0xfdfe, 1 << 3)]
    [InlineData(SpectrumKeyCode.G, 0xfdfe, 1 << 4)]
    [InlineData(SpectrumKeyCode.Q, 0xfbfe, 1 << 0)]
    [InlineData(SpectrumKeyCode.W, 0xfbfe, 1 << 1)]
    [InlineData(SpectrumKeyCode.E, 0xfbfe, 1 << 2)]
    [InlineData(SpectrumKeyCode.R, 0xfbfe, 1 << 3)]
    [InlineData(SpectrumKeyCode.T, 0xfbfe, 1 << 4)]
    [InlineData(SpectrumKeyCode.N1, 0xf7fe, 1 << 0)]
    [InlineData(SpectrumKeyCode.N2, 0xf7fe, 1 << 1)]
    [InlineData(SpectrumKeyCode.N3, 0xf7fe, 1 << 2)]
    [InlineData(SpectrumKeyCode.N4, 0xf7fe, 1 << 3)]
    [InlineData(SpectrumKeyCode.N5, 0xf7fe, 1 << 4)]
    [InlineData(SpectrumKeyCode.N0, 0xeffe, 1 << 0)]
    [InlineData(SpectrumKeyCode.N9, 0xeffe, 1 << 1)]
    [InlineData(SpectrumKeyCode.N8, 0xeffe, 1 << 2)]
    [InlineData(SpectrumKeyCode.N7, 0xeffe, 1 << 3)]
    [InlineData(SpectrumKeyCode.N6, 0xeffe, 1 << 4)]
    [InlineData(SpectrumKeyCode.P, 0xdffe, 1 << 0)]
    [InlineData(SpectrumKeyCode.O, 0xdffe, 1 << 1)]
    [InlineData(SpectrumKeyCode.I, 0xdffe, 1 << 2)]
    [InlineData(SpectrumKeyCode.U, 0xdffe, 1 << 3)]
    [InlineData(SpectrumKeyCode.Y, 0xdffe, 1 << 4)]
    [InlineData(SpectrumKeyCode.Enter, 0xbffe, 1 << 0)]
    [InlineData(SpectrumKeyCode.L, 0xbffe, 1 << 1)]
    [InlineData(SpectrumKeyCode.K, 0xbffe, 1 << 2)]
    [InlineData(SpectrumKeyCode.J, 0xbffe, 1 << 3)]
    [InlineData(SpectrumKeyCode.H, 0xbffe, 1 << 4)]
    [InlineData(SpectrumKeyCode.Space, 0x7ffe, 1 << 0)]
    [InlineData(SpectrumKeyCode.SShift, 0x7ffe, 1 << 1)]
    [InlineData(SpectrumKeyCode.M, 0x7ffe, 1 << 2)]
    [InlineData(SpectrumKeyCode.N, 0x7ffe, 1 << 3)]
    [InlineData(SpectrumKeyCode.B, 0x7ffe, 1 << 4)]
    public void KeysDownWithCShiftWork(SpectrumKeyCode key, int port, int data)
    {
        // --- Arrange
        var kd = new ZxSpectrum48Machine().KeyboardDevice;
        kd.SetStatus(SpectrumKeyCode.CShift, true);
        kd.SetStatus(key, true);

        // --- Act
        var cStatus = kd.GetKeyLineStatus(0xfefe);
        var status = kd.GetKeyLineStatus((ushort)port);

        // --- Assert
        (cStatus & 0x01).ShouldBe((byte)0x00);
        status.ShouldBe((byte)~(data));
    }

}

