using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Metadata;
using SpectrumEngine.Client.Avalonia.Utility;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.ViewModels;

/// <summary>
/// The view model that represents the virtual machine's state
/// </summary>
public class MachineViewModel: ViewModelBase
{
    private MachineControllerState _mstate;
    private bool _useKeyboard48;
    private TapeMode _tapeMode;
    private bool _allowFastLoad;
    private int _clockMultiplier;
    private FrameStats _frameStats = new(); 

    /// <summary>
    /// Set the machine controller to use with this view model
    /// </summary>
    /// <param name="controller">The machine controller to use</param>
    public void SetMachineController(MachineController controller)
    {
        var oldController = Controller;
        
        // --- Unsubscribe from the events of the previous controller
        if (Controller != null)
        {
            Controller.StateChanged -= OnStateChanged;
            Controller.FrameCompleted -= OnFrameCompleted;
            Controller.Machine.MachinePropertyChanged -= OnMachinePropertyChanged;
        }

        // --- Set up the new controller and handle the state changes
        Controller = controller;
        Controller.StateChanged += OnStateChanged;
        Controller.FrameCompleted += OnFrameCompleted;
        Controller.Machine.MachinePropertyChanged += OnMachinePropertyChanged;
        ClockMultiplier = Controller.Machine.TargetClockMultiplier;
        RaisePropertyChanged(nameof(Controller));
        RaisePropertyChanged(nameof(Id));
        RaisePropertyChanged(nameof(DisplayName));
        
        // --- Use the appropriate keyboard
        var kbTypeProp = Controller.Machine.GetMachineProperty(MachinePropNames.KBTYPE_48);
        if (kbTypeProp is bool kbTypeValue)
        {
            UseKeyboard48 = kbTypeValue;
        }
        
        // --- Sign initial state change
        ControllerChanged?.Invoke(this, (oldController, Controller));
        OnStateChanged(this, (MachineControllerState.None, MachineControllerState.None));

        // --- Execute this handle whenever the controller's state changes
        void OnStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
        {
            // --- Refresh command states whenever the controller state changes
            MachineControllerState = e.NewState;
        }

        // --- Initiate this handler whenever a new frame has been completed
        void OnFrameCompleted(object? sender, bool completed)
        {
            if (!completed) return;
            var frameStats = Controller.FrameStats;
            FrameStats = new FrameStats
            {
                FrameCount = frameStats.FrameCount,
                LastCpuFrameTimeInMs = frameStats.LastCpuFrameTimeInMs,
                AvgCpuFrameTimeInMs = frameStats.AvgCpuFrameTimeInMs,
                LastFrameTimeInMs = frameStats.LastFrameTimeInMs,
                AvgFrameTimeInMs = frameStats.LastFrameTimeInMs
            };
        }

        // --- Respond to machine property changes
        void OnMachinePropertyChanged(object? sender, (string key, object? value) args)
        {
            if (args.key == MachinePropNames.TAPE_MODE && args.value is TapeMode tapeMode)
            {
                TapeMode = tapeMode;
                return;
            }
            if (args.key == MachinePropNames.KBTYPE_48 && args.value is bool kbType)
            {
                UseKeyboard48 = kbType;
            }
        }
    }

    public MachineController? Controller { get; private set; }

    /// <summary>
    /// The ID of the machine
    /// </summary>
    public string Id => Controller?.Machine.MachineId ?? "";

    /// <summary>
    /// The display name of the machine
    /// </summary>
    public string DisplayName => Controller?.Machine.DisplayName ?? "";
    
    public event EventHandler<(MachineController? OldController, MachineController? NewController)>? ControllerChanged; 

