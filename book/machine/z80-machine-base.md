# The `Z80MachineBase` Implementation

Though now the project implements only the ZX Spectrum 48K, in the future, it may provide emulation for other ZX Spectrum machines, like ZX Spectrum 128K, and beyond.

Each concrete machine derives directly or indirectly from the `Z80MachineBase` class, the common base class for all computer emulators using a Z80 CPU as their main component.

> *Note*: One of the essential concepts is the *machine frame*. You can read more details about it [here](../z80/z80-implementation.md#machine-frames). `Z80MachineBase` implements the execution loop that continuously runs a machine frame during the emulation.  

 `Z80MachineBase` implements the `IZ80Machine` interface:

## The `IZ80Machine` Interface

```csharp
public interface IZ80Machine : IZ80Cpu
{
    void Configure();
    
    double FrameTimeInMs { get; }
    int TargetClockMultiplier { get; set; }

    int ScreenWidthInPixels { get; }
    int ScreenHeightInPixels { get; }
    uint[] GetPixelBuffer();
    
    object? GetMachineProperty(string key);
    void SetMachineProperty(string key, object? value);
    event EventHandler<(string propertyName, object? newValue)>? MachinePropertyChanged;

    void SetKeyStatus(SpectrumKeyCode key, bool isDown);
    void QueueKeyPress(int startFrame, int frames, SpectrumKeyCode primary, SpectrumKeyCode? secondary);
    void EmulateKeystroke();

    ExecutionContext ExecutionContext { get; }
    FrameTerminationMode ExecuteMachineFrame();
}
```

- The `Configure` method provides a way to configure (or reconfigure) the emulated machine after changing the properties of its components.
- `FrameTimeInMs` returns the duration of a machine frame in milliseconds.
- By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock frequency multiplier to emulate a faster CPU. The `TargetClockMultiplier` property shows this value. You can set it any time; however, the machine starts using it only at the beginning of the subsequent machine frame.
- `ScreenWidthInPixels` defines the screen's width, while `ScreenHeightInPixels` defines the screen's height. Both properties use native machine pixels as their units.
- When it is time to render the machine's screen, the `GetPixelBuffer` method returns an array of 32-bit pixels that define the ARGB pixel values starting from the top-left corner of the screen. The pixels are read from left to right, and then the rows from top to bottom.
- The `GetMachineProperty` and `SetMachineProperty` methods allow handling named properties that a particular machine may use for its operation. These properties have a string key and a value that can be any object type except `null`. The class raises a `MachinePropertyChanged` event whenever a particular property changes its previous value.
- `SetKeyStatus` allows setting the state of a particular key in the machine's keyboard (pressed or released).
- When the machine runs, there you can emulate keystrokes. The `QueueKeyPress` method allows you to queue a key to be emulated in the future.
- The `EmulateKeystroke` method is essential to the machine frame execution. Its role is to emulate a queued keystroke.
- While executing a machine frame, the `ExecutionContext` property describes the context the currently running frame uses.
- The `ExecutionMachineFrame` method executes the next machine frame according to the current execution context.

## The CPU Clock Multiplier

Most retro-computers work with a fixed clock frequency, while others may change it. This emulator allows a clock multiplier value to increase the base clock frequency with an integer multiplier. This feature provides an excellent effect; you can test how the emulated computer could work with an increased frequency. Because of hardware limitations, you could not increase the clock frequency in most computers; however, the software emulation makes it possible.

The `TargetClockMultiplier` value handles this frequency change. It is one by default, but you can change it to other values (between 1 and 32).

Setting `TargetClockMultiplier` does not change the CPU's clock frequency immediately. It uses the previous frequency while the current machine frame completes; however it starts the next frame with the value set in `TargetClockMultiplier`.

> *Note*: Changing the clock frequency value may hurt the logic of apps and games that assume the standard CPU frequency (because of timing), and they may behave unexpectedly or even crash.

## Machine Properties

Each machine may have some individual trait that other machines do not have (or treat entirely differently). The concept of machine property is to store and manage these machine-specific traits.

Such a property is, for example, using tape. Though not only does the ZX Spectrum family of retro-computers have tape, handling them may be very different for another computer.
The `IZ80Machine` interface (and its core implementation, the `Z80MachineBase` class) provides two methods, `GetMachineProperty`, and `SetMachineProperty`, and an event, `MachinePropertyChanged`, to handle them. A particular machine can handle this event to respond to changes in a property's value. For example, the ZX Spectrum 48K implementation uses the "TapeData" property to manage the stream of bytes that the machine considers as the contents of the current tape.

## The Frame Execution Context

The machine frame execution must be aware of the current context. For example, if the user runs the machine in debug mode, execution must pause at breakpoints.

The machine's execution context stores the state the engine should consider when running the machine frame. The `ExecutionContext` class defines that context:

```csharp
public sealed class ExecutionContext
{
    public FrameTerminationMode FrameTerminationMode { get; set; }
    public DebugStepMode DebugStepMode { get; set; }
    public int? TerminationPartition { get; set; }
    public ushort? TerminationPoint { get; set; }
    public FrameTerminationMode? LastTerminationReason { get; set; }
    public bool Cancelled { get; set; }
}
```

- The `FrameTerminationMode` property tells the engine when it should terminate the currently running machine frame. This property accepts the following values:
  - `Normal`: the frame terminates when the current frame completes.
  - `DebugEvent`: the execution completes when a debugger event occurs (e.g., stopping at a breakpoint).
  - `UntilExecutionPoint`: The execution completes when the current PC address (and an optional memory partition) reaches a specified termination point.
- `DebugStepMode` defines how the machine execution loop should handle the debug mode. This property uses these values:
  - `NoDebug`: The debug mode is turned off, and the machine should ignore breakpoints.
  - `StopAtBreakpoint`: The execution loop should terminate as soon as it reaches an active breakpoint.
  - `StepInto`: The execution loop should stop after completing the subsequent CPU instruction.
  - `StepOver`: The execution loop should stop after the PC reaches the address of subsequent CPU instruction. If the current instruction is a subroutine call, the execution stops when the subroutine returns. If the instruction is a block instruction, the execution stops when the block completes. Should the instruction be a HALT, the loop terminates as the CPU gets out of the halted mode.
  - `StepOut`: The execution loop stops after the first RET instruction (conditional or unconditional) when it returns to its caller.
- The `TerminationPartition` and `TerminationPoint` properties define an execution point to pause the code execution when `FrameTerminationMode` is `UntilExecutionPoint`. In this mode, `TerminationPoint` is the value of the PC register to pause; `TerminationPartition` is the index of the currently selected ROM. *These properties are reserved for future extensions.*
- `LastTerminationReason` holds a value showing how the machine frame execution terminated last time. This property can hold the same values as `FrameTerminationMode`.
- The `Canceled` property indicates whether the user has canceled the execution of the last machine frame.

The `ExecutionContext` property of a Z80MachineBase-derived instance can be set before the engine calls the `ExecuteMachineFrame` method. After the method returns, the `LastTerminationReason` and the `Canceled` properties are updated to show how the machine frame has terminated.

## The Implementation Details of the Machine Frame Execution

The most noteworthy method of `Z80MachineBase` is the `ExecuteMachineFrame` method, which is responsible for managing a machine frame.

This method uses two implementations that run in normal and debug modes, respectively:

```csharp
public virtual FrameTerminationMode ExecuteMachineFrame()
{
    return ExecutionContext.FrameTerminationMode == FrameTerminationMode.Normal
        ? ExecuteMachineLoopWithNoDebug()
        : ExecuteMachineLoopWithDebug();
}
```

The reason for this duplication is that the debug mode execution may be a bit slower (due to the continuous check of pause conditions, such as reaching a predefined breakpoint). Let's see how the `ExecuteMachineLoopWithDebug` method works, as it helps understand what activities a machine frame is built from.

The source code of the method contains helpful comments:

```csharp
private FrameTerminationMode ExecuteMachineLoopWithDebug()
{
    // --- Section #1: Sign that the loop execution is in progress
    ExecutionContext.LastTerminationReason = null;

    // --- Section #2: Check the startup breakpoint
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

    // --- Section #3: Execute the machine loop until the frame is completed or the loop is interrupted because of
    // --- any other completion reason, like reaching a breakpoint, etc.
    do
    {
        // --- Section #3A: Test if the machine frame has just been completed.
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

        // --- Section #3B: Set the interrupt signal, if required so
        if (ShouldRaiseInterrupt())
        {
            SignalFlags |= Z80Signals.Int;
        }
        else
        {
            SignalFlags &= ~Z80Signals.Int;
        }

        // --- Section #3C: Execute the next CPU instruction entirely 
        do
        {
            ExecuteCpuCycle();
        } while (Prefix != OpCodePrefix.None);

        // --- Allow the machine to do additional tasks after the completed CPU instruction
        AfterInstructionExecuted();

        // --- Section #3D: Do the machine reached the termination point?
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

        // --- Section #3E: Check for machine frame completion
        FrameCompleted = Tacts >= _nextFrameStartTact;
    } while (!FrameCompleted);

    // --- Section #4: Calculate the overflow, we need this value in the next frame
    FrameOverflow = (int)(Tacts - _nextFrameStartTact);

    // --- Done
    return (ExecutionContext.LastTerminationReason = FrameTerminationMode.Normal).Value;
}
```

Here are a few additional comments:

- **Section #2** is responsible for allowing a startup breakpoint. This feature is helpful when you want to set a breakpoint at the startup address of the executed code. For example, this feature can set a breakpoint at address 0 when you debug the ZX Spectrum 48 ROM.
- **Section #3** is the actual machine frame. It continuously executes CPU instructions and lets the other hardware components (such as the ULA) do their tasks until the end of the frame is completed.
- **Section #3A** carries out the chores of starting a new frame. As the machine frame always executes complete Z80 instructions, a new frame may start with an overflow (the clock cycles of the CPU instruction that did not fit into the latest machine frame). If the user has changed it, this code sets a new CPU clock multiplier. A particular machine type can override the `OnInitNewFrame` method to handle the frame initialization with other hardware components.
- **Section #3B** signs the CPU if a maskable interrupt is requested. The responsibility is delegated to the virtual `ShouldRaiseInterrupt` method.
- **Section #3C** executes the subsequent CPU instruction entirely. At the end of the operation, it calls the virtual `AfterInstructionExecuted` method, which activates other hardware elements (such as the ULA).
- **Section #3D** tests if the machine frame should be terminated because of reaching a termination mode or a breakpoint.
- **Section #3E** examines if the current CPU tact has already reached the start of the next frame. If so, the frame is completed.
- **Section #4** calculates the frame overflow and signs the normal termination of the machine frame.