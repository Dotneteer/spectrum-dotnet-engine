using SpectrumEngine.Emu;
using System.Windows;

namespace SpectrumEngine.Wpf.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        AppViewModel.Instance.SetMachineController(new MachineController(new ZxSpectrum48Machine()));
    }
}
