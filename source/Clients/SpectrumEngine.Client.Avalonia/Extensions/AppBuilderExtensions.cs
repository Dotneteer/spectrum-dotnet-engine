using Avalonia;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumEngine.Client.Avalonia
{
    public static class AppBuilderExtensions
    {

        public static AppBuilder RegisterComponents(this AppBuilder appBuilder)
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

            return appBuilder;
        }
    }
}
