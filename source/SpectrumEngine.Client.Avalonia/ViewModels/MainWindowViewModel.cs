// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

using Avalonia;
using Avalonia.Platform;
using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        // --- Get environment information
        var os = AvaloniaLocator.Current.GetService<IRuntimePlatform>()!.GetRuntimeInfo();
        UseNativeMenu = os.OperatingSystem is OperatingSystemType.OSX;

        // --- Initialize preferences
        Preferences = new PreferencesViewModel();

        // --- Initialize the machine information
        Machine = new MachineViewModel();
        Machine.ControllerChanged += (sender, change) =>
        {
            if (change.NewController == null) return;
            Cpu = new CpuPanelViewModel(change.NewController);
            Ula = new UlaPanelViewModel(change.NewController);
        };
        
        // --- Spectrum Display
        Display = new DisplayViewModel();
        
        // --- Initialize view options
        EmuViewOptions = new EmuViewOptionsViewModel(this)
        {
            ShowMenuBar = false,
            ShowToolbar = true,
            ShowStatusBar = true,
            ShowKeyboard = false,
            IsMuted = false
        };
        DevToolsViewOptions = new DevToolsViewOptionsViewModel(this)
        {
            ShowMenuBar = false,
            ShowDevTools = false,
            ShowToolbar = true,
            ShowStatusBar = true,
            ShowSiteBar = true,
            ShowPanels = true,
            SiteBarOnLeft = true,
            PanelsAtBottom = true
        };
        
        // --- Initialize the parts of the DevTools window
        SiteBarViewOptions = new SiteBarViewOptionsViewModel
        {
            ShowCpu = true,
            ShowUla = true,
            ShowBreakpoints = true
        };
        SiteBarViewOptions.OnPanelGotVisible += (_, _) =>
        {
            if (!SiteBarViewOptions.ShowCpu && !SiteBarViewOptions.ShowUla && !SiteBarViewOptions.ShowBreakpoints)
            {
                DevToolsViewOptions.ShowSiteBar = false;
                return;
            }
            if (!DevToolsViewOptions.ShowSiteBar) DevToolsViewOptions.ShowSiteBar = true;
        };

        PanelsViewOptions = new PanelsViewOptionsViewModel
        {
            ShowMemory = true,
            ShowDisassembly = true,
            ShowWatch = false,
            SelectedIndex = 0
        };

        var buffer = new OutputBuffer();
        for (var i = 0; i < 10; i++)
        {
            buffer.Color = OutputColors.Cyan;
            buffer.Write("Hello");
            buffer.Color = OutputColors.Green;
            buffer.Strikethrough = true;
            buffer.WriteLine("World");
            buffer.Strikethrough = false;
            buffer.Bold = true;
            buffer.Color = OutputColors.Red;
            buffer.Write("Welcome ");
            buffer.Bold = false;
            buffer.Italic = true;
            buffer.Background = OutputColors.Red;            
            buffer.WriteLine("in SpectrumEngine");
            buffer.Italic = false;
            buffer.Bold = false;
        }

        Commands = new CommandsPanelViewModel
        {
            Buffer = buffer
        };
    }
    
    /// <summary>
    /// Should use the native menu feature of OSX?
    /// </summary>
    public bool UseNativeMenu { get; }

    /// <summary>
    /// The machine state partition of the view model
    /// </summary>
    public MachineViewModel Machine { get; }

    /// <summary>
    /// The view options part of the view model
    /// </summary>
    public EmuViewOptionsViewModel EmuViewOptions { get; }

    /// <summary>
    /// The DevTools partition of the view model
    /// </summary>
    public DevToolsViewOptionsViewModel DevToolsViewOptions { get; }

    /// <summary>
    /// The display view model created by the machine
    /// </summary>
    public DisplayViewModel Display { get; }
    
    /// <summary>
    /// Application preferences
    /// </summary>
    public PreferencesViewModel Preferences { get; }

    /// <summary>
    /// DevTools site bar view options
    /// </summary>
    public SiteBarViewOptionsViewModel SiteBarViewOptions { get; }
    
    /// <summary>
    /// DevTools panels view options
    /// </summary>
    public PanelsViewOptionsViewModel PanelsViewOptions { get; }
    
    /// <summary>
    /// Represents the CPU's view model
    /// </summary>
    public CpuPanelViewModel? Cpu { get; private set; }
    
    /// <summary>
    /// Represents the ULA's view model
    /// </summary>
    public UlaPanelViewModel? Ula { get; private set; }
    
    public CommandsPanelViewModel Commands { get; private set; }
}
