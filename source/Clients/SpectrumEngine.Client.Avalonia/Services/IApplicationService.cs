using Avalonia;
using System.Reactive.Subjects;

namespace SpectrumEngine.Client.Avalonia.Services
{
    public interface IApplicationService
    {       
        Application Application { get; }

        bool IsBusy { get; set; }
        Subject<bool> IsBusyChange { get; }

        void ShutdownApplication();

        void Dispose();
    }
}