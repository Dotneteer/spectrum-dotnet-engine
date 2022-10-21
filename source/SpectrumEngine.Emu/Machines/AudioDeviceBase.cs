using System.Diagnostics;
using System.Text.Json;

namespace SpectrumEngine.Emu;

/// <summary>
/// Ths class represents the functionality of an audio device that can generate audio samples
/// </summary>
public class AudioDeviceBase: IAudioDevice
{
    private const int GATE = 100_000;

    private int _audioSampleRate;
    private int _audioSampleLength;
    private int _audioLowerGate;
    private int _audioGateValue;
    private int _audioNextSampleTact;
    private readonly List<float> _audioSamples = new();

    /// <summary>
    /// Initialize the beeper device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    protected AudioDeviceBase(IZxSpectrumMachine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Release resources
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrumMachine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public virtual void Reset()
    {
        _audioSampleLength = 0;
        _audioLowerGate = 0;
        _audioSamples.Clear();
    }

    /// <summary>
    /// Gets the audio sample rate
    /// </summary>
    public int GetAudioSampleRate() => _audioSampleRate;

    /// <summary>
    /// Sets up the sample rate to use with this device
    /// </summary>
    /// <param name="sampleRate">Audio sample rate</param>
    public void SetAudioSampleRate(int sampleRate)
    {
        _audioSampleRate = sampleRate;
        var sampleLength = (double)Machine.BaseClockFrequency / sampleRate;
        _audioSampleLength = (int)sampleLength;
        _audioLowerGate = (int)((sampleLength - _audioSampleLength) * GATE);
        _audioGateValue = 0;
    }

    /// <summary>
    /// Gets the audio samples rendered in the current frame
    /// </summary>
    /// <returns>Array with the audio samples</returns>
    public float[] GetAudioSamples() => _audioSamples.ToArray();

    /// <summary>
    /// This method signs that a new machine frame has been started
    /// </summary>
    public virtual void OnNewFrame()
    {
        var cpuTactsInFrame = Machine.TactsInFrame;
        if (_audioNextSampleTact != 0)
        {
            if (_audioNextSampleTact > cpuTactsInFrame)
            {
                _audioNextSampleTact -= cpuTactsInFrame;
            }
            else
            {
                _audioSamples.Add(GetCurrentSampleValue());
                _audioNextSampleTact = _audioSampleLength - cpuTactsInFrame + _audioNextSampleTact;
            }
        }
        _audioSamples.Clear();
    }

    /// <summary>
    /// Calculates the current audio value according to the CPU's clock
    /// </summary>
    /// <remarks>
    /// We do not need to calculate the value, as it is always the value of the last EAR bit
    /// </remarks>
    public virtual void CalculateCurrentAudioValue()
    {
        // --- Intentionally empty
    }

    /// <summary>
    /// Renders the subsequent beeper sample according to the current EAR bit value
    /// </summary>
    public void SetNextAudioSample()
    {
        CalculateCurrentAudioValue();
        if (Machine.CurrentFrameTact <= _audioNextSampleTact) return;
        
        _audioSamples.Add(GetCurrentSampleValue());
        _audioGateValue += _audioLowerGate;
        _audioNextSampleTact += _audioSampleLength;
        if (_audioGateValue < GATE) return;
        
        _audioNextSampleTact += 1;
        _audioGateValue -= GATE;
    }

    /// <summary>
    /// Gets the current sound sample (according to the current CPU tact)
    /// </summary>
    /// <returns>Sound sample value</returns>
    protected virtual float GetCurrentSampleValue()
    {
        return 0.0f;
    }
}