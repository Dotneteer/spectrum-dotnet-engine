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

As its name suggests, this method reads the content of the memory with the specified address:

```csharp
public override byte DoReadMemory(ushort address)
    => _memory[address];
```

### `DelayMemoryRead`

The total memory read delay in ZX Spectrum 48K comprises two factors. First, the Z80 CPU adds a 3-T-states delay. Second, if the memory address is within a contended partition (between $4000 and $7fff), the ULA lets the CPU wait while it reads the contents of the screen memory.
This delay depends on the current screen rendering tact within the machine frame.

> *Note*: You can learn more details about the ZX Spectrum 48K memory contention [here](https://worldofspectrum.org/faq/reference/48kreference.htm) in the "Contended Memory" section.

```csharp
public override void DelayMemoryRead(ushort address)
{
    DelayAddressBusAccess(address);
    TactPlus3();
}
```
### `DoWriteMemory`

The write operation does not allow modifying the contents of the $0000-$3fff memory range, as it represents ROM:

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

The same delay applies to writing the memory as to reading it:

```csharp
public override void DelayMemoryWrite(ushort address)
{
    DelayAddressBusAccess(address);
    TactPlus3();
}
```

### `DelayAddressBusAccess`

The memory range of $4000-$7fff is a subject of contention between the Z80 CPU and the ULA, where the ULA has priority over the CPU. So, while the ULA reads screen information from this memory range, the CPU must wait. The `_contentionValues` array stores the number of T-States to wait while the machine is in a particular tact of the machine frame:

```csharp
private byte[] _contentionValues = Array.Empty<byte>();
```

The screen device is the one that sets up the contention delay values when it initializes itself (these values depend on the screen configuration settings). While the screen initializes, it invokes the `AllocateContentionsValues` and `SetContentionValue` methods of `ZxSpectrum48Machine` to set up the delay values for each screen rendering tact:

```csharp
// --- Allocate the contention value array 
public void AllocateContentionValues(int tactsInFrame)
{
    _contentionValues = new byte[tactsInFrame];
}

// --- Set a particular item of the contention value array
public void SetContentionValue(int tact, byte value)
{
    _contentionValues[tact] = value;
}
```

The `DelayAddressBusAccess` method takes care that the CPU waits for the ULA to release the access to the contended memory:

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

> *Note*: The method takes care of the clock frequency differences between the CPU and the ULA when we use clock frequency multiplication.

## I/O Handling

Similarly to memory, I/O operations are also delayed when the ULA reads or writes a particular port. The Z80 CPU adds a 4 T-states delay in the normal operation mode. However, when the ULA uses the address bus to access I/O, the ULA lets the CPU wait while completing the I/O operation.

### Contention Delay

This delay is an outcome of two effects:
Effect #1: If the lowest bit of the I/O port is zero, the ULA needs to read or write the particular I/O port, and it causes a delay if the ULA is currently busy reading screen information from the memory.
Effect #2: The address put on the bus causes delays just as it happens with memory.
The combination of these two effects provides four possible delay schemes implemented in the `DelayContendedIo` method:

```csharp
private void DelayContendedIo(ushort address)
{
    var lowbit = (address & 0x0001) != 0;

    // --- Check for contended range
    if ((address & 0xc000) == 0x4000)
    {
        if (lowbit)
        {
            // --- Low bit set, C:1, C:1, C:1, C:1
            ApplyContentionDelay();
            TactPlus1();
            ApplyContentionDelay();
            TactPlus1();
            ApplyContentionDelay();
            TactPlus1();
            ApplyContentionDelay();
            TactPlus1();
        }
        else
        {
            // --- Low bit reset, C:1, C:3
            ApplyContentionDelay();
            TactPlus1();
            ApplyContentionDelay();
            TactPlus3();
        }
    }
    else
    {
        if (lowbit)
        {
            // --- Low bit set, N:4
            TactPlus4();
        }
        else
        {
            // --- Low bit reset, C:1, C:3
            TactPlus1();
            ApplyContentionDelay();
            TactPlus3();
        }
    }

    // --- Apply I/O contention
    void ApplyContentionDelay()
    {
        var delay = GetContentionValue(CurrentFrameTact / ClockMultiplier);
        TactPlusN(delay);
    }
}
```

> *Note*: You can learn more details about the ZX Spectrum 48K I/O contention [here](https://worldofspectrum.org/faq/reference/48kreference.htm) in the "Contended I/O" section.

The `DelayPortRead` and `DelayPortWrite` methods call `DelayContendedIo`:

```csharp
public override void DelayPortRead(ushort address)
    => DelayContendedIo(address);
public override void DelayPortWrite(ushort address)
    => DelayContendedIo(address);    
```

### `DoReadPort`

The `DoReadPort` method delegates the responsibility or retrieving a value read from a particular I/O port to the `ReadPort0Xfe` method, provided the port's lowest bit is reset (known port). Otherwise, if the lowest bit is set (unhandled port), it provides to floating bus device to retrieve a value. (You can learn more about the floating bus [here](../hw-overview.md#the-floating-bus).)

```csharp
public override byte DoReadPort(ushort address)
{
    return (address & 0x0001) == 0 
        ? ReadPort0Xfe(address)
        : FloatingBusDevice.ReadFloatingPort();
}
```

The `ReadPort0Xfe` method collects the bits of the result from the affected devices: the keyboard and the tape signal (EAR bit).

However, in the hardware, the ULA uses the same pin for all of the MIC socket (tape signal output), EAR socket (tape signal input), and the internal speaker (beeper output). Moreover, the EAR and MIC sockets are connected only by resistors, so activating one activates the other.

This hardware implementation results in a strange effect when reading bit 6 (EAR bit input): the value depends on the Bit 3 and Bit 4 values written to the output port and the time elapsed since the last write (as the charging time of a capacitor is involved). ULA Issue 2 and 3 handle this differently, making the scenario more complex.

The `ReadPort0Xfe` method handles these chores this way:

```csharp
private byte ReadPort0Xfe(ushort address)
{
    var portValue = KeyboardDevice.GetKeyLineStatus(address);

    // --- Check for LOAD mode
    if (TapeDevice.TapeMode == TapeMode.Load)
    {
        var earBit = TapeDevice.GetTapeEarBit();
        BeeperDevice.SetEarBit(earBit);
        portValue = (byte)((portValue & 0xbf) | (earBit ? 0x40 : 0));
    }
    else
    {
        // --- Handle analog EAR bit
        var bit4Sensed = _portBit4LastValue;
        if (!bit4Sensed)
        {
            // --- Changed later to 1 from 0 than to 0 from 1?
            var chargeTime = _portBit4ChangedFrom1Tacts - _portBit4ChangedFrom0Tacts;
            if (chargeTime > 0)
            {
                // --- Yes, calculate charge time
                chargeTime = chargeTime > 700 ? 2800 : 4 * chargeTime;

                // --- Calculate time ellapsed since last change from 1 to 0
                bit4Sensed = Tacts - _portBit4ChangedFrom1Tacts < chargeTime;
            }
        }

        // --- Calculate bit 6 value
        var bit6Value = _portBit3LastValue
            ? 0x40
            : bit4Sensed
                ? 0x40
                : 0x00;

        // --- Check for ULA 3
        if (UlaIssue == 3 && _portBit3LastValue && !bit4Sensed)
        {
            bit6Value = 0x00;
        }

        // --- Merge bit 6 with port value
        portValue = (byte)((portValue & 0xbf) | bit6Value);
    }
    return portValue;
}
```

### `DoWritePort`

```csharp
public override void DoWritePort(ushort address, byte value)
{
    if ((address & 0x0001) == 0)
    {
        WritePort0xFE(value);
    }
}
```

## Screen Rendering

_TBD_

## Keystroke Emulation

_TBD_

## Machine Frame Execution Overrides

