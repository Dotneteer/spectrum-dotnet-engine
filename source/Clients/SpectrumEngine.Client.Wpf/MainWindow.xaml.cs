using SpectrumEngine.Emu;
using System.Windows;

namespace SpectrumEngine.Client.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = AppViewModel.Instance;
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        Logger.Flush();
        Application.Current.Shutdown();
    }
}
