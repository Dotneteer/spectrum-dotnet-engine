using ManagedBass;
using SpectrumEngine.Emu;
using System;
using System.Linq;

namespace SpectrumEngine.Client.Avalonia.Providers
{
    public class BassAudioProvider : IAudioProvider
    {
        public const int SAMPLE_RATE = 48000;

        private float _volume;
        private int? _streamHandle;
        private bool _disposedValue;

        public BassAudioProvider(IAudioDevice audioDevice)
        {
            AudioDevice = audioDevice;
        }

        ~BassAudioProvider()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Tha audio device of the emulated machine
        /// </summary>
        public IAudioDevice AudioDevice { get; }

        /// <summary>
        /// Volume for audio 0 (silent) to 1 (max).
        /// </summary>
        public float Volume 
        {
            get => _volume;
            set => _volume = Math.Clamp(value, 0, 1);
        }

        /// <summary>
        /// Adds the specified set of pulse samples to the sound
        /// </summary>
        /// <param name="samples">
        /// Array of sound samples (values between 0.0F and 1.0F)
        /// </param>
        public void AddSamples(float[] samples)
        {
            if (!_streamHandle.HasValue) return;

            // convert to 16-bits with volume
            var bytes = samples.Select(item => (short)(item * short.MaxValue * _volume)).ToArray();

            _ = Bass.StreamPutData(_streamHandle.Value, bytes, samples.Length * 2);
        }

        /// <summary>
        /// Restes the audio provider
        /// </summary>
        public void Reset()
        {
            if (!_streamHandle.HasValue) return;

            try
            {
                Bass.Stop();
                Bass.Free();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            InitializeDevice();
        }

        /// <summary>
        /// Turn on playing sound
        /// </summary>
        public void Play()
        {
            if (!_streamHandle.HasValue)
            {
                InitializeDevice();
            } 
            else
            {
                Bass.ChannelPlay(_streamHandle.Value);
            }
        }

        /// <summary>
        /// Pause playing sound
        /// </summary>
        public void Pause()
        {
            if (!_streamHandle.HasValue) return;
            Bass.ChannelPause(_streamHandle.Value);
        }

        /// <summary>
        /// Stop playing sound
        /// </summary>
        public void Stop()
        {
            if (!_streamHandle.HasValue) return;
            Bass.ChannelStop(_streamHandle.Value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                Bass.Stop();
                Bass.Free();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void InitializeDevice()
        {
            _volume = .15F;
            Bass.UpdatePeriod = 10;

            // initialize default output device (and measure latency)
            if (!Bass.Init(-1, SAMPLE_RATE, DeviceInitFlags.Latency))
            {
                System.Diagnostics.Debug.WriteLine("Can't initialize audio device");
            }

            Bass.GetInfo(out BassInfo info);
            System.Diagnostics.Debug.Write(info);

            if (_streamHandle.HasValue)
            {
                Bass.StreamFree(_streamHandle.Value);
            }

            // create stream and play it
            _streamHandle = Bass.CreateStream(SAMPLE_RATE, 1, BassFlags.Default | BassFlags.Mono, StreamProcedureType.Push);
            Bass.ChannelPlay(_streamHandle.Value);
        }
    }
}
