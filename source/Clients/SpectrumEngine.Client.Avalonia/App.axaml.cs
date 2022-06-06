using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SpectrumEngine.Client.Avalonia.Styles;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Client.Avalonia.Views;

namespace SpectrumEngine.Client.Avalonia
{
    public class App : Application
    {
#pragma warning disable CS8618
        public static MainWindowViewModel AppViewModel;
#pragma warning restore CS8618
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            LoadTheme();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                AppViewModel = new MainWindowViewModel();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = AppViewModel,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        private void LoadTheme()
        {
            Styles.Add(new DarkTheme());
        }
    }
}