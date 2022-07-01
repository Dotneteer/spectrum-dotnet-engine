using System;

namespace SpectrumEngine.Client.Avalonia.Providers
{
    public interface IAudioProvider : IDisposable
    {
        /// <summary>
        /// Turn on playing sound
        /// </summary>
        public void Play();

        /// <summary>
        /// Pause playing sound
        /// </summary>
        public void Pause();

        /// <summary>
        /// Stop playing sound
        /// </summary>
        public void Stop();

        /// <summary>
        /// Adds the specified set of pulse samples to the sound
        /// </summary>
        /// <param name="samples">
        /// Array of sound samples (values between 0.0F and 1.0F)
        /// </param>
        public void AddSamples(float[] samples);
    }
}
