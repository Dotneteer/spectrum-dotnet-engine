using System.Diagnostics;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements a machine controller that can operate an emulated machine invoking its execution loop.
/// </summary>
public class MachineController
{
    private CancellationTokenSource? _tokenSource;
    private Task? _machineTask;
    private MachineControllerState _machineState;
    private readonly FrameStats _frameStats = new FrameStats();

    /// <summary>
    /// Initializes the controller to manage the specified machine.
    /// </summary>
    /// <param name="machine">The machine to manage</param>
    /// <exception cref="ArgumentNullException">
    /// The machine to manage was null.
    /// </exception>
    public MachineController(IZ80Machine machine)
    {
        Machine = machine ?? throw new ArgumentNullException(nameof(machine));
        Context = machine.ExecutionContext;
        IsDebugging = false;
    }

    /// <summary>
    /// The machine the controller manages.
    /// </summary>
    public IZ80Machine Machine { get; }

    /// <summary>
    /// The execution context of the controlled machine
    /// </summary>
    private ExecutionContext Context { get; }

    /// <summary>
    /// Get the current state of the machine controller.
    /// </summary>
    public MachineControllerState State
    {
        get => _machineState;
        private set
        {
            if (_machineState == value) return;
            
            var oldState = _machineState;
            _machineState = value;
            StateChanged?.Invoke(this, (oldState, _machineState));
        }
    }

    /// <summary>
    /// Represents the frame statistics of the last running frame
    /// </summary>
    public FrameStats FrameStats => _frameStats;
    
    /// <summary>
    /// Indicates if the machine runs in debug mode
    /// </summary>
    public bool IsDebugging { get; private set; }

    /// <summary>
    /// This event fires when the state of the controller changes.
    /// </summary>
    public event EventHandler<(MachineControllerState OldState, MachineControllerState NewState)>? StateChanged;

    /// <summary>
    /// This event fires whenever an execution loop has been completed. The event parameter flag indicates if the
    /// frame has been completed entirely (normal termination mode)
    /// </summary>
    public event EventHandler<bool>? FrameCompleted;

    /// <summary>
    /// Start the machine in normal mode.
    /// </summary>
    public void Start()
    {
        IsDebugging = false;
        Run();
    }

    /// <summary>
    /// Start the machine in debug mode.
    /// </summary>
    public void StartDebug()
    {
        IsDebugging = true;
        Run(FrameTerminationMode.DebugEvent, DebugStepMode.StopAtBreakpoint);
    }
    
    /// <summary>
    /// Pause the running machine.
    /// </summary>
    public async Task Pause()
    {
        if (State != MachineControllerState.Running)
        {
            throw new InvalidOperationException("The machine is not running");
        }
        await FinishExecutionLoop(MachineControllerState.Pausing, MachineControllerState.Paused);
    }

    /// <summary>
    /// Stop the running or paused machine.
    /// </summary>
    public async Task Stop()
    {
        if (State != MachineControllerState.Running && State != MachineControllerState.Paused)
        {
            throw new InvalidOperationException("The machine is not running");
        }

        IsDebugging = false;
        await FinishExecutionLoop(MachineControllerState.Stopping, MachineControllerState.Stopped);
        _frameStats.FrameCount = 0;
        _frameStats.LastCpuFrameTimeInMs = 0.0;
        _frameStats.AvgFrameTimeInMs = 0.0;
        _frameStats.LastFrameTimeInMs = 0.0;
        _frameStats.AvgFrameTimeInMs = 0.0;
    }

    /// <summary>
    /// Stop and then start the machine.
    /// </summary>
    public async Task Restart()
    {
        await Stop();
        Machine.Reset();
        Start();
    }

    /// <summary>
    /// Starts the machine in step-into mode.
    /// </summary>
    public void StepInto()
    {
        IsDebugging = true;
        Run(FrameTerminationMode.DebugEvent, DebugStepMode.StepInto);
    }

    /// <summary>
    /// Starts the machine in step-over mode.
    /// </summary>
    public void StepOver()
    {
        IsDebugging = true;
        Run(FrameTerminationMode.DebugEvent, DebugStepMode.StepOver);
    }

    /// <summary>
    /// Starts the machine in step-out mode.
    /// </summary>
    public void StepOut()
    {
        IsDebugging = true;
        Run(FrameTerminationMode.DebugEvent, DebugStepMode.StepOut);
    }

    public async Task RunToTerminationPoint(int? terminationPartition, ushort? terminationPoint)
    {
        Run(FrameTerminationMode.UntilExecutionPoint, DebugStepMode.NoDebug, terminationPartition, terminationPoint);
        await CompleteExecutionLoop();
        if (!Context.Cancelled)
        {
            State = MachineControllerState.Paused;
        }
    }

    /// <summary>
    /// Run the machine loop until cancelled
    /// </summary>
    private void Run(
        FrameTerminationMode terminationMode = FrameTerminationMode.Normal,
        DebugStepMode debugStepMode = DebugStepMode.NoDebug,
        int? terminationPartition = null,
        ushort? terminationPoint = null)
    {
        if (State == MachineControllerState.Running)
        {
            throw new InvalidOperationException("The machine is already running");
        }
        if (State == MachineControllerState.None || State == MachineControllerState.Stopped)
        {
            // --- First start (after stop), reset the machine
            Machine.Reset();
        }
        Context.FrameTerminationMode = terminationMode;
        Context.DebugStepMode = debugStepMode;
        Context.TerminationPartition = terminationPartition;
        Context.TerminationPoint = terminationPoint;
        Context.Cancelled = false;
        
        State = MachineControllerState.Running;
        _machineTask = Task.Run(async () =>
        {
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            var loopStartInTicks = DateTime.Now.Ticks;
            var completedFrames = 0;
            var stopwatch = new Stopwatch();
            do
            {
                // --- Run the machine frame and measure execution time
                stopwatch.Restart();
                var termination = Machine.ExecuteMachineFrame();
                var cpuTime = stopwatch.ElapsedTicks;
                var frameCompleted = termination == FrameTerminationMode.Normal;
                FrameCompleted?.Invoke(this, frameCompleted);
                var frameTime = stopwatch.ElapsedTicks;
                if (frameCompleted)
                {
                    _frameStats.FrameCount++;
                } 
                _frameStats.LastCpuFrameTimeInMs = (double) cpuTime / Stopwatch.Frequency * 1000.0;
                _frameStats.AvgCpuFrameTimeInMs = _frameStats.FrameCount == 0
                    ? _frameStats.LastCpuFrameTimeInMs
                    : (_frameStats.AvgCpuFrameTimeInMs * (_frameStats.FrameCount - 1) +
                       _frameStats.LastCpuFrameTimeInMs) / _frameStats.FrameCount;
                _frameStats.LastFrameTimeInMs = (double) frameTime / Stopwatch.Frequency * 1000.0;
                _frameStats.AvgFrameTimeInMs = _frameStats.FrameCount == 0
                    ? _frameStats.LastFrameTimeInMs
                    : (_frameStats.AvgFrameTimeInMs * (_frameStats.FrameCount - 1) +
                       _frameStats.LastFrameTimeInMs) / _frameStats.FrameCount;
                
                if (termination != FrameTerminationMode.Normal || token.IsCancellationRequested)
                {
                    Context.Cancelled = token.IsCancellationRequested;
                    return;
                }

                // --- Calculate the time to wait before the next machine frame starts
                completedFrames++;
                var nexFrameTimeInTicks = loopStartInTicks + ((long)(Machine.FrameTimeInMs * completedFrames * 10_000));
                var currentTimeInTicks = DateTime.Now.Ticks;
                Logger.Log($"{completedFrames}: {nexFrameTimeInTicks}, {nexFrameTimeInTicks - loopStartInTicks}, {nexFrameTimeInTicks - currentTimeInTicks}, {TimeSpan.FromTicks(nexFrameTimeInTicks - currentTimeInTicks).Milliseconds}");
                var waitTimeInMs = TimeSpan.FromTicks(nexFrameTimeInTicks - currentTimeInTicks).Milliseconds;
                if (waitTimeInMs > 2)
                {
                    try
                    {
                        await Task.Delay(waitTimeInMs - 2, token);
                    }
                    catch
                    {
                        // --- We intentionally ignore the task cancellation
                    }
                }
            } while (true);
        });
    }

    /// <summary>
    /// Finishes running the current execution loop of the machine
    /// </summary>
    /// <param name="beforeState">Controller state before the finishing operation</param>
    /// <param name="afterState">Controller state after the finishing operation</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    /// The machine controller is in an invalid state
    /// </exception>
    private async Task FinishExecutionLoop(
        MachineControllerState beforeState,
        MachineControllerState afterState)
    {
        State = beforeState;
        _tokenSource?.Cancel();
        await CompleteExecutionLoop();
        State = afterState;
    }

    /// <summary>
    /// Completes the current execution loop of the machine
    /// </summary>
    /// <returns></returns>
    private async Task CompleteExecutionLoop()
    {
        if (_machineTask != null)
        {
            await _machineTask;
            _machineTask.Dispose();
            _machineTask = null;
        }
    }
}
