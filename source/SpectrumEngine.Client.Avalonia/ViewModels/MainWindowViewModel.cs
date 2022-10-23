// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

using System.Collections.Generic;
using Avalonia;
using Avalonia.Platform;
using SpectrumEngine.Client.Avalonia.Controls.DevTools;
using SpectrumEngine.Client.Avalonia.Controls.Emulators;

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
            RaisePropertyChanged(nameof(Cpu));
            Ula = new UlaPanelViewModel(change.NewController);
            RaisePropertyChanged(nameof(Ula));
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
        
        // --- Initiualize activities
        Activities = new List<ActivityViewModel>
        {
            new()
            {
                Id = "Project",
                Icon = "IconFiles",
                Description = "Project Explorer"
            },
            new()
            {
                Id = "Debug",
                Icon = "IconDebug",
                Description = "Run and Debug"
            },
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

        Commands = new CommandsPanelViewModel();
        Disassembler = new DisassemblyViewModel(this)
        {
            FullRangeFrom = 0x0000,
            FullRangeTo = 0x3fff
        };
        Debugger = new DebugViewModel();
        Debugger.BreakpointsChanged += (_, bpInfo) =>
        {
            Disassembler.ApplyBreakpointChanges(bpInfo.OldBps, bpInfo.NewBps);
        };

        MemoryViewer = new MemoryViewModel
        {
            RangeFrom = 0x0000,
            RangeTo = 0xffff,
            LastSetTopPosition = -1
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
    /// DevTool activities
    /// </summary>
    public List<ActivityViewModel> Activities { get; }
    
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
    
    /// <summary>
    /// Represents the view model of the Commands panel
    /// </summary>
    public CommandsPanelViewModel Commands { get; }
    
    /// <summary>
    /// Represents the Debugger's view model
    /// </summary>
    public DebugViewModel Debugger { get; }
    
    /// <summary>
    /// Represents the disassembler's view model
    /// </summary>
    public DisassemblyViewModel Disassembler { get; }
    
    /// <summary>
    /// Represents the memory viewer's view model
    /// </summary>
    public MemoryViewModel MemoryViewer { get; }
}
