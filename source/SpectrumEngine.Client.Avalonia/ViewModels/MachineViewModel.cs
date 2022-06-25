using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Metadata;
using SpectrumEngine.Emu;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.ViewModels;

/// <summary>
/// The view model that represents the virtual machine's state
/// </summary>
public class MachineViewModel: ViewModelBase
{
    private readonly MainWindowViewModel _parentVm;
    private MachineController? _mc;
    private MachineControllerState _mstate;
    private int _machineFrames;
    private bool _allowFastLoad;
    private TapeMode _tapeMode;
    private int _clockMultiplier;
    private FrameStats _frameStats = new FrameStats(); 

    /// <summary>
    /// Bind this view model to its parent
    /// </summary>
    /// <param name="parent">The parent MainWindowViewModel instance</param>
    public MachineViewModel(MainWindowViewModel parent)
    {
        _parentVm = parent;
    }
    
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
            _mc.Machine.MachinePropertyChanged -= OnMachinePropertyChanged;
        }

        // --- Set up the new controller and handle the state changes
        _mc = controller;
        _machineFrames = 0;
        _mc.StateChanged += OnStateChanged;
        _mc.FrameCompleted += OnFrameCompleted;
        _mc.Machine.MachinePropertyChanged += OnMachinePropertyChanged;
        ClockMultiplier = _mc.Machine.TargetClockMultiplier;
        OnStateChanged(this, (MachineControllerState.None, MachineControllerState.None));

        // --- Update view model properties
        _parentVm.Display = new DisplayViewModel(_mc);

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

        void OnMachinePropertyChanged(object? sender, (string key, object? value) args)
        {
            if (args.key == MachinePropNames.TapeMode && args.value is TapeMode tapeMode)
            {
                TapeMode = tapeMode;
            }
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
            _mc?.Machine.SetMachineProperty(MachinePropNames.FastLoad, value);
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
    /// Execute the Start command
    /// </summary>
    public void Start()
    {
        _mc?.Start();
        RaiseCommandExecuted();
    }

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
    public Task Pause()
    {
        RaiseCommandExecuted();
        return _mc!.Pause();
    }

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
    public Task Stop()
    {
        RaiseCommandExecuted();
        return _mc!.Stop();
    }

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
    public Task Restart()
    {
        RaiseCommandExecuted();
        return _mc!.Restart();
    }

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
    public void StartDebug()
    {
        RaiseCommandExecuted();
        _mc!.StartDebug();
    }

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
    public void StepInto()
    {
        _mc!.StepInto();
        RaiseCommandExecuted();
    }

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
    public void StepOver()
    {
        _mc!.StepOver();
        RaiseCommandExecuted();
    }

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
    public void StepOut()
    {
        _mc!.StepOut();
        RaiseCommandExecuted();
    }

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
                _mc?.Machine.SetMachineProperty(MachinePropNames.TapeData, dataBlocks);
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
                    _mc?.Machine.SetMachineProperty(MachinePropNames.TapeData, dataBlocks);
                }
                else
                {
                    throw new InvalidOperationException("Could not read tape file");
                }
            }
            catch (Exception ex)
            {
                // --- Perhaps wrong file format
                _mc?.Machine.SetMachineProperty(MachinePropNames.TapeData, null);
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
        _mc?.Machine.SetMachineProperty(MachinePropNames.RewindRequested, true);
        RaiseCommandExecuted();
    }

    /// <summary>
    /// Sets the clock multiplier to the specified value
    /// </summary>
    /// <param name="arg"></param>
    public void SetClockMultiplier(object? arg)
    {
        if (arg is not string stringValue || !int.TryParse(stringValue, out var intValue)) return;
        
        _mc!.Machine.TargetClockMultiplier = intValue;
        ClockMultiplier = intValue;
    }
}