using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected TRet SetProperty<TRet>(
            ref TRet backingField,
            TRet newValue,
            [CallerMemberName] string? propertyName = null)
        {
            return this.RaiseAndSetIfChanged(ref backingField, newValue, propertyName);
        }

        public void RaisePropertyChanged(string name)
        {
            ((IReactiveObject) this).RaisePropertyChanged(name);
        }
    }
}