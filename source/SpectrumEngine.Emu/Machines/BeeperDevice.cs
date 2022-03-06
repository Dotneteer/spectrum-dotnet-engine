namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum beeper device.
/// </summary>
public sealed class BeeperDevice : IBeeperDevice
{
    private const int GATE = 100_000;

    private int _audioSampleLength;
    private int _audioLowerGate;
    private int _audioGateValue;
    private int _audioNextSampleTact;
    private readonly List<float> _audioSamples = new();

    /// <summary>
    /// Initialize the beeper device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public BeeperDevice(IZxSpectrum48Machine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrum48Machine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        AudioSampleRate = -1;
        _audioSampleLength = 0;
        _audioLowerGate = 0;
        _audioSamples.Clear();
    }

    /// <summary>
    /// Gets the current audio sample rate of the device
    /// </summary>
    public int AudioSampleRate { get; private set; }

    /// <summary>
    /// Gets the number of audio samples in a machine frame.
    /// </summary>
    /// <remarks>
    /// Because of rounding, there are machine frames that have one more sample than this value shows
    /// </remarks>
    public int AudioSamplesInFrame => (Machine.TactsInFrame * Machine.ClockMultiplier / _audioSampleLength);

    /// <summary>
    /// Sets up the sample rate to use with this device
    /// </summary>
    /// <param name="sampleRate">Audio sample rate</param>
    public void SetAudioSampleRate(int sampleRate)
    {
        AudioSampleRate = sampleRate;
        var sampleLenght = (double)Machine.BaseClockFrequency * Machine.ClockMultiplier / sampleRate;
        _audioSampleLength = (int)sampleLenght;
        _audioLowerGate = (int)((sampleLenght - _audioSampleLength) * GATE);
        _audioGateValue = 0;
    }

    /// <summary>
    /// Gets the last EAR bit value.
    /// </summary>
    public bool EarBitValue { get; private set; }

    /// <summary>
    /// This method sets the EAR bit value to generate sound with the beeper.
    /// </summary>
    /// <param name="value">EAR bit value to set</param>
    public void SetEarBit(bool value)
    {
        EarBitValue = value;
        // TODO: Generate the beeper sound sample
    }

    /// <summary>
    /// Renders the subsequent beeper sample according to the current EAR bit value
    /// </summary>
    public void RenderBeeperSample()
    {
        if (Machine.CurrentFrameTact > _audioNextSampleTact)
        {
            _audioSamples.Add(EarBitValue ? 1.0f : 0.0f);
            _audioGateValue += _audioLowerGate;
            _audioNextSampleTact += _audioSampleLength;
            if (_audioGateValue >= GATE)
            {
                _audioNextSampleTact += 1;
                _audioGateValue -= GATE;
            }
        }
    }

    /// <summary>
    /// Gets the audio samples rendered in the current frame
    /// </summary>
    /// <returns>Array with the audio samples</returns>
    public float[] GetAudioSamples() => _audioSamples.ToArray();

    /// <summary>
    /// This method signs that a new machine frame has been started
    /// </summary>
    public void OnNewFrame()
    {
        var cpuTactsInFrame = Machine.TactsInFrame * Machine.ClockMultiplier;
        if (_audioNextSampleTact != 0)
        {
            if (_audioNextSampleTact > cpuTactsInFrame)
            {
                _audioNextSampleTact -= cpuTactsInFrame;
            }
            else
            {
                _audioSamples.Add(EarBitValue ? 1.0f : 0.0f);
                _audioNextSampleTact = _audioSampleLength - cpuTactsInFrame + _audioNextSampleTact;
            }
        }
        _audioSamples.Clear();
    }
}