# The `ZxSpectrum48Machine` Implementation

The `ZxSpectrum48Machine`, which class derives from `Z80MachineBase`, implements the behavior of a ZX Spectrum 48K computer. The implementation delegates the emulation of hardware components to a set of devices:
- `KeyboardDevice`: Implements the keyboard's behavior; allows setting and querying the state of individual keys.
- `ScreenDevice`: Implements the screen rendering of the ULA chip. This device works simultaneously with the Z80 CPU and displays the subsequent pixels as the CPU executes the instructions. *This device creates the ARGB (32-bit) bitmap of the screen to display, but it does not undertake the task of showing it on the UI.*
- `BeeperDevice`: Represents the one-bit beeper of ZX Spectrum 48, creating the samples that produce the emulated sound on the host machine. *Though this device creates the sound samples, it does not send them to a physical sound device.*
- `TapeDevice`: Emulates the flow of binary signal stream (EAR bit) coming from a tape (to allow loading data) and implements the tape signal (MIC bit) flow when saving data to the tape.
- `ZxSpectrum48FloatingBusDevice`: Emulates the behavior of the ZX Spectrum 48K's floating bus.

The `ZxSpectrum48Machine` class overrides and extends the behavior of its ancestor class, `Z80MachineBase`, at these points:

- Implementing the reset behavior
- Implementing the behavior of the memory
  - Separating the ROM and RAM partitions
  - Delaying memory access according to the contention with the ULA
- Implementing the behavior of output ports
  - Handling reading and writing from even ports
  - Delaying I/O access according to the contention with the ULA
  - Dispatching the responsibility of handling input and output bits to specific devices
  - Emulating some hardware-related behavior, such as reading the Bit 6 value according to the last writes of Bit 3/Bit 4(due to a capacitor in the hardware)
- Setting the screen width and height values
- Providing access to the screen's bitmap as rendered during the execution of a machine frame
- Handling the emulation of queued keystrokes
- Overriding the `OnInitNewFrame` method to allow the screen and beeper devices to respond to the start of a new machine frame.
- Overriding the `ShouldRaiseInterrupt` method to provide the INT signal behavior of the ZX Spectrum 48K
- Overriding the `AfterInstructionExecuted` method to let the beeper device render a sound sample and let the tape device detect passive/LOAD/SAVE mode changes
- Overriding the `OnTactIncremented` method to emulate that the ULA renders the display pixels simultaneously with the Z80 CPU

## Reset and Hard Reset

The constructor of `ZxSpectrum48Machine` sets up all properties of the machine that contribute to the behavior of ZX Spectrum 48K. Then, it instantiates the devices and executes the reset procedure.
The routine completes with loading the contents of the ROM from an embedded resource: 

```csharp
public ZxSpectrum48Machine()
{
    // --- Set up machine attributes
    BaseClockFrequency = 3_500_000;
    ClockMultiplier = 1;
    DelayedAddressBus = true;
        
    // --- Create and initialize devices
    KeyboardDevice = new KeyboardDevice(this);
    ScreenDevice = new ScreenDevice(this);
    BeeperDevice = new BeeperDevice(this);
    FloatingBusDevice = new ZxSpectrum48FloatingBusDevice(this);
    TapeDevice = new TapeDevice(this);
    Reset();

    // --- Initialize the machine's ROM
    UploadRomBytes(LoadRomFromResource(DefaultRomResource));
}
```

The `Reset` method takes care of resetting not only the Z80 CPU but also the devices attached to ZX Spectrum 48K. After devices are reset, the method carries out a few extra initialization steps:

- It sets the sampling rate of the beeper device.
- The machine enters into a "passive" state to handle the tape emulation properly and signs that the currently used emulated tape cassette is rewound.
- It sets the clock frequency multiplier and prepares the engine for the first machine frame.
- It clears the queue of emulated keystrokes.

```csharp
public override void Reset()
{
    // --- Reset the CPU
    base.Reset();

    // --- Reset and setup devices
    KeyboardDevice.Reset();
    ScreenDevice.Reset();
    BeeperDevice.Reset();
    BeeperDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
    FloatingBusDevice.Reset();
    TapeDevice.Reset();
        
    // --- Set default property values
    SetMachineProperty(MachinePropNames.TapeMode, TapeMode.Passive);
    SetMachineProperty(MachinePropNames.RewindRequested, null);

    // --- Unknown clock multiplier in the previous frame
    _oldClockMultiplier = -1;

    // --- Prepare for running a new machine loop
    ClockMultiplier = TargetClockMultiplier;
    ExecutionContext.LastTerminationReason = null;
    _lastRenderedFrameTact = -0;

    // --- Empty the queue of emulated keystrokes
    lock (_emulatedKeyStrokes) { _emulatedKeyStrokes.Clear(); }
}
```

The hard reset (turning off and then on the machine) executes the cold start for the CPU, fills up the RAM partition of the memory with zeros, and then resets the machine:

```csharp
public override void HardReset()
{
    base.HardReset();
    for (var i = 0x4000; i < _memory.Length; i++) _memory[i] = 0;
    Reset();
}
```

## Memory Handling

The [Memory and I/O Operations](../z80/z80-implementation.md#memory-and-io-operations) of the "Implementing the Z80 CPU" article describes the virtual methods the `Z80Cpu` defines for the derived virtual machines to customize memory and I/O handling (read and write operations, delays). The article also discusses [memory contention](../z80/z80-implementation.md#memory-contention).

The `ZxSpectrum48Machine` class overrides these methods to emulate the behavior of the machine's physical memory:

- `DoReadMemory`, `DelayMemoryRead`
- `DoWriteMemory`, `DelayMemoryWrite`
- `DelayAddressBusAccess`

The ZX Spectrum 48K has a flat linear memory that can be emulated with a consecutive array of 64K bytes. The `ZxSpectrum48Machine` represents the memory with a byte array:

```csharp
private readonly byte[] _memory = new byte[0x1_0000];
```

The first 16,384 bytes of this memory are ROM; the others represent the RAM partition.

### `DoReadMemory`

As its name suggest, this method reads the content of the memory with the specified address:

```csharp
public override byte DoReadMemory(ushort address)
    => _memory[address];
```

### `DelayMemoryRead`

```csharp
public override void DelayMemoryRead(ushort address)
{
    DelayAddressBusAccess(address);
    TactPlus3();
}
```

### `DoWriteMemory`

```csharp
public override void DoWriteMemory(ushort address, byte value)
{
    if ((address & 0xc000) != 0x0000)
    {
        _memory[address] = value;
    }
}
```

### `DelayMemoryWrite`

```csharp
public override void DelayMemoryWrite(ushort address)
{
    DelayAddressBusAccess(address);
    TactPlus3();
}
```

### `DelayAddressBusAccess`

```csharp
public override void DelayAddressBusAccess(ushort address)
{
    if ((address & 0xc000) == 0x4000)
    {
        // --- We read from contended memory
        var delay = _contentionValues[CurrentFrameTact / ClockMultiplier];
        TactPlusN(delay);
    }
}
```

## I/O Handling

_TBD_

## Screen Rendering

_TBD_

## Keystroke Emulation

_TBD_

## Machine Frame Execution Overrides

