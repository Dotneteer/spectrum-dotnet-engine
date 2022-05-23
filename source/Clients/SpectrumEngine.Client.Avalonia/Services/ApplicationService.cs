using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using SpectrumEngine.Emu;
using System;
using System.Reactive.Subjects;

namespace SpectrumEngine.Client.Avalonia.Services
{
    public class ApplicationService : IDisposable, IApplicationService
    {
        private readonly Application application;
        private bool disposedValue;

        private bool isBusy;
        private readonly Subject<bool> isBusyChange;

        public ApplicationService()
        {
            application = Application.Current ?? throw new InvalidProgramException("Application.Current is null on ApplicationService");
            isBusyChange = new Subject<bool>();
        }

        public Application Application => application;

        public bool IsBusy
        {
            set
            {
                if (isBusy == value) return;

                isBusy = value;
                isBusyChange.OnNext(value);
            }
            get => isBusy;
        }

        public Subject<bool> IsBusyChange => isBusyChange;

        public void ShutdownApplication()
        {
            Logger.Flush();
            if (application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    isBusyChange.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}
