namespace SpectrumEngine.Emu.Machines.ZxSpectrum128;

public class ZxSpectrum128PsgDevice: AudioDeviceBase, IPsgDevice
{
    // ---The number of ULA tacts that represent a single PSG clock tick
    private const int PSG_CLOCK_STEP = 16;
    
    // --- The value of the next ULA tact when a PSG output value should be generated
    private long _psgNextClockTact;

    // --- The PsgChip instance that provides the sound sample calculation
    private readonly PsgChip _psg = new();

    /// <summary>
    /// Initialize the beeper device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum128PsgDevice(IZxSpectrumMachine machine) : base(machine)
    {
        // --- Set the first tact to create a sample for
        _psgNextClockTact = PSG_CLOCK_STEP;
    }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        _psg.Reset();
    }

    /// <summary>
    /// Reads the value of the PSG register addressed with the las SetPsgRegisterIndex operation
    /// </summary>
    /// <returns>The value of the PSG register</returns>
    public byte ReadPsgRegisterValue() => _psg.ReadPsgRegisterValue();

    /// <summary>
    /// Writes the value of the PSG register addressed with the las SetPsgRegisterIndex operation
    /// </summary>
    /// <param name="value">Value to set for the PSG register</param>
    public void WritePsgRegisterValue(byte value) => _psg.WritePsgRegisterValue(value);

    /// <summary>
    /// Sets the PSG register index to read or write
    /// </summary>
    /// <param name="index">PSG register index</param>
    public void SetPsgRegisterIndex(int index) => _psg.SetPsgRegisterIndex(index);

    /// <summary>
    /// Calculates the current audio value according to the CPU's clock
    /// </summary>
    /// <remarks>
    /// We do not need to calculate the value, as it is always the value of the last EAR bit
    /// </remarks>
    public override void CalculateCurrentAudioValue()
    {
        while (Machine.CurrentFrameTact >= _psgNextClockTact) {
            GeneratePsgOutputValue();
            _psgNextClockTact += PSG_CLOCK_STEP;
        }    
    }

    /// <summary>
    /// Gets the current sound sample (according to the current CPU tact)
    /// </summary>
    /// <returns>Sound sample value</returns>
    protected override float GetCurrentSampleValue()
    {
        var value = _psg.OrphanSamples > 0
            ? (float)_psg.OrphanSum / _psg.OrphanSamples / 65535
            : 0.0f;
        _psg.OrphanSum = 0;
        _psg.OrphanSamples = 0;
        return value;
    }

    /// <summary>
    /// This method signs that a new machine frame has been started
    /// </summary>
    public override void OnNewFrame()
    {
        base.OnNewFrame();
        if (_psgNextClockTact >= Machine.TactsInFrame)
        {
            _psgNextClockTact -= Machine.TactsInFrame;
        }
    }

    /// <summary>
    /// Generates the current PSG output value according to current register settings
    /// </summary>
    private void GeneratePsgOutputValue() => _psg.GenerateOutputValue();
}