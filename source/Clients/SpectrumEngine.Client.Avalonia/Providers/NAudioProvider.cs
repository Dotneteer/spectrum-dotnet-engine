using NAudio.Wave;
using SpectrumEngine.Emu;
using System;

namespace SpectrumEngine.Client.Avalonia.Providers;

/// <summary>
/// This provider uses the NAudio library to create sound from samples.
/// </summary>
public class NAudioProvider : ISampleProvider
{
    /// <summary>
    /// Number of sound frames buffered
    /// </summary>
    public const int FRAMES_BUFFERED = 50;
    public const int FRAMES_DELAYED = 2;

    private float[] _waveBuffer;
    private int _bufferLength;
    private int _frameCount;
    private long _writeIndex;
    private long _readIndex;
    private IWavePlayer? _waveOut;

    private int _written;
    private int _read;

    public NAudioProvider(IAudioDevice audioDevice)
    {
        AudioDevice = audioDevice;
        _waveBuffer = Array.Empty<float>();
    }

    /// <summary>
    /// Tha audio device of the emulated machine
    /// </summary>
    public IAudioDevice AudioDevice { get; }

    /// <summary>
    /// Represents wave file format in memory
    /// </summary>
    public WaveFormat? WaveFormat { get; private set; }

    /// <summary>
    /// Restes the audio provider
    /// </summary>
    public void Reset()
    {
        try
        {
            _waveOut?.Dispose();
        }
        catch
        {
            // --- We ignore this exception deliberately
        }
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48_000, 1);
        _waveOut = null;
        _bufferLength = (AudioDevice.AudioSamplesInFrame + 1) * FRAMES_BUFFERED;
        _waveBuffer = new float[_bufferLength];
        _frameCount = 0;
        _writeIndex = 0;
        _readIndex = 0;

        _written = 0;
        _read = 0;
    }

    /// <summary>
    /// Adds the specified set of pulse samples to the sound
    /// </summary>
    /// <param name="samples">
    /// Array of sound samples (values between 0.0F and 1.0F)
    /// </param>
    public void AddSoundFrame(float[] samples)
    {
        if (_waveOut == null) return;
        foreach (var sample in samples)
        {
            _waveBuffer[_writeIndex++] = sample;
            if (_writeIndex >= _bufferLength) _writeIndex = 0;
        }
        _written += samples.Length;
    }

    /// <summary>
    /// Fill the specified buffer with 32 bit floating point samples
    /// </summary>
    /// <param name="buffer">The buffer to fill with samples.</param>
    /// <param name="offset">Offset into buffer</param>
    /// <param name="count">The number of samples to read</param>
    /// <returns>the number of samples written to the buffer.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
        // --- We set up the initial buffer content for desired latency
        if (_frameCount <= FRAMES_DELAYED)
        {
            for (var i = 0; i < count; i++)
            {
                buffer[offset++] = 0.0F;
            }
        }
        else
        {
            // --- We use the real samples
            for (var i = 0; i < count; i++)
            {
                buffer[offset++] = _waveBuffer[_readIndex++];
                if (_readIndex >= _bufferLength) _readIndex = 0;
            }
        }
        _frameCount++;
        _read += count;
        return count;
    }

    /// <summary>
    /// Turn on playing sound
    /// </summary>
    public void PlaySound()
    {
        if (_waveOut == null)
        {
            SetupSound();
        }
        _waveOut!.Volume = 1.0F;
        _waveOut.Play();
    }

    /// <summary>
    /// Pause playing sound
    /// </summary>
    public void PauseSound()
    {
        _waveOut?.Pause();
    }

    /// <summary>
    /// Stop playing sound
    /// </summary>
    public void KillSound()
    {
        if (_waveOut == null) return;

        _waveOut.Volume = 0.0F;
        _waveOut.Stop();
        _waveOut.Dispose();
        _waveOut = null;
    }

    /// <summary>
    /// Setup the NAudio sound
    /// </summary>
    private void SetupSound()
    {
        Reset();
        _waveOut = new WaveOut
        {
            DesiredLatency = 100,
        };
        _waveOut.Init(this);
        _waveOut.Volume = 1.0F;
    }
}
