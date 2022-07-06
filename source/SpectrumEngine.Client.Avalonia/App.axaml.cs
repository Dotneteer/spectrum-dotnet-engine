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
        
        public static MainWindow? AppWindow { get; private set; }
        
        public static DevToolsWindow? DevToolsWindow { get; set; }
        
        public static bool IsAppClosing { get; private set; }
        
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
                desktop.MainWindow = AppWindow = new MainWindow
                {
                    DataContext = AppViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static void ShowDevToolsWindow()
        {
            if (DevToolsWindow == null)
            {
                DevToolsWindow = new DevToolsWindow
                {
                    DataContext = AppViewModel
                };
            }
            DevToolsWindow.Show();
        }

        public static void HideDevToolsWindow()
        {
            DevToolsWindow?.Hide();
        }

        public static void DiscardDevToolsWindow()
        {
            DevToolsWindow = null;
        }

        public static void CloseDevToolsWindow()
        {
            IsAppClosing = true;
            DevToolsWindow?.Close();
        }
        
        private void LoadTheme()
        {
            Styles.Add(new DarkTheme());
        }
    }
}