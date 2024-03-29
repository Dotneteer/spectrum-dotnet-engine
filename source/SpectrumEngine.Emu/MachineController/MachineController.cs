﻿using System.Diagnostics;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements a machine controller that can operate an emulated machine invoking its execution loop.
/// </summary>
public class MachineController
{
    private CancellationTokenSource? _tokenSource;
    private Task? _machineTask;
    private MachineControllerState _machineState;

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
    /// Gets or sets the object providing debug support
    /// </summary>
    public IDebugSupport? DebugSupport { get; set; }
    
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
        set
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
    public FrameStats FrameStats { get; } = new();

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
    public async Task Start()
    {
        IsDebugging = false;
        Run();
        await CompleteExecutionLoop();
    }

    /// <summary>
    /// Start the machine in debug mode.
    /// </summary>
    public async Task StartDebug()
    {
        IsDebugging = true;
        Run(FrameTerminationMode.DebugEvent, DebugStepMode.StopAtBreakpoint);
        await CompleteExecutionLoop();
        if (Context.LastTerminationReason == FrameTerminationMode.DebugEvent)
        {
            // --- We are about to pause because of a debug event
            await FinishExecutionLoop(MachineControllerState.Pausing, MachineControllerState.Paused);
        }
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

        // --- Stop the machine
        IsDebugging = false;
        await FinishExecutionLoop(MachineControllerState.Stopping, MachineControllerState.Stopped);
        Machine.OnStop();
        
        // --- Reset frame statistics
        FrameStats.FrameCount = 0;
        FrameStats.LastCpuFrameTimeInMs = 0.0;
        FrameStats.AvgFrameTimeInMs = 0.0;
        FrameStats.LastFrameTimeInMs = 0.0;
        FrameStats.AvgFrameTimeInMs = 0.0;
        
        // --- Reset the imminent breakpoint
        if (Context.DebugSupport != null)
        {
            Context.DebugSupport.ImminentBreakpoint = null;
        }
    }

    /// <summary>
    /// Stop and then start the machine.
    /// </summary>
    public async Task Restart()
    {
        await Stop();
        Machine.HardReset();
        await Start();
    }

    /// <summary>
    /// Starts the machine in step-into mode.
    /// </summary>
    public async Task StepInto()
    {
        IsDebugging = true;
        Run(FrameTerminationMode.DebugEvent, DebugStepMode.StepInto);
        await FinishExecutionLoop(MachineControllerState.Pausing, MachineControllerState.Paused);
    }

    /// <summary>
    /// Starts the machine in step-over mode.
    /// </summary>
    public async Task StepOver()
    {
        IsDebugging = true;
        Run(FrameTerminationMode.DebugEvent, DebugStepMode.StepOver);
        await FinishExecutionLoop(MachineControllerState.Pausing, MachineControllerState.Paused);
    }

    /// <summary>
    /// Starts the machine in step-out mode.
    /// </summary>
    public async Task StepOut()
    {
        IsDebugging = true;
        Run(FrameTerminationMode.DebugEvent, DebugStepMode.StepOut);
        await FinishExecutionLoop(MachineControllerState.Pausing, MachineControllerState.Paused);
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
        switch (State)
        {
            case MachineControllerState.Running:
                throw new InvalidOperationException("The machine is already running");
            case MachineControllerState.None or MachineControllerState.Stopped:
                // --- First start (after stop), reset the machine
                Machine.Reset();
                break;
        }

        // --- Initialize the context
        Context.FrameTerminationMode = terminationMode;
        Context.DebugStepMode = debugStepMode;
        Context.TerminationPartition = terminationPartition;
        Context.TerminationPoint = terminationPoint;
        Context.Canceled = false;
        Context.DebugSupport = DebugSupport;
        
        // --- Set up the state
        Machine.ContentionDelaySincePause = 0;

        // --- Now, run!
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
                    FrameStats.FrameCount++;
                } 
                FrameStats.LastCpuFrameTimeInMs = (double) cpuTime / Stopwatch.Frequency * 1000.0;
                FrameStats.AvgCpuFrameTimeInMs = FrameStats.FrameCount == 0
                    ? FrameStats.LastCpuFrameTimeInMs
                    : (FrameStats.AvgCpuFrameTimeInMs * (FrameStats.FrameCount - 1) +
                       FrameStats.LastCpuFrameTimeInMs) / FrameStats.FrameCount;
                FrameStats.LastFrameTimeInMs = (double) frameTime / Stopwatch.Frequency * 1000.0;
                FrameStats.AvgFrameTimeInMs = FrameStats.FrameCount == 0
                    ? FrameStats.LastFrameTimeInMs
                    : (FrameStats.AvgFrameTimeInMs * (FrameStats.FrameCount - 1) +
                       FrameStats.LastFrameTimeInMs) / FrameStats.FrameCount;
                
                if (termination != FrameTerminationMode.Normal || token.IsCancellationRequested)
                {
                    Context.Canceled = token.IsCancellationRequested;
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
