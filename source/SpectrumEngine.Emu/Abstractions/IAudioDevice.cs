namespace SpectrumEngine.Emu;

/// <summary>
/// This interface represents anaudio device that creates sound samples according to a particular sample rate.
/// </summary>
public interface IAudioDevice : IGenericDevice<IZxSpectrum48Machine>
{
    /// <summary>
    /// Gets the current audio sample rate of the device
    /// </summary>
    int AudioSampleRate { get; }

    /// <summary>
    /// Sets up the sample rate to use with this device
    /// </summary>
    /// <param name="sampleRate">Audio sample rate</param>
    void SetAudioSampleRate(int sampleRate);

    /// <summary>
    /// Gets the number of audio samples in a machine frame.
    /// </summary>
    /// <remarks>
    /// Because of rounding, there are machine frames that have one more sample than this value shows
    /// </remarks>
    int AudioSamplesInFrame { get; }

    /// <summary>
    /// Gets the audio samples rendered in the current frame
    /// </summary>
    /// <returns>Array with the audio samples</returns>
    float[] GetAudioSamples();

    /// <summary>
    /// This method signs that a new machine frame has been started
    /// </summary>
    void OnNewFrame();
}

