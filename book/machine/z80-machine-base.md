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
    object? GetMachineProperty(string key);
    void SetMachineProperty(string key, object? value);
    event EventHandler<(string propertyName, object? newValue)>? MachinePropertyChanged;
    uint[] GetPixelBuffer();
    ExecutionContext ExecutionContext { get; }
    FrameTerminationMode ExecuteMachineFrame();
}
```

- The `Configure` method provides a way to configure (or reconfigure) the emulated machine after changing the properties of its components.
`FrameTimeInMs` returns the duration of a machine frame in milliseconds.
- By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock frequency multiplier to emulate a faster CPU. The `TargetClockMultiplier` property shows this value. You can set it any time; however, the machine starts using it only at the beginning of the subsequent machine frame.
- `ScreenWidthInPixels` defines the screen's width, while `ScreenHeightInPixels` defines the screen's height. Both properties use native machine pixels as their units.
- The `GetMachineProperty` and `SetMachineProperty` methods allow handling named properties that a particular machine may use for its operation. These properties have a string key and a value that can be any object type except `null`. The class raises a `MachinePropertyChanged` event whenever a particular property changes its previous value.
- When it is time to render the machine's screen, the `GetPixelBuffer` method returns an array of 32-bit pixels that define the ARGB pixel values starting from the top-left corner of the screen. The pixels are read from left to right, and then the rows from top to bottom.
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

*TBD*

## The Implementation Details of the Machine Frame Execution

*TBD*

