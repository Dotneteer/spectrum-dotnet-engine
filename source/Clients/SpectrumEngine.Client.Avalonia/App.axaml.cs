using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SpectrumEngine.Client.Avalonia.ViewModels.Shell;
using SpectrumEngine.Client.Avalonia.Views.Shell;
using Splat;

namespace SpectrumEngine.Client.Avalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindowView
                {
                    DataContext = Locator.Current.GetService<IMainWindow>()!
                };
            }
            
            base.OnFrameworkInitializationCompleted();
        }
    }
}