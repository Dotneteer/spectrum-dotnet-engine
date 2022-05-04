using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Wpf;

/// <summary>
/// This class represents the application's view model
/// </summary>
public class AppViewModel : ObservableObject
{
    // --- Store the machine controller instance
    private MachineController? _mc;

    // --- The current machine controller state
    private MachineControllerState _mstate;

    // --- Number of frames rendered
    private int _machineFrames;

    private DisplayViewModel? _displayViewModel;

    /// <summary>
    /// The singleton instance of the view model, we assign it the the DataContext of MainWindow.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Singleton")]
    public static AppViewModel Instance = new();

    /// <summary>
    /// Initializat the members of the view model
    /// </summary>
    public AppViewModel()
    {
        // --- Define the commands of the view model
        StartCommand = new RelayCommand(() => _mc!.Start(), () => _mstate != MachineControllerState.Running);
        PauseCommand = new AsyncRelayCommand(() => _mc!.Pause(), () => _mstate == MachineControllerState.Running);
        StopCommand = new AsyncRelayCommand(
            () => _mc!.Stop(), 
            () => _mstate == MachineControllerState.Running || _mstate == MachineControllerState.Paused);
        RestartCommand = new AsyncRelayCommand(
            () => _mc!.Restart(), 
            () => _mstate == MachineControllerState.Running || _mstate == MachineControllerState.Paused);
        StartDebugCommand = new RelayCommand(() => _mc!.StartDebug(), () => _mstate != MachineControllerState.Running);
        StepIntoCommand = new RelayCommand(() => _mc!.StepInto(), () => _mstate == MachineControllerState.Paused);
        StepOverCommand = new RelayCommand(() => _mc!.StepInto(), () => _mstate == MachineControllerState.Paused);
        StepOutCommand = new RelayCommand(() => _mc!.StepInto(), () => _mstate == MachineControllerState.Paused);
    }

    #region Machine controller related view model

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
        DisplayViewModel = new(_mc);

        void OnStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
        {
            // --- Refresh command states whenever the controller state changes
            MachineControllerState = e.NewState;
            StartCommand.NotifyCanExecuteChanged();
            PauseCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
            RestartCommand.NotifyCanExecuteChanged();
            StartDebugCommand.NotifyCanExecuteChanged();
            StepIntoCommand.NotifyCanExecuteChanged();
            StepOverCommand.NotifyCanExecuteChanged();
            StepOutCommand.NotifyCanExecuteChanged();
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

    // --- Machine controller commands
    public RelayCommand StartCommand { get; }
    public AsyncRelayCommand PauseCommand { get; }
    public AsyncRelayCommand StopCommand { get; }
    public AsyncRelayCommand RestartCommand { get; }
    public RelayCommand StartDebugCommand { get; }
    public RelayCommand StepIntoCommand { get; }
    public RelayCommand StepOverCommand { get; }
    public RelayCommand StepOutCommand { get; }

    /// <summary>
    /// Get or set the machine frames completed
    /// </summary>
    public int MachineFrames
    {
        get => _machineFrames;
        set => SetProperty(ref _machineFrames, value);
    }

    #endregion

    public DisplayViewModel? DisplayViewModel
    {
        get => _displayViewModel;
        set => SetProperty(ref _displayViewModel, value);
    }
}