    public FrameStats FrameStats
    {
        get => _frameStats;
        set => SetProperty(ref _frameStats, value);
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
    /// Indicates that the current machine uses ZX Spectrum 128 keyboard
    /// </summary>
    public bool UseKeyboard48
    {
        get => _useKeyboard48;
        set => SetProperty(ref _useKeyboard48, value);
    }
    
    /// <summary>
    /// The current tape mode
    /// </summary>
    public TapeMode TapeMode
    {
        get => _tapeMode;
        private set => SetProperty(ref _tapeMode, value);
    }
    
    /// <summary>
    /// Indicates if fast load from tape is allowed
    /// </summary>
    public bool AllowFastLoad
    {
        get => _allowFastLoad;
        set
        {
            SetProperty(ref _allowFastLoad, value);
            Controller?.Machine.SetMachineProperty(MachinePropNames.FAST_LOAD, value);
        }
    }

    /// <summary>
    /// The current clock multiplier
    /// </summary>
    public int ClockMultiplier
    {
        get => _clockMultiplier;
        set => SetProperty(ref _clockMultiplier, value);
    }
    /// <summary>
    /// Raised when a machine command has been executed
    /// </summary>
    public EventHandler? CommandExecuted;

    private void RaiseCommandExecuted()
    {
        CommandExecuted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Selects the specified machine type
    /// </summary>
    /// <param name="machineType"></param>
    /// <returns></returns>
    public async Task SelectMachineType(object? machineType)
    {
        if (machineType is string machineId &&
            string.Compare(machineId, Id, StringComparison.OrdinalIgnoreCase) != 0)
        {
            if (MachineControllerState is MachineControllerState.Paused
                or MachineControllerState.Pausing
                or MachineControllerState.Running)
            {
                await Stop();
            }

            var newMachine = MachineFactory.CreateMachine(machineId);
            if (newMachine == null)
            {
                var msgBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error",
                    $"Cannot find or instantiate machine with ID '{machineId}'");
                await msgBox.Show();
                return;
            }
            
            // --- Set up machine properties
            newMachine.SetMachineProperty(MachinePropNames.FAST_LOAD, AllowFastLoad);
        }
    }

    /// <summary>
    /// Enable/disable the Start command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanSelectMachineType(object? parameter)
        => parameter is "sp48" or "sp128"; 
    
    /// <summary>
    /// Execute the Start command
    /// </summary>
    public Task Start()
    {
        RaiseCommandExecuted();
        return Controller!.Start();
    }

    /// <summary>
    /// Enable/disable the Start command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStart(object? parameter)
        => Controller != null && MachineControllerState != MachineControllerState.Running; 
    
    /// <summary>
    /// Execute the Pause command
    /// </summary>
    public Task Pause()
    {
        RaiseCommandExecuted();
        return Controller!.Pause();
    }

    /// <summary>
    /// Enable/disable the Pause command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanPause(object? parameter)
        => Controller != null && MachineControllerState == MachineControllerState.Running; 
    
    /// <summary>
    /// Execute the Stop command
    /// </summary>
    public Task Stop()
    {
        RaiseCommandExecuted();
        return Controller!.Stop();
    }

    /// <summary>
    /// Enable/disable the Stop command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStop(object? parameter)
        => Controller != null && MachineControllerState == MachineControllerState.Running 
           || MachineControllerState == MachineControllerState.Paused; 
    
    /// <summary>
    /// Execute the Restart command
    /// </summary>
    public Task Restart()
    {
        RaiseCommandExecuted();
        return Controller!.Restart();
    }

    /// <summary>
    /// Enable/disable the Restart command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanRestart(object? parameter)
        => Controller != null && MachineControllerState == MachineControllerState.Running 
           || MachineControllerState == MachineControllerState.Paused;
    
    /// <summary>
    /// Execute the StartDebug command
    /// </summary>
    public Task StartDebug()
    {
        RaiseCommandExecuted();
        return Controller!.StartDebug();
    }

    /// <summary>
    /// Enable/disable the StartDebug command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStartDebug(object? parameter)
        => Controller != null && MachineControllerState != MachineControllerState.Running;
    
    /// <summary>
    /// Execute the StepInto command
    /// </summary>
    public void StepInto()
    {
        Controller?.StepInto();
        RaiseCommandExecuted();
    }

    /// <summary>
    /// Enable/disable the StepInto command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStepInto(object? parameter)
        => Controller != null && MachineControllerState == MachineControllerState.Paused;
    
    /// <summary>
    /// Execute the StepOver command
    /// </summary>
    public void StepOver()
    {
        Controller?.StepOver();
        RaiseCommandExecuted();
    }

    /// <summary>
    /// Enable/disable the StepOver command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStepOver(object? parameter)
        => Controller != null && MachineControllerState == MachineControllerState.Paused;
    
    /// <summary>
    /// Execute the StepOut command
    /// </summary>
    public void StepOut()
    {
        Controller?.StepOut();
        RaiseCommandExecuted();
    }

    /// <summary>
    /// Enable/disable the StepOut command
    /// </summary>
    /// <param name="parameter">Unused</param>
    /// <returns>Is the command enabled?</returns>
    [DependsOn(nameof(MachineControllerState))]
    private bool CanStepOut(object? parameter)
        => Controller != null && MachineControllerState == MachineControllerState.Paused;
    
    public void ToggleFastLoad() => AllowFastLoad = !AllowFastLoad;

    public async Task SetTapeFile()
    {
        if (App.AppWindow == null) return;

        // --- Allow the user to select a tape file
        var dlg = new OpenFileDialog();
        dlg.Filters!.Add(new FileDialogFilter() { Name = "TAP Files", Extensions = { "tap" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "TZX Files", Extensions = { "tzx" } });
        
        // --- BUG: Avalonia 0.10.15 does not allow "*" filter an MacOSX
        dlg.Filters.Add(new FileDialogFilter() { Name = "All Files", Extensions = { "*" } });
        dlg.AllowMultiple = true;
        var result = await dlg.ShowAsync(App.AppWindow);

        // --- Check for selected tape file
        if (result != null)
        {
            // --- Read tape data
            var reader = new BinaryReader(File.OpenRead(result[0]));
            
            // --- Try as TZX
            var tzxReader = new TzxReader(reader);
            var readerFound = false;
            try
            {
                readerFound = tzxReader.ReadContent();
            }
            catch (Exception)
            {
                // --- This exception is intentionally ingnored
            }

            if (readerFound)
            {
                // --- This is a .TZX format
                var dataBlocks = tzxReader.DataBlocks.Select(b => b.GetDataBlock()).Where(b => b != null).ToList();
                Controller?.Machine.SetMachineProperty(MachinePropNames.TAPE_DATA, dataBlocks);
                return;
            }

            // --- Let's assume .TAP tap format
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var tapReader = new TapReader(reader);
            try
            {
                readerFound = tapReader.ReadContent();
                if (readerFound)
                {
                    // --- This is a .TAP format
                    var dataBlocks = tapReader.DataBlocks.ToList();
                    Controller?.Machine.SetMachineProperty(MachinePropNames.TAPE_DATA, dataBlocks);
                }
                else
                {
                    throw new InvalidOperationException("Could not read tape file");
                }
            }
            catch (Exception ex)
            {
                // --- Perhaps wrong file format
                Controller?.Machine.SetMachineProperty(MachinePropNames.TAPE_DATA, null);
                var msgBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error",
                    $"Cannot parse the contents of the specified file as a valid TZX or TAP file ({ex.Message})");
                await msgBox.Show();
            }
            RaiseCommandExecuted();
        }
    }

    /// <summary>
    /// Send a request to the machine to rewind the tape
    /// </summary>
    public void Rewind()
    {
        Controller?.Machine.SetMachineProperty(MachinePropNames.REWIND_REQUESTED, true);
        RaiseCommandExecuted();
    }

    /// <summary>
    /// Sets the clock multiplier to the specified value
    /// </summary>
    /// <param name="arg"></param>
    public void SetClockMultiplier(object? arg)
    {
        if (arg is not string stringValue || !int.TryParse(stringValue, out var intValue)) return;
        
        Controller!.Machine.TargetClockMultiplier = intValue;
        ClockMultiplier = intValue;
    }
}