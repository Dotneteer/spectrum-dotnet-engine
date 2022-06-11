using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

/// <summary>
/// Represents the state of the host environment
/// </summary>
public class EnvironmentViewModel: ViewModelBase
{
    public EnvironmentViewModel()
    {
        var os = AvaloniaLocator.Current.GetService<IRuntimePlatform>()!.GetRuntimeInfo();
        UseNativeMenu = os.OperatingSystem is OperatingSystemType.OSX;
        InitialWindowState = UseNativeMenu ? WindowState.FullScreen : WindowState.Maximized;
    }
    
    /// <summary>
    /// Should use the native menu feature of OSX?
    /// </summary>
    public bool UseNativeMenu { get; }

    /// <summary>
    /// Represents the initial state of the app's main window (OS-dependent)
    /// </summary>
    public WindowState InitialWindowState { get; }

}