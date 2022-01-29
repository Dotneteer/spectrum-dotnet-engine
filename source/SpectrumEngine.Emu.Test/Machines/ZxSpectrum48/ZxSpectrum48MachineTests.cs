namespace SpectrumEngine.Emu.Test.Machines.ZxSpectrum48;
public class ZxSpectrum48MachineTests
{
    [Fact]
    public void ConstructionWorks()
    {
        // --- Act
        var m = new ZxSpectrum48Machine();

        // --- Assert
        m.Cpu.ShouldNotBeNull();
        m.MemoryDevice.ShouldNotBeNull();
        m.IoHandler.ShouldNotBeNull();
        m.KeyboardDevice.ShouldNotBeNull();
        m.ScreenDevice.ShouldNotBeNull();
        m.BeeperDevice.ShouldNotBeNull();
        m.FloatingBusDevice.ShouldNotBeNull();
        m.TapeDevice.ShouldNotBeNull();
        m.BaseClockFrequency.ShouldBe(3_500_000);

        var cpu = m.Cpu;
        cpu.Tacts.ShouldBe(0ul);
        cpu.Frames.ShouldBe(0u);
        cpu.CurrentFrameTact.ShouldBe(0u);
        cpu.TactsInFrame.ShouldBe(69888);

        var md = m.MemoryDevice;
        md.ReadMemory(0x0000).ShouldBe((byte)0xF3);
        md.ReadMemory(0x3fff).ShouldBe((byte)0x3C);

        var sd = m.ScreenDevice;
        sd.RasterLines.ShouldBe(312);
        sd.ScreenWidth.ShouldBe(352);
        
    }
}
