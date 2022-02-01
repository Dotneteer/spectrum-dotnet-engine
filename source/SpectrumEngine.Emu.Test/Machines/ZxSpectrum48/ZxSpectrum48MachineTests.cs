namespace SpectrumEngine.Emu.Test.Machines.ZxSpectrum48;
public class ZxSpectrum48MachineTests
{
    [Fact]
    public void ConstructionWorks()
    {
        // --- Act
        var m = new ZxSpectrum48Machine();

        // --- Assert
        m.ShouldNotBeNull();
        m.IoHandler.ShouldNotBeNull();
        m.KeyboardDevice.ShouldNotBeNull();
        m.ScreenDevice.ShouldNotBeNull();
        m.BeeperDevice.ShouldNotBeNull();
        m.FloatingBusDevice.ShouldNotBeNull();
        m.TapeDevice.ShouldNotBeNull();
        m.BaseClockFrequency.ShouldBe(3_500_000);

        m.Tacts.ShouldBe(0ul);
        m.TactsInFrame.ShouldBe(69888);

        m.DoReadMemory(0x0000).ShouldBe((byte)0xF3);
        m.DoReadMemory(0x3fff).ShouldBe((byte)0x3C);

        var sd = m.ScreenDevice;
        sd.RasterLines.ShouldBe(312);
        sd.ScreenWidth.ShouldBe(352);
    }

    [Fact]
    public void MachineLoopWorks()
    {
        // --- Arrange
        var m = new ZxSpectrum48Machine
        {
            TargetClockMultiplier = 10
        };

        // --- Act
        for (var i = 0; i < 100; i++)
        {
            var result = m.ExecuteMachineLoop();

            // --- Assert
            result.ShouldBe(LoopTerminationMode.Normal);
        }
    }
}
