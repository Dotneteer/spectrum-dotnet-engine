using Avalonia;
using System.Reactive.Subjects;

namespace SpectrumEngine.Client.Avalonia.Services
{
    /// <summary>
    /// Service for manage application states
    /// </summary>
    public interface IApplicationService
    {       
        /// <summary>
        /// Get application instance
        /// </summary>
        Application Application { get; }

        /// <summary>
        /// Get or set flag for busy state
        /// </summary>
        bool IsBusy { get; set; }

        /// <summary>
        /// Subject for track Busy change states
        /// </summary>
        Subject<bool> IsBusyChange { get; }

        /// <summary>
        /// Shutdown application
        /// </summary>
        void ShutdownApplication();

        /// <summary>
        /// Dispose instance
        /// </summary>
        void Dispose();
    }
}