using ReactiveUI;
using Splat;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public abstract class RoutableViewModelBase : ReactiveObject, IRoutableViewModel
    {
        public RoutableViewModelBase()
        {
            HostScreen = Locator.Current.GetService<IScreen>()!;
        }

        public IScreen HostScreen { get; }

        public abstract string? UrlPathSegment { get; }
    }
}
