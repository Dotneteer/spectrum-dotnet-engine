namespace SpectrumEngine.Emu;

/// <summary>
/// This class is intended to be a reusable base class for emulators using the Z80 CPU.
/// </summary>
public abstract class Z80MachineBase : 
    Z80Cpu,
    IZ80Machine, 
    IDebugSupport
{
    // --- Store the start tact of the next machine frame
    private ulong _nextFrameStartTact;

    // --- This flag indicates that the last machine frame has been completed.

    // --- Store machine-specific properties here
    private readonly Dictionary<string, object> _machineProps = new(StringComparer.InvariantCultureIgnoreCase);
    
    /// <summary>
    /// The folder where the ROM files are stored
    /// </summary>
    private const string ROM_RESOURCE_FOLDER = "Roms";

    /// <summary>
    /// This property stores the execution context where the emulated machine runs its execution loop.
    /// </summary>
    public ExecutionContext ExecutionContext { get; } = new();

    /// <summary>
    /// Gets the value of the machine property with the specified key
    /// </summary>
    /// <param name="key">Machine property key</param>
    /// <returns>Value of the property, if found; otherwise, null</returns>
    public object? GetMachineProperty(string key)
        => _machineProps.TryGetValue(key, out var value) ? value : null;

    /// <summary>
    /// Sets the value of the specified machine property
    /// </summary>
    /// <param name="key">Machine property key</param>
    /// <param name="value">Machine property value</param>
    public void SetMachineProperty(string key, object? value)
    {
        if (value == null)
        {
            if (!_machineProps.ContainsKey(key)) return;
            
            _machineProps.Remove(key);
            MachinePropertyChanged?.Invoke(this, (key, null));
        }
        else
        {
            if (_machineProps.TryGetValue(key, out var oldValue))
            {
                if (oldValue == value) return;
            }
            _machineProps[key] = value;
            MachinePropertyChanged?.Invoke(this, (key, value));
        }
    }

    /// <summary>
    /// This event fires when the state of a machine property changes.
    /// </summary>
    public event EventHandler<(string propertyName, object? newValue)>? MachinePropertyChanged;
    
    /// <summary>
    /// Get the duration of a machine frame in milliseconds.
    /// </summary>
    public double FrameTimeInMs { get; private set; }

    /// <summary>
    /// This property gets or sets the value of the target clock multiplier to set when the next machine frame starts.
    /// </summary>
    /// <remarks>
    /// By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock
    /// frequency multiplier to emulate a faster CPU.
    /// </remarks>
    public int TargetClockMultiplier { get; set; } = 1;

    /// <summary>
    /// This flag indicates that the last machine frame has been completed.
    /// </summary>
    private bool FrameCompleted { get; set; }

    /// <summary>
    /// Shows the number of frame tacts that overflow to the subsequent machine frame.
    /// </summary>
    private int FrameOverflow { get; set; }

    /// <summary>
    /// Set the number of tacts in a machine frame.
    /// </summary>
    /// <param name="tacts">Number of tacts in a machine frame</param>
    public override void SetTactsInFrame(int tacts)
    {
        base.SetTactsInFrame(tacts);
        FrameTimeInMs = tacts * 1000.0 / BaseClockFrequency;
    }

    /// <summary>
    /// This method provides a way to configure (or reconfigure) the emulated machine after changing the properties
    /// of its components.
    /// </summary>
    public virtual void Configure()
    {
        // --- Implement this method in derived classes
    }

    /// <summary>
    /// This method emulates resetting a machine with a hardware reset button.
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        FrameCompleted = true;
        FrameOverflow = 0;
    }

    /// <summary>
    /// Get the name of the default ROM's resource file within this assembly.
    /// </summary>
    protected abstract string DefaultRomResource { get; }

    /// <summary>
    /// Load the specified ROM from the current assembly resource.
    /// </summary>
    /// <param name="romName">Name of the ROM file to load</param>
    /// <param name="page">Optional ROM page for multi-rom machines</param>
    /// <returns>The byte array that represents the ROM contents</returns>
    /// <exception cref="InvalidOperationException">
    /// The ROM cannot be loaded from the named resource.
    /// </exception>
    protected static byte[] LoadRomFromResource(string romName, int page = -1)
    {
        var resourceName = page == -1 ? romName : $"{romName}-{page}";
        var currentAsm = typeof(Z80MachineBase).Assembly;
        resourceName = $"{currentAsm.GetName().Name}.{ROM_RESOURCE_FOLDER}.{romName}.{resourceName}.rom";
        var resMan = currentAsm.GetManifestResourceStream(resourceName);
        if (resMan == null)
        {
            throw new InvalidOperationException($"Input stream for the '{romName}' .rom file not found.");
        }
        using var stream = new StreamReader(resMan).BaseStream;
        stream.Seek(0, SeekOrigin.Begin);
        var bytes = new byte[stream.Length];
        // ReSharper disable once MustUseReturnValue
        stream.Read(bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// This member stores the last startup breakpoint to check. It allows setting a breakpoint to the first
    /// instruction of a program.
    /// </summary>
    public ushort? LastStartupBreakpoint { get; set; }

    /// <summary>
    /// Executes the machine loop using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    public virtual FrameTerminationMode ExecuteMachineFrame()
    {
        return ExecutionContext.FrameTerminationMode == FrameTerminationMode.Normal
            ? ExecuteMachineLoopWithNoDebug()
            : ExecuteMachineLoopWithDebug();
    }

    /// <summary>
    /// Width of the screen in native machine screen pixels
    /// </summary>
    public abstract int ScreenWidthInPixels { get; }

    /// <summary>
    /// Height of the screen in native machine screen pixels
    /// </summary>
    public abstract int ScreenHeightInPixels { get; }

    /// <summary>
    /// Gets the buffer that stores the rendered pixels
    /// </summary>
    public abstract uint[] GetPixelBuffer();

    /// <summary>
    /// Executes the machine loop using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    private FrameTerminationMode ExecuteMachineLoopWithNoDebug()
    {
        // --- Sign that the loop execution is in progress
        ExecutionContext.LastTerminationReason = null;

        // --- Execute the machine loop until the frame is completed or the loop is interrupted because of any other
        // --- completion reason, like reaching a breakpoint, etc.
        do
        {
            // --- Test if the machine frame has just been completed.
            if (FrameCompleted)
            {
                var currentFrameStart = Tacts - (ulong)FrameOverflow;

                // --- Update the CPU's clock multiplier, if the machine's has changed.
                var clockMultiplierChanged = false;
                if (AllowCpuClockChange() && ClockMultiplier != TargetClockMultiplier)
                {
                    // --- Use the current clock multiplier
                    ClockMultiplier = TargetClockMultiplier;
                    clockMultiplierChanged = true;
                }

                // --- Allow a machine to handle frame initialization
                OnInitNewFrame(clockMultiplierChanged);
                FrameCompleted = false;

                // --- Calculate the start tact of the next machine frame
                _nextFrameStartTact = currentFrameStart + (ulong)(TactsInFrame * ClockMultiplier);
            }

            // --- Set the interrupt signal, if required so
            if (ShouldRaiseInterrupt())
            {
                SignalFlags |= Z80Signals.Int;
            }
            else
            {
                SignalFlags &= ~Z80Signals.Int;
            }

            // --- Execute the next CPU instruction entirely 
            do
            {
                ExecuteCpuCycle();
            } while (Prefix != OpCodePrefix.None);

            // --- Allow the machine to do additional tasks after the completed CPU instruction
            AfterInstructionExecuted();

            FrameCompleted = Tacts >= _nextFrameStartTact;
        } while (!FrameCompleted);

        // --- Calculate the overflow, we need this value in the next frame
        FrameOverflow = (int)(Tacts - _nextFrameStartTact);

        // --- Done
        return (ExecutionContext.LastTerminationReason = FrameTerminationMode.Normal).Value;
    }

    /// <summary>
    /// Executes the machine loop using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    private FrameTerminationMode ExecuteMachineLoopWithDebug()
    {
        // --- Sign that the loop execution is in progress
        ExecutionContext.LastTerminationReason = null;

        // --- Check the startup breakpoint
        if (Regs.PC != LastStartupBreakpoint)
        {
            // --- Check startup breakpoint
            CheckBreakpoints();
            if (ExecutionContext.LastTerminationReason.HasValue)
            {
                // --- The code execution has stopped at the startup breakpoint.
                // --- Sign that fact so that the next time the code do not stop
                LastStartupBreakpoint = Regs.PC;
                return ExecutionContext.LastTerminationReason.Value;
            }
        }

        // --- Remove the startup breakpoint
        LastStartupBreakpoint = null;

        // --- Execute the machine loop until the frame is completed or the loop is interrupted because of any other
        // --- completion reason, like reaching a breakpoint, etc.
        do
        {
            // --- Test if the machine frame has just been completed.
            if (FrameCompleted)
            {
                var currentFrameStart = Tacts - (ulong)FrameOverflow;

                // --- Update the CPU's clock multiplier, if the machine's has changed.
                var clockMultiplierChanged = false;
                if (AllowCpuClockChange() && ClockMultiplier != TargetClockMultiplier)
                {
                    // --- Use the current clock multiplier
                    ClockMultiplier = TargetClockMultiplier;
                    clockMultiplierChanged = true;
                }

                // --- Allow a machine to handle frame initialization
                OnInitNewFrame(clockMultiplierChanged);
                FrameCompleted = false;

                // --- Calculate the start tact of the next machine frame
                _nextFrameStartTact = currentFrameStart + (ulong)(TactsInFrame * ClockMultiplier);
            }

            // --- Set the interrupt signal, if required so
            if (ShouldRaiseInterrupt())
            {
                SignalFlags |= Z80Signals.Int;
            }
            else
            {
                SignalFlags &= ~Z80Signals.Int;
            }

            // --- Execute the next CPU instruction entirely 
            do
            {
                ExecuteCpuCycle();
            } while (Prefix != OpCodePrefix.None);

            // --- Allow the machine to do additional tasks after the completed CPU instruction
            AfterInstructionExecuted();

            // --- Do the machine reached the termination point?
            if (TestTerminationPoint())
            {
                // --- The machine reached the termination point
                return (ExecutionContext.LastTerminationReason = FrameTerminationMode.UntilExecutionPoint).Value;
            }

            // --- Test if the execution reached a breakpoint
            CheckBreakpoints();
            if (ExecutionContext.LastTerminationReason.HasValue)
            {
                // --- The code execution has stopped at the startup breakpoint.
                // --- Sign that fact so that the next time the code do not stop
                LastStartupBreakpoint = Regs.PC;
                return ExecutionContext.LastTerminationReason.Value;
            }

            FrameCompleted = Tacts >= _nextFrameStartTact;
        } while (!FrameCompleted);

        // --- Calculate the overflow, we need this value in the next frame
        FrameOverflow = (int)(Tacts - _nextFrameStartTact);

        // --- Done
        return (ExecutionContext.LastTerminationReason = FrameTerminationMode.Normal).Value;
    }

    /// <summary>
    /// This method tests if any breakpoint is reached during the execution of the machine frame to suspend the loop.
    /// </summary>
    private void CheckBreakpoints()
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// This method tests if the CPU reached the specified termination point.
    /// </summary>
    /// <returns>
    /// True, if the execution has reached the termination point; otherwise, false.
    /// </returns>
    /// <remarks>
    /// By default, this method checks if the PC equals the execution context's TerminationPoint value. 
    /// </remarks>
    protected virtual bool TestTerminationPoint() 
        => ExecutionContext.FrameTerminationMode == FrameTerminationMode.UntilExecutionPoint && 
           Regs.PC == ExecutionContext.TerminationPoint;

    /// <summary>
    /// The machine's execution loop calls this method to check if it can change the clock multiplier.
    /// </summary>
    /// <returns>
    /// True, if the clock multiplier can be changed; otherwise, false.
    /// </returns>
    protected virtual bool AllowCpuClockChange()
    {
        return true;
    }

    /// <summary>
    /// The machine's execution loop calls this method when it is about to initialize a new frame.
    /// </summary>
    /// <param name="clockMultiplierChanged">
    /// Indicates if the clock multiplier has been changed since the execution of the previous frame.
    /// </param>
    protected virtual void OnInitNewFrame(bool clockMultiplierChanged)
    {
        // --- Override this method in derived classes.
    }

    /// <summary>
    /// Tests if the machine should raise a Z80 maskable interrupt
    /// </summary>
    /// <returns>
    /// True, if the INT signal should be active; otherwise, false.
    /// </returns>
    protected abstract bool ShouldRaiseInterrupt();

    /// <summary>
    /// The machine frame loop invokes this method after executing a CPU instruction.
    /// </summary>
    protected virtual void AfterInstructionExecuted()
    {
        // --- Override this method in derived classes.
    }
}
