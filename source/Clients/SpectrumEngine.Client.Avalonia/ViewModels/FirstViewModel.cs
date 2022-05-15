using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumEngine.Client.Avalonia.ViewModels
{
    public class FirstViewModel : ViewModelBase, IRoutableViewModel
    {
        public string UrlPathSegment => "first";

        public IScreen HostScreen { get; }

        public FirstViewModel() : this(Locator.Current.GetService<IScreen>())
        {
        }

        public FirstViewModel(IScreen? screen = null)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
        }
    }
}
