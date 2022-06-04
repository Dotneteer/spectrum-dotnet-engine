using System.Threading.Tasks;
using Avalonia;
using Avalonia.Metadata;
using Avalonia.Platform;
using SpectrumEngine.Emu;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private MachineController? _mc;
    private MachineControllerState _mstate;
    private int _machineFrames;
    private DisplayViewModel? _display;
    private ViewOptionsViewModel _viewOptionsViewModel;
    private bool _useNativeMenu;

    public MainWindowViewModel()
    {
        _viewOptionsViewModel = new ViewOptionsViewModel();
        var os = AvaloniaLocator.Current.GetService<IRuntimePlatform>()!.GetRuntimeInfo();
        _useNativeMenu = os.OperatingSystem is OperatingSystemType.OSX;
    }
    
    #region OS related properties

    public bool UseNativeMenu => _useNativeMenu;
    
    #endregion
    
    #region Machine Controller related parts
    
    /// <summary>
    /// Set the machine controller to use with this view model
    /// </summary>
    /// <param name="controller">The machine controller to use</param>
    public void SetMachineController(MachineController controller)
    {
        // --- Unsubscribe from the events of the previous controller
        if (_mc != null)
        {
            _mc.StateChanged -= OnStateChanged;
            _mc.FrameCompleted -= OnFrameCompleted;
        }

        // --- Set up the new controller and handle the state changes
        _mc = controller;
        _machineFrames = 0;
        _mc.StateChanged += OnStateChanged;
        _mc.FrameCompleted += OnFrameCompleted;

        // --- Update view model properties
        Display = new DisplayViewModel(_mc);

        void OnStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
        {
            // --- Refresh command states whenever the controller state changes
            MachineControllerState = e.NewState;
            
            // --- Reset frame counter on a start/restart
            if (e.OldState is MachineControllerState.None or MachineControllerState.Stopped 
                && e.NewState is MachineControllerState.Running)
            {
                MachineFrames = 0;
            } 
        }

        void OnFrameCompleted(object? sender, bool completed)
        {
            if (completed) MachineFrames++;
        }
    }

    /// <summary>
    /// Get or set the controller state
    /// </summary>
    public MachineControllerState MachineControllerState
    {
        get => _mstate;
        set => SetProperty(ref _mstate, value);
    }

    /// <summary>
    /// Execute the Start command
    /// </summary>
    public void Start() => _mc?.Start();

    /// <summary>
    /// Enable/disable the Start command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStart(object? parameter)
        => _mc != null && MachineControllerState != MachineControllerState.Running; 
    
    /// <summary>
    /// Execute the Pause command
    /// </summary>
    public Task Pause() => _mc!.Pause();

    /// <summary>
    /// Enable/disable the Pause command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanPause(object? parameter)
        => _mc != null && MachineControllerState == MachineControllerState.Running; 
    
    /// <summary>
    /// Execute the Stop command
    /// </summary>
    public Task Stop() => _mc!.Stop();

    /// <summary>
    /// Enable/disable the Stop command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStop(object? parameter)
        => _mc != null && MachineControllerState == MachineControllerState.Running 
           || MachineControllerState == MachineControllerState.Paused; 
    
    /// <summary>
    /// Execute the Restart command
    /// </summary>
    public Task Restart() => _mc!.Restart();

    /// <summary>
    /// Enable/disable the Restart command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanRestart(object? parameter)
        => _mc != null && MachineControllerState == MachineControllerState.Running 
           || MachineControllerState == MachineControllerState.Paused;
    
    /// <summary>
    /// Execute the StartDebug command
    /// </summary>
    public void StartDebug() => _mc!.StartDebug();

    /// <summary>
    /// Enable/disable the StartDebug command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStartDebug(object? parameter)
        => _mc != null && MachineControllerState != MachineControllerState.Running;
    
    /// <summary>
    /// Execute the StepInto command
    /// </summary>
    public void StepInto() => _mc!.StepInto();

    /// <summary>
    /// Enable/disable the StepInto command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStepInto(object? parameter)
        => _mc != null && MachineControllerState == MachineControllerState.Paused;
    
    /// <summary>
    /// Execute the StepOver command
    /// </summary>
    public void StepOver() => _mc!.StepOver();

    /// <summary>
    /// Enable/disable the StepOver command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStepOver(object? parameter)
        => _mc != null && MachineControllerState == MachineControllerState.Paused;
    
    /// <summary>
    /// Execute the StepOut command
    /// </summary>
    public void StepOut() => _mc!.StepOut();

    /// <summary>
    /// Enable/disable the StepOut command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStepOut(object? parameter)
        => _mc != null && MachineControllerState == MachineControllerState.Paused;
    
    /// <summary>
    /// Get or set the machine frames completed
    /// </summary>
    public int MachineFrames
    {
        get => _machineFrames;
        set => SetProperty(ref _machineFrames, value);
    }

    public DisplayViewModel? Display
    {
        get => _display;
        set => SetProperty(ref _display, value);
    }

    #endregion
    
    #region View Options related parts
    
    public ViewOptionsViewModel ViewOptions
    {
        get => _viewOptionsViewModel;
        set => SetProperty(ref _viewOptionsViewModel, value);
    }

    public void ToggleShowToolbar() => ViewOptions.ShowToolbar = !ViewOptions.ShowToolbar;

    public void ToggleShowStatusBar() => ViewOptions.ShowStatusBar = !ViewOptions.ShowStatusBar;

    #endregion
}
