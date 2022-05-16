using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;
using Splat;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public ViewModelBase()
        {
            MainWindow = Locator.Current.GetService<IMainWindow>()!;
        }

        public IMainWindow MainWindow { get; }

        [Reactive]
        public bool IsBusy { get; set; } = false;
        //protected bool _isBusy;
        //public bool IsBusy
        //{
        //    get
        //    {
        //        return _isBusy;
        //    }
        //    set
        //    {
        //        if (value == _isBusy) return;

        //        _isBusy = value;
        //        this._events.PublishOnUIThread(new BusyEvent(_isBusy));
        //        NotifyOfPropertyChange(() => IsBusy);
        //    }
        //}
    }
}
