namespace SpectrumEngine.Emu;

/// <summary>
/// This interface represents anaudio device that creates sound samples according to a particular sample rate.
/// </summary>
public interface IAudioDevice : IGenericDevice<IZxSpectrumMachine>
{
    /// <summary>
    /// Gets the audio sample rate
    /// </summary>
    int GetAudioSampleRate();
    
    /// <summary>
    /// Sets up the sample rate to use with this device
    /// </summary>
    /// <param name="sampleRate">Audio sample rate</param>
    void SetAudioSampleRate(int sampleRate);

    /// <summary>
    /// Gets the audio samples rendered in the current frame
    /// </summary>
    /// <returns>Array with the audio samples</returns>
    float[] GetAudioSamples();

    /// <summary>
    /// This method signs that a new machine frame has been started
    /// </summary>
    void OnNewFrame();

    /// <summary>
    /// Renders the subsequent beeper sample according to the current EAR bit value
    /// </summary>
    void SetNextAudioSample();

    /// <summary>
    /// Calculates the current audio value according to the CPU's clock
    /// </summary>
    /// <remarks>
    /// We do not need to calculate the value, as it is always the value of the last EAR bit
    /// </remarks>
    void CalculateCurrentAudioValue();
}

