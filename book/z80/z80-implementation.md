# Implementing the Z80 CPU

There are limitless ways to implement the software emulation of the Z80 CPU. When you select an approach, you have several aspects you need to consider. When I decided to implement it for the sake of integrating it with a ZX Spectrum emulator, I followed this approach:

- **Low-fidelity emulation of the CPU's output signals**. The current emulator does not implement the `M1`, `MREQ`, `IORQ`, `WR`, `RD`, `REFSH`, and `BUSAK` signals. Because ZX Spectrum uses the ULA chip to manage peripheral devices, we do not need to emulate these low-level signals.
- **High-fidelity emulation of the CPU's input signals**. The software emulates all other input signals except for the `WAIT`, and `BUSRQ` signals. The contention between the Z80 CPU and the ULA chip to access the address and data bus does not need the low-level signals; there's an easier way to implement it accurately.
- **High-fidelity emulation of timing**. We need very accurate timing to run ZX Spectrum games, as those leverage the stringent timing and cooperation between the CPU and the ULA, especially when rendering the screen and generating sound through the one-bit beeper.
- **High-fidelity emulation of undocumented Z80 instructions**. The emulator handles all (officially) undocumented instructions, and those handle the third and fifth bits of Register F as well as the internal `WZ` (`MEMPTR`) register accurately.
- **Focus on performance**. I have chosen an implementation model that prefers performance against the conciseness of the source code to write.

In this article, you find significant details that allow you to follow and understand the source code that implements the Z80 CPU.

## Z80 CPU Implementation Source Code Files

All the Z80 CPU-related source files are within the `Z80` folder of the emulator project. The implementation of the CPU is encapsulated in a single class, `Z80Cpu`; however, it is split into several partitions:

- `Z80Cpu.cs`: Defines a couple of nested types the CPU emulation utilizes in its implementation.
- `Z80Cpu-AluHelper.cs`: Contains several helper fields and methods to support the CPU's ALU operations.
- `Z80Cpu-BitInstructions.cs`: This file contains the code for processing bit manipulation instructions (with `$CB` prefix).
- `Z80Cpu-Clock.cs`: Declares methods that handle the clock and CPU timing.
- `Z80Cpu-Contention.cs`: This partition defines members that handle memory contention.
- `Z80Cpu-Dependencies.cs`: This contains the definition of dependencies that you need to define to allow the Z80 to work in an emulated hardware environment.
- `Z80Cpu-ExecutionCycle.cs`: This partition lists the methods that contribute to the CPU's execution cycle.
- `Z80Cpu-ExtendedInstructions.cs`: This file contains the code for processing extended Z80 instructions (with `$ED` prefix).
- `Z80Cpu-Helpers.cs`: Contains miscellaneous helpers for instruction execution.
- `Z80Cpu-IndexedInstructions.cs`: This partition contains the code for processing IX- or IY-indexed bit manipulation instructions (with `$DDCB` or `$FDCB` prefix).
- `Z80Cpu-IndexedInstructions.cs`: This partition contains the code for processing IX- or IY-indexed Z80 instructions (with `$DD` or `$FD` prefix).
- `Z80Cpu-StandardInstructions.cs`: This file contains the code for processing standard Z80 instructions (with no prefix).
- `Z80Cpu-State.cs`: This partition defines the state information we use while emulating the behavior of the CPU.

## The connection of the Z80 CPU and the Emulated Machine

When designing the emulator's architecture, I made a design decision (you can read more about it [here](../machine/architecture.md#design-decisions)). This design improves the performance of the emulator; however, it has some consequences:
- The class that emulates a particular machine (in this case, `ZxSpectrum48Machine`) must derive from the `Z80Cpu` class and override a few methods that implement memory and I/O port handling.
- The `Z80Cpu` implementation must support machine frames (see [here](#machine-frames)).

## Registers and CPU state

The implementation uses a flat .NET structure (`Registers`) with the `[StructLayout(LayoutKind.Explicit)]` attribute to store and access Z80 register values. Each register and register pair defines an explicit field offset. With this technique, the LSB and MSB parts of a register pair share the same location as the 16-bit register pair. Because of this location sharing, we do not need separate operations for 8-bit and 16-bit register value updates.

Here is the list of 16-bit register pairs (and their 8-bit constituents) that the `Registers` structure stores:

- `AF` (`A` and `F`)
- `BC` (`B` and `C`)
- `DE` (`D` and `E`)
- `HL` (`H` and `L`)
- `AF'`
- `BC'`
- `DE'`
- `HL'`
- `IX` (`XH` and `XL`)
- `IY` (`YH` and `YL`)
- `IR` (`I` and `R`)
- `PC`
- `SP`
- `WZ` (`WH` and `WL`)

This structure utilizes the `FieldOffset` attribute to let the 16-registers and their 8-bit MSB and LSB share the same structure offset:

```csharp
[StructLayout(LayoutKind.Explicit)]
public class Registers
{
    [FieldOffset(0)]
    public ushort AF;
    // ...

    [FieldOffset(1)]
    public byte A;

    /// <summary>Flags</summary>
    [FieldOffset(0)]
    public byte F;

    // ...
}
```

From an emulation point of view, we need to keep track of these signals:

- **NMI** (Non-Maskable Interrupt): Though ZX Spectrum 48K does not use this interrupt, other Z80-based machine types and additional ZX Spectrum peripheral devices may utilize it.
- **INT** (Maskable Interrupt)
- **RESET**: This signal signs that we need to reset the CPU.

When we process these signals, the most important test is detecting any active signal. To make this test quick and straightforward, we use an enumeration, `Z80Signals`:

```csharp
[Flags]
public enum Z80Signals
{
    None = 0,
    Int = 0x01,
    Nmi = 0x02,
    Reset = 0x04,
}
```

Besides the registers and signals, we keep other CPU state information:

- **`InterruptMode`**: Stores the current interrupt mode of the CPU (0, 1, or 2).
- **`Iff1`**: This flip-flop indicates if the maskable interrupt is enabled (`true`) or disabled (`false`).
- **`Iff2`**: The purpose of this flop-flop is to save the status of `Iff1` when a non-maskable interrupt occurs.
- **`Halted`**: This flag indicates if the CPU is in a halted state.
- **`BaseClockFrequency`** : Holds the base clock frequency of the CPU. We use this value to calculate other machine attributes regarding the machine frame.
- **`ClockMultiplier`**: By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock frequency multiplier to emulate a faster CPU.
- **`Tacts`**: The number of T-states (clock cycles) elapsed since the last reset. You know that accurate timing is at the heart of the CPU's implementation. We use a 64-bit counter, representing a long enough period.
- **`TactsInFrame`**: The emulated machine runs a loop of machine frames. Each frame has the same duration in T-states; this property shows their number in a machine frame.
- **`Frames`**: The number of completed machine frames since the machine started. The value is calculated from `Tacts`, `TactsInFrame`, and `ClockMultiplier`.
- **`CurrentFrameTact`**: Indicates the current tact in the executing machine frame. Its value is between 0 and `TactsInFrame`; it is calculated from `Tacts`, `TactsInFrame`, and `ClockMultiplier`.
- **`FrameCompleted`**: This flag indicates that a machine frame has been completed since the last reset of this flag.
- **`F53Updated`**, **`PrevF53Updated`**: These flags keep track of modifications of the bit 3 and 5 flags of Register F. We need to keep this value, as we utilize it within the `SCF` and `CCF` instructions to calculate the new values of F.
- **`OpCode`**: The last fetched opcode. If an instruction is prefixed, it contains the prefix or the opcode following the prefix, depending on which was fetched last.
- **`Prefix`**: The current prefix to consider when processing the subsequent opcode.
- **`EiBacklog`**: We use this variable to handle the EI instruction properly. When an EI instruction is executed, any pending interrupt request is not accepted until after the instruction following EI is executed. This single instruction delay is necessary when the next instruction is a return instruction. Interrupts are not allowed until a return is completed.
- **`RetExecuted`**: We need this flag to implement the step-over debugger function that continues the execution and stops when the current subroutine returns to its caller. The debugger will observe the change of this flag and manage its internal tracking of the call stack accordingly.
- **`AllowExtendedInstructions`**: The ZX Spectrum Next computer uses additional Z80 instructions. This flag indicates if those are allowed.

## Machine Frames

Emulating a computer should simulate the real-time behavior, producing the exact timing as if we used the current hardware. However, the modern PCs on which we run the emulator software use a specific CPU clock frequency that often changes. There is a simple technique to resolve this issue:

We select a known period, called *machine frame*, which carries out a task that always takes the same number of T-states. We can calculate the time the *machine frame* takes from the number of T-states in this period and the current CPU frequency. When emulating real-time behavior, we run the machine frame continuously in a loop:

1. We record the start time on the host PC.
2. We let the machine and the CPU run as fast as the PC running the emulator allows it, executing a complete machine frame.
3. We process the output (screen, sound, whatever else) the machine has generated in this single frame.
4. From the duration of the real-time machine frame, we subtract the time elapsed from the beginning of step 1 and let the emulator process sleep for the calculated time. Then we go back to step 1.

In the case of the ZX Spectrum 48, there is a natural choice of machine frame, the screen rendering frame. It always takes exactly 69,888 T-states (ca. 19.97 milliseconds). At the end of the machine frame, we have a screen to display. We can generate 50.08 frames per second with this approach, just as the real hardware does.

The implementation of the Z80 CPU supports the machine frame infrastructure with the `Frames`, `CurrentFrameTact`, and `FrameCompleted` state variables.

> *Note*:  A machine emulator must calculate the `TactsInFrame` value of the corresponding machine when creating the machine instance and invoke the `SetTactsInFrame` method to set this value.

## Timing

The CPU administers the time elapsed since the last hard or soft reset in the `Tacts` variable. Whenever the clock steps (moves to the next T-state), the code increments this variable. In a computer, other hardware components work parallel with the CPU. For example, in ZX Spectrum, the ULA is such a component; in ZX Spectrum 128/2/2+/3/3+ the AY-3-8912 Programmable Sound Generator chip is another example.

We do not use multiple threads in the emulator to handle such a scenario.

Instead, we let other hardware components perform their duties within that clock cycle whenever `Tacts` increments.

> _Note_: Here, we use the assumption that the CPU's clock-signal resolution is enough to emulate other hardware components. This presumption is valid for the ZX Spectrum family of retro-computers. However, we may need to use a different approach in other cases.

The code contains predefined increment methods named `TactPlus1`, `TactPlus2`, `TactPlus3`, and others to increment `Tacts` by one, two, three, and other values. These methods have this form:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void TactPlus1()
{
    Tacts += 1;
    OnTactIncremented(1);
}

// ...

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void TactPlusN(byte n)
{
    Tacts += n;
    OnTactIncremented(n);
}

// ...

public virtual void OnTactIncremented(int increment)
{
}
```

A machine emulator can use the `OnTactIncremented` method to emulate the behavior of other hardware components. The method receives the increment value; the default implementation has an empty method body. However, when integrating the `Z80Cpu` component with other elements in the emulated hardware (such as the ULA), you can create a method that correctly emulates the simultaneous execution.

We emulate multiplied clock frequency to calculate the length of the machine frame as `TactsInFrame * ClockMultiplier`. You can observe the `IncrementTacts` method sets the `FrameCompleted` flag, and resetting it is the responsibility of the machine loop.

You can also observe (see the `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute) that we ask the compiler for inlining these timing methods.

## Memory Contention

Many operations read and write the memory contents, and those instructions place the address onto the bus. If not just the CPU but other hardware components use the bus, they may contend with the CPU for the right of using the bus. As only one component can use the bus, other components must wait.

In the case of ZX Spectrum, the CPU competes with the ULA for the bus. Because the ULA has priority, the CPU must wait in such cases. The delay depends on the address put onto the bus.

The Z80 implementation has unique timing methods that take a 16-bit address as an argument. For example, we have two `TactPlus1` methods:

```csharp
void TactPlus1();
void TactPlus1(ushort address);
```

While the first method always increments the current T-state counter by one. The second implements it by one, and if the provided `address` points to a contended memory address, the T-state counter may be incremented with another value (zero or higher), depending on `address`.

The `TactPlusN` methods use this implementation:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void TactPlus1(ushort address)
{
    if (DelayedAddressBus) DelayAddressBusAccess(address);
    TactPlus1();
}

// ...

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void TactPlus4(ushort address)
{
    if (DelayedAddressBus) DelayAddressBusAccess(address);
    TactPlus1();
    if (DelayedAddressBus) DelayAddressBusAccess(address);
    TactPlus1();
    if (DelayedAddressBus) DelayAddressBusAccess(address);
    TactPlus1();
    if (DelayedAddressBus) DelayAddressBusAccess(address);
    TactPlus1();
}

// ...

public virtual void DelayAddressBusAccess(ushort address) 
{
}
```

Here, the `DelayedAddressBus` flag determines if the address bus of a particular emulated machine is delayed or not; the ZX Spectrum 48 machine sets this value to `true`.

The `DelayAddressBusAccess` method has an empty body; an emulated machine must override it to emulate the contention used by the multiple components when accessing the address bus.

## The Execution Cycle of the CPU

We emulate the behavior of the CPU by invoking its execution cycle in a loop. This cycle focuses on processing the subsequent instruction byte (pointed by the Program Counter register). Standard instructions use a single-byte opcode, so the execution cycle processes the entire instruction. If the opcode uses some operation data (like the `LD BC,nnnn` instruction, which has two data bytes defining the 16-bit lata to load to BC), the CPU reads those bytes in a single cycle.

However, if an instruction has one or more prefix bytes (such as `$CB`, `$ED`, `$DD`, `$FD`, `$DDCB` , or `$FDCB`), the instruction gets processed in multiple cycles: one cycle for each prefix byte plus another cycle for the instruction.

The execution loop contains these steps:

1. The CPU tests if any signal is active. If so, it processes them in this order (priority):
   - RESET
   - Non-Maskable Interrupt (NMI)
   - Maskable Interrupt (INT)
2. If the CPU is halted, it waits for 4 T-states and immediately completes the execution loop.
3. The CPU reads the next opcode byte from the address pointed by the PC register. This activity takes 3 T-states.
4. The CPU increments the last seven bits of the R register and puts the value of the IR register-pair to the address bus. Then, it triggers a memory refresh operation on physical hardware. (In the emulator, we do not need to execute this last step.)
5. The CPU completes the processing of the opcode byte (this phase takes one T-state). For the simplest instructions, it means completing the instruction. For instructions with data bytes, this step prepares for processing the rest of the instruction. For prefixed operations, the CPU prepares to carry on further processing.

> _Note_: Of course, in step 5, executing instructions may take additional time, including memory and I/O port access and 16-bit arithmetic.

## Handling Z80 Signals

The execution cycle first tests if there are any active signals to handle. Those are processed separately, following the priority order (RESET, NMI, INT). The Z80 CPU has other input signals, such as WAIT and BUSRQ. We do not need them for the ZX Spectrum 48K emulation. Nonetheless, we may need them later for emulating the communication with a peripheral device. Actually, the ULA uses the WAIT signal to delay the CPU while reading the screen memory. Anyhow, we have a relatively simple way to emulate the delay without watching the WAIT signal.

### Hard Reset and Soft Reset

When the CPU is powered up, it executes a hard reset that sets the registers to these values:

- **`AF`**, **`AF'`**, **`SP`**: `0xffff`
- **`BC`**, **`BC'`**, **`DE`**, **`DE'`**, **`HL`**, **`HL'`**, **`IX`**, **`IY`**, **`IR`**, **`PC`**, **`WZ`**: `0x0000`

The hard reset disables the interrupt by resetting the `IFF1` and `IFF2` interrupt flip-flops. The CPU enters into interrupt mode 0.

When the RESET signal gets active, the CPU executes a soft reset. Is sets the values of these registers:

- **`AF`**, **`AF'`**, **`SP`**: `0xffff`
- **`PC`**, **`WZ`**: `0x0000`

All other registers keep their value before the reset. Like the hard reset, the soft reset disables the interrupt by resetting the `IFF1` and `IFF2` interrupt flip-flops. The CPU enters into interrupt mode 0.

### Handling the Non-Maskable Interrupt (NMI)

Both the NMI and INT signals can be active at the same time. In such a case, NMI gets priority.

Handling the NMI contains these steps (11 T-states):

1. It takes 4 T-states while the CPU acknowledges the NMI signal.
2. If the CPU is in a halted state, it gets out of it.
3. The IIF2 flip-flop stores the value of IFF1. The purpose of IFF2 is to save the status of IFF1 when a non-maskable interrupt occurs. When a non-maskable interrupt is accepted, IFF1 resets to prevent further interrupts until reenabled by the programmer. Therefore, after a non-maskable interrupt is accepted, maskable interrupts are disabled, but the previous state of IFF1 is saved so that the complete state of the CPU just prior to the non-maskable interrupt can be restored at any time.
4. The IFF1 flip-flop gets reset, disabling further maskable interrupts.
5. The CPU pushes the current value of the Program Counter to the stack. This operation takes 7 T-states.
6. The CPU sets the Program Counter to `$0066`, the start of the NMI handler routine. Due to the Z80's internal architecture, this operation overlaps with storing the PC to the stack and does not require any additional clock cycles.

### Handling the Maskable Interrupt (INT)

The presence of an active INT signal is not enough to invoke the interrupt handler. The CPU scans the INT signal only at the very end of the completion of the instruction, and the emulator checks for an active INT signal before processing a new instruction. Provided the IFF1 flip-flop is enabled, the CPU invokes the interrupt handler with these steps:

1. It takes 4 T-states while the CPU acknowledges the INT signal.
2. If the CPU is in a halted state, it gets out of it.
3. The IFF1 and IFF2 flip-flops get reset, disabling further maskable interrupts.
4. The CPU pushes the current value of the Program Counter to the stack. This operation takes 7 T-states.

According to the current Interrupt Mode, the CPU invokes the interrupt handler method:

- On ZX Spectrum, Interrupt Mode 0 and 1 result in the same behavior, as no peripheral device would put an instruction on the data bus. In Interrupt Mode 0, the CPU would read a `$FF` value from the bus, the opcode for the `RST $38` instruction. In Interrupt Mode 1, the CPU responds to an interrupt by executing an `RST $38` instruction. Due to the Z80's internal architecture, setting the WZ register to `$0038` overlaps with storing the PC to the stack and does not require additional clock cycles.
- In Interrupt Mode 2, the CPU creates a 16-bit address to read the interrupt handler routine's address. The upper 8 bits of the address are the I register contents; the lower 8 bits are the value read from the data bus. As no device puts data to the bus, the CPU always reads `$FF`. Having the vector address, the CPU reads the first byte into WL (the LSB of the WZ register), the second byte into WH (the MSB of the WZ register). These read operations take 6 T-states.
- The CPU puts the contents of the WZ register into the PC register and starts the interrupt handler's execution.

> _Note_: In Interrupt Mode 0 and 1, calling the handler routine takes 13 T-states, while in Interrupt Mode 2, it is 19 T-states.

## Memory and I/O Operations

The operation of the CPU is not complete without physical memory that stores the program to execute and the data to operate—also, most hardware use some I/O ports to communicate with the environment.

The `Z80Cpu` class provides eight virtual and abstract functions that handle the memory and the I/O:

```csharp
public class Z80Cpu
{
    // ...
    public abstract byte DoReadMemory(ushort address);
    public virtual void DelayMemoryRead(ushort address) => TactPlus3();
    public abstract void DoWriteMemory(ushort address, byte value);
    public virtual void DelayMemoryWrite(ushort address) => TactPlus3();
    public abstract byte DoReadPort(ushort address);
    public virtual void DelayPortRead(ushort address) => TactPlus4();
    public abstract void DoWritePort(ushort address, byte value);
    public virtual void DelayPortWrite(ushort address) => TactPlus4();
    // ...
}
```

The names of these methods are self-explaining. An emulated machine must override all `Do...` methods, as they are abstract. The `Delay...` methods implement the default delay of the Z80 CPU. However, particular hardware may result in different delays; you can override these methods to emulate them accurately.

The `Z80Cpu` implementation uses the methods above in its `ReadMemory`, `Writememory`, `ReadPort`, and `WritePort` operations:

```csharp
public class Z80Cpu
{
    // ...
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadMemory(ushort address)
    {
        DelayMemoryRead(address);
        return DoReadMemory(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteMemory(ushort address, byte data)
    {
        DelayMemoryWrite(address);
        DoWriteMemory(address, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadPort(ushort address)
    {
        DelayPortRead(address);
        return DoReadPort(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WritePort(ushort address, byte data)
    {
        DelayPortWrite(address);
        DoWritePort(address, data);
    }

    // ...
}
```

If you replace the default implementation with your own, remember that memory operations use at least 3 T-states delay, and I/O port operations add at least 4 T-states. If you use values less than those, you implement a Z80 CPU emulation that violates the hardware concepts.

## Executing Instructions

One of the essential methods of the `Z80Cpu` is the `ExecuteCpuCyle`, which implements the CPU's execution loop, as its name suggests. It carries out the step you have learned earlier in the [The Execution Cycle of the CPU](#the-execution-cycle-of-the-cpu) section. This method follows the signal an instruction processing state through the following members:

- **`OpCode`**: The last fetched opcode. If an instruction is prefixed, it contains the prefix or the opcode following the prefix, depending on which was fetched last.
- **`Prefix`**: The current prefix to consider when processing the subsequent opcode.
- **`EiBacklog`**: We use this variable to handle the EI instruction properly. When an EI instruction is executed, any pending interrupt request is not accepted until after the instruction following EI is executed. This single instruction delay is necessary when the next instruction is a return instruction. Interrupts are not allowed until a return instruction is completed.

`ExecuteCpuCycle` keeps track of the current operation prefix (obviously in the `Prefix` field). When `Prefix` has a `None` value after the `ExecuteCpyCycle` returns, it signs that a complete Z80 instruction has been executed. Any other values mean that the execution is somewhere in the middle of a multi-byte instruction.

To enhance instruction execution performance, the `Z80Cpu` class uses jump tables. It has separate jump tables for these instruction classes:

- Standard instructions
- Bit manipulation
- Extended instructions
- Indexed instructions (IX- and IY-indexed instructions use the same table)
- Indexed bit manipulations (IX- and IY-indexed instructions use the same table)

Here is an extract from the `Z80Cpu-StandardInstrcutions.cs` file; it demonstrates the use of the `_standardInstrs` table, which is a jump table for standard intructions:

```csharp
public partial class Z80Cpu
{
    // ...
    private Action[]? _standardInstrs;

    private void InitializeStandardInstructionsTable()
    {
        _standardInstrs = new Action[]
        {
            Nop,        LdBCNN,     LdBCiA,     IncBC,      IncB,       DecB,       LdBN,       Rlca,       // 00-07
            // ...
            LdA_B,      LdA_C,      LdA_D,      LdA_E,      LdA_H,      LdA_L,      LdA_HLi,    Nop,        // 78-7f
            // ...
        }
    }

    // ...
    private void Nop() { }

    // ...
    private void LdBCNN()
    {
        Regs.C = ReadCodeMemory();
        Regs.B = ReadCodeMemory();
    }

    // ...
}
```

The execution cycle uses the current opcode byte to address the jump table and call the corresponding instruction method:

```csharp
public void ExecuteCpuCycle()
{
    // ...
    switch (Prefix)
    {
        case OpCodePrefix.None:
            switch (OpCode)
            {
                case 0xcb:
                    Prefix = OpCodePrefix.CB;
                    break;
                
                // ...
                default:
                    _standardInstrs![OpCode]?.Invoke();
                    Prefix = OpCodePrefix.None;
                    break;
                }
                break;
        // ...
    }
    // ...
}
```

### The `$DD` and `$FD` prefixes

The Z80 CPU has two prefixes, `$DD` and `$FD`, which signify that the next operation will be indexed. These prefixes work differently than the `$CB` and `$ED` prefixes.

When an opcode follows a `$CB` prefix, that opcode signifies a bit manipulation instruction, and the CPU executes it accordingly. All the possible 256 opcodes after `$CB` are meaningful operations. When the CPU executes an instruction after the `$ED` prefix, only a small set of the potential 256 instructions are used; the others result in a `NOP` instruction. So, the `$CB` and `$ED` prefixes imply a two-byte opcode.

However, the `$DD` and `$FD` prefixes work differently. These are like pseudo instructions, and they tell the CPU to use the IX or IY register in the subsequent instructions interpreted as standard instructions, provided those are related to the HL register. Also, if those use either the H or L registers, the CPU will use the upper or lower 8-bit halves of IX or IY. If the subsequent instruction is not HL-related, the CPU ignores the `$DD` or `$FD` prefixes. (Of course, in this case, the instruction execution takes 4 T-states longer, with the duration of the prefix processing.)

So, you can add as many `$DD` and `$FD` prefixes after each other as you want. The `$DD`, `$FD`, `$DD` prefix sequence is the same as a single `$DD`. Similarly, the `$DD`, `$DD`, `$FD` sequence is the same as a single `$FD`. Of course, each prefix adds 4 T-states to the instruction processing time.

There is an essential consequence of this architecture: you can create very long instructions by means of the T-states they need to run. Because the CPU samples interrupt signals (both NMI and INT) only when instruction execution is about to complete, such (intentionally or accidentally) long prefix sequences may cause the program to miss the active interrupt signal.

### Instruction Helpers

The Z80 CPU has many instructions, and some are very similar to others. In the `Z80Cpu-Helpers.cs` file, I collected a few helpful methods that the implementation of Z80 instructions share. Here is an example:

```csharp
private void CallCore()
{
    TactPlus1(Regs.IR);
    Regs.SP--;
    WriteMemory(Regs.SP, (byte)(Regs.PC >> 8));
    Regs.SP--;
    WriteMemory(Regs.SP, (byte)Regs.PC);
    Regs.PC = Regs.WZ;
}
```

The CPU emulation consumes this method in about a dozen instructions. For example, the `CALL NZ,NN` instruction checks the Z flag before invoking `CallCore`:

```csharp
private void CallNZ()
{
    Regs.WL = ReadCodeMemory();
    Regs.WH = ReadCodeMemory();
    if ((Regs.F & FlagsSetMask.Z) == 0)
    {
        CallCore();
    }
}
```

The `CallCore` method, as its name suggests, handles calling a subroutine by pushing the return address to the stack.

### ALU Operation Helpers

The Z80 CPU offers about a dozen arithmetic and logic operations (implemented by the ALU within the CPU). These operations, including 8-bit and 16-bit ones, set the eight bits of the F register in operation-specific ways. This list is a basic description of how the ALU operations influence the values of the flags:

- **S (Sign) flag**: It stores the state of the Accumulator's most significant bit (bit 7). When the Z80 CPU performs arithmetic operations on signed numbers, it uses the binary twos-complement notation to represent and process numeric information.
- **Z (Zero) flag**: This flag is set (1) or cleared (0) if the result generated by the execution of particular instructions is 0. For 8-bit arithmetic and logical operations, the Z flag is set to 1 if the resulting byte in the Accumulator is zero. If the byte is not zero, the Z flag is reset to 0.
- **R5 (Undocumented) flag**: Though there is no explicit semantics behind this flag, numerous Z80 instructions set or reset its value. ALU operations generally copy Bit 5 of the Accumulator into this flag, but some exceptional cases are. _Some documentation names this flag Y flag._
- **H (Half-Carry) flag**: This flag is set (1) or cleared (0) depending on the Carry and Borrow status between bits 3 and 4 of an 8-bit arithmetic operation. This flag is used by the Decimal Adjust Accumulator (`DAA`) instruction to correct the result of a packed BCD add or subtract operation.
- **R3 (Undocumented) flag**: Though there is no explicit semantics behind this flag, numerous Z80 instructions set or reset its value. ALU operations generally copy Bit 3 of the Accumulator into this flag, but some exceptional cases are. _Some documentation names this flag X flag._
- **PV (Parity/Overflow) flag**: It is set to a specific state depending on the operation being performed. For arithmetic operations, this flag indicates an overflow condition when the result in the Accumulator is greater than the maximum possible number (+127) or is less than the minimum possible number (–128). This overflow condition is determined by examining the sign bits of the operands.
- **N (Add/Subtract) flag**: This flag is used by the Decimal Adjust Accumulator instruction (`DAA`) to distinguish between the `ADD` and `SUB` instructions. For `ADD` instructions, N is cleared to 0. For `SUB` instructions, N is set to 1.
- **C (Carry) flag**: This flag is set or cleared depending on the operation being performed.

When emulating an ALU operation, for example, a 16-bit addition, the source code that executes the operation is simple compared to the one that calculates the new value of F register:

```csharp
private ushort Add16(ushort regHL, ushort regOther)
{
    var tmpVal = regHL + regOther;
    var lookup =
        ((regHL & 0x0800) >> 11) |
        ((regOther & 0x0800) >> 10) |
        ((tmpVal & 0x0800) >> 9);
    Regs.WZ = (ushort)(regHL + 1);
    Regs.F = (byte)((Regs.F & FlagsSetMask.SZPV) |
        ((tmpVal & 0x10000) != 0 ? FlagsSetMask.C : 0x00) |
        ((tmpVal >> 8) & (FlagsSetMask.R3R5)) |
        s_HalfCarryAddFlags[lookup]);
    F53Updated = true;
    return (ushort)tmpVal;
}
```

Here, the `Add16` method implements the core of a 16-bit addition. The first code line carries out the addition of the two 16-bit values. The second line (initializing the `lookup` variable) and the assignment of `Regs.F` show how complicated the new F register value calculation is. You can also observe that the code utilizes a helper table (`s_HalfCarryAddFlags`).

Many 8-bit ALU operations, such as `INC` and `DEC` use pre-calculated helper tables that contain some flag values for a particular operand. For single-operand instructions (like `INC` and `DEC`), we have only 256 possible operands, so this approach seems easy. The `INC B` instruction affects the flags this way by the official documentation:

- S is set if the result is negative; otherwise, it is reset.
- Z is set if the result is 0; otherwise, it is reset.
- H is set if carry from bit 3; otherwise, it is reset.
- PV is set if B was 7Fh before operation; otherwise, it is reset.
- N is reset.
- C is not affected.

The unofficial documentation specifies that the R5 and R3 flags take their value from bit 3 and 5 of the Accumulator, respectively.

The implementation of `INC B` calculates the new value of the F register with a helper table, `s_8BitIncFlags`.

```csharp
private void IncB()
{
    Regs.F = (byte)(s_8BitIncFlags[Regs.B++] | (Regs.F & FlagsSetMask.C));
    F53Updated = true;
}
```

When the `Z80Cpu` class initializes an instance, it takes care of setting up the helper tables, including `s_8BitIncFlags` and the others. For example, this is the code snippet that prepares `s_8BitIncFlags`:

```csharp
// ...
s_8BitIncFlags = new byte[0x100];
for (var b = 0; b < 0x100; b++)
{
    var oldVal = (byte)b;
    var newVal = (byte)(oldVal + 1);
    var flags =
        // --- C is unaffected, we keep it 0 here in this table.
        (newVal & FlagsSetMask.R3) |
        (newVal & FlagsSetMask.R5) |
        ((newVal & 0x80) != 0 ? FlagsSetMask.S : 0) |
        (newVal == 0 ? FlagsSetMask.Z : 0) |
        ((oldVal & 0x0F) == 0x0F ? FlagsSetMask.H : 0) |
        (oldVal == 0x7F ? FlagsSetMask.PV : 0);
        // --- Observe, N is 0, as this is an increment operation
    s_8BitIncFlags[b] = (byte)flags;
}
// ...
```

### Why `WZ`, `R5`, and `R3` So Important Are

The internal implementation of the Z80 hardware uses a 16-bit register, WZ (`MEMPTR`), as a temporary buffer during the execution of many instructions. It is also split into two 8-bit registers, `WH`, the MSB, and `WL`, the LSB. At first sight, you may think we do not need to care about the value of an internal register, as it cannot be read or written directly.

However, when executing a `BIT` instructions, the internal value of `WZ` determines the result of the R3 and R5 bits of the F register after the operation completes. As F can be written to the stack with the `PUSH AF` instruction and read from the top with a `POP`, we can observe the effect of `WZ`. So, we cannot avoid implementing the behavior of `WZ`.

The following code sets the new value of F after a `BIT` instruction. You can see it uses the value of `WH`:

```csharp
// ...
Regs.F = (byte)((Regs.F & FlagsSetMask.C) | FlagsSetMask.H | (Regs.WH & FlagsSetMask.R3R5));
var bitVal = oper & (0x01 << bit);
if (bitVal == 0)
{
    Regs.F |= FlagsSetMask.PV | FlagsSetMask.Z;
}
Regs.F |= (byte)(bitVal & FlagsSetMask.S);
// ...
```

> *Note*: The Z80 CPU offers 192 `BIT` operations that differ in their arguments.

## A Few Instruction Implementation Samples

In this section, I will highlight a few interesting implementation details of particular instructions.

### Register loading operations

> 8-bit register-to-register load operations are as simple as you expect them:

```csharp
// "ld h,c" operation (0x61)
private void LdH_C()
{
    Regs.H = Regs.C;
}
```

> Observe that the `LD (BC),A` operation sets the upper 8 bits of the `WZ` internal register (`MEMPTR`):

```csharp
// "ld (bc),a" operation (0x02)
private void LdBCiA()
{
    WriteMemory(Regs.BC, Regs.A);
    Regs.WH = Regs.A;
}
```

> The `WZ` register is used in every load operation when the CPU uses 16-bit addresses:

```csharp
// "ld hl,(NN)" operation (0x2A)
private void LdHLNNi()
{
    ushort adr = FetchCodeByte();
    adr += (ushort)(FetchCodeByte() << 8);
    Regs.WZ = (ushort)(adr + 1);
    ushort val = ReadMemory(adr);
    val += (ushort)(ReadMemory(Regs.WZ) << 8);
    Regs.HL = val;
}

// "ld (NN),a" operation (0x32)
private void LdNNiA()
{
    var l = FetchCodeByte();
    var addr = (ushort)((FetchCodeByte() << 8) | l);
    Regs.WL = (byte)((addr + 1) & 0xFF);
    Regs.WH = Regs.A;
    WriteMemory(addr, Regs.A);
}
```

> Indexed load operations use a signed 8-bit displacement relative to the base address. The `IndexReg` property of `Z80Cpu` refers to `IX` or `IY` depending on the current instruction prefix (`0xDD` or `0xFD`):

```csharp
// "ld (ix+D),d" operation (0x72)
private void LdIXiD()
{
    byte dist = ReadMemory(Regs.PC);
    TactPlus5(Regs.PC);
    Regs.PC++;
    Regs.WZ = (ushort)(IndexReg + (sbyte)dist);
    WriteMemory(Regs.WZ, Regs.D);
}
```

### 8-bit ALU Operations

> Several 8-bit operations use pre-calculated flag arrays to get the resulting flag values faster than calculating them on-the-fly:

```csharp
// "dec c" operation (0x0D)
// Register C is decremented.
// S is set if result is negative; otherwise, it is reset.
// Z is set if result is 0; otherwise, it is reset.
// H is set if borrow from bit 4, otherwise, it is reset.
// P/V is set if m was 80h before operation; otherwise, it is reset.
// N is set.
// C is not affected.
private void DecC()
{
    Regs.F = (byte)(s_8BitDecFlags[Regs.C--] | (Regs.F & FlagsSetMask.C));
    F53Updated = true;
}

// "inc e" operation (0x1C)
// Register E is incremented.
// S is set if result is negative; otherwise, it is reset.
// Z is set if result is 0; otherwise, it is reset.
// H is set if carry from bit 3; otherwise, it is reset.
// P/V is set if r was 7Fh before operation; otherwise, it is reset.
// N is reset.
// C is not affected.
private void IncE()
{
    Regs.F = (byte)(s_8BitIncFlags[Regs.E++] | (Regs.F & FlagsSetMask.C));
    F53Updated = true;
}
```

> Many 8-bit ALU operations have helper methods that calculate the resulting flag values:

```csharp
// "and d" operation (0xA2)
private void AndD()
{
    And8(Regs.D);
}

// The core of the 8-bit AND operation
private void And8(byte value)
{
    Regs.A &= value;
    Regs.F = (byte)(FlagsSetMask.H | s_SZ53PVTable![Regs.A]);
    F53Updated = true;
}

/// "sub l" operation (0x95)
private void SubL()
{
    Sub8(Regs.L);
}

// The core of the 8-bit SUB operation
private void Sub8(byte value)
{
    var tmp = Regs.A - value;
    var lookup =
        ((Regs.A & 0x88) >> 3) |
        ((value & 0x88) >> 2) |
        ((tmp & 0x88) >> 1);
    Regs.A = (byte)tmp;
    Regs.F =(byte)
        (((tmp & 0x100) != 0 ? FlagsSetMask.C : 0) |
        FlagsSetMask.N |
        s_HalfCarrySubFlags[lookup & 0x07] |
        s_OverflowSubFlags[lookup >> 4] |
        s_SZ53Table![Regs.A]);
        F53Updated = true;
}
```

### Conditional Jump Operations

> The implementation of conditional jump statements is pretty straightforward. Observe, 16-bit address handling uses the `WZ` internal register:

```csharp
// "jr nc,E" operation (0x30)
private void JrNC()
{
    var e = FetchCodeByte();
    if ((Regs.F & FlagsSetMask.C) == 0)
    {
        RelativeJump(e);
    }
}

// "jp nc,NN" operation (0xD2)
private void JpNC_NN()
{
    Regs.WL = FetchCodeByte();
    Regs.WH = FetchCodeByte();
    if ((Regs.F & FlagsSetMask.C) == 0)
    {
        Regs.PC = Regs.WZ;
    }
}
```

### Subroutine Calls

> The `CALL` instructions use a helper method (`CallCore`) that deals with the details of managing the stack:

```csharp
// "call NN" operation (0xCD)
private void CallNN()
{
    Regs.WL = FetchCodeByte();
    Regs.WH = FetchCodeByte();
    CallCore();
}

// "call nz,NN" operation (0xC4)
private void CallNZ()
{
    Regs.WL = FetchCodeByte();
    Regs.WH = FetchCodeByte();
    if ((Regs.F & FlagsSetMask.Z) == 0)
    {
        CallCore();
    }
}

// The core of the CALL instruction 
private void CallCore()
{
    TactPlus1(Regs.IR);
    Regs.SP--;
    WriteMemory(Regs.SP, (byte)(Regs.PC >> 8));
    Regs.SP--;
    WriteMemory(Regs.SP, (byte)Regs.PC);
    Regs.PC = Regs.WZ;
}
```

### Bit Instructions

> Bit instructions use simple helpers:

```csharp
// "sla c" operation (0xCB, 0x21)
private void Sla_C()
{
    Regs.C = Sla8(Regs.C);
}

// The core of the 8-bit SLA operation.
private byte Sla8(byte oper)
{
    Regs.F = (byte)(oper >> 7);
    byte result = (byte)(oper << 1);
    Regs.F |= s_SZ53PVTable![tmp];
    F53Updated = true;
    return result;
}

// "bit 7,e" operation (0xCB, 0x7B)
private void Bit7_E()
{
    Bit8(7, Regs.E);
}

// The core of the 8-bit BIT operation.
private void Bit8(int bit, byte oper)
{
    Regs.F = (byte)((Regs.F & FlagsSetMask.C) | FlagsSetMask.H | (oper & FlagsSetMask.R3R5));
    var bitVal = oper & (0x01 << bit);
    if (bitVal == 0)
    {
        Regs.F |= FlagsSetMask.PV | FlagsSetMask.Z;
    }
    Regs.F |= (byte)(bitVal & FlagsSetMask.S);
    F53Updated = true;
}
```

> The `RES` and `SET` instruction are straightforward:

```csharp
// "res 3,c" operation (0xCB, 0x99)
private void Res3C()
{
    Regs.C &= 0xf7;
}

// "res 5,(hl)" operation (0xCB, 0xAE)
private void Res5HLi()
{
    var tmp = (byte)(ReadMemory(Regs.HL) & 0xdf);
    TactPlus1(Regs.HL);
    WriteMemory(Regs.HL, tmp);
}

// "set 6,h" operation (0xCB, 0xF4)
private void Set6H()
{
    Regs.H |= 0x40;
}

// "set 2,(hl)" operation (0xCB, 0xD6)
private void Set2HLi()
{
    var tmp = (byte)(ReadMemory(Regs.HL) | 0x04);
    TactPlus1(Regs.HL);
    WriteMemory(Regs.HL, tmp);
}
```

### Block Operations

> Block operations are more complex than others, as the following implementation samples show. Look at the strange way they update the flag values! This information comes from the documentation that details officially undocumented details:

```csharp
// "ldir" operation (0xED, 0xB0)
private void Ldir()
{
    byte tmp = ReadMemory(Regs.HL);
    WriteMemory(Regs.DE, tmp);
    TactPlus2(Regs.DE);
    Regs.BC--;
    tmp += Regs.A;
    Regs.F = (byte)
        ((Regs.F & (FlagsSetMask.C | FlagsSetMask.Z | FlagsSetMask.S)) |
        (Regs.BC != 0 ? FlagsSetMask.PV : 0) |
        (tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
    F53Updated = true;
    if (Regs.BC != 0)
    {
        TactPlus5(Regs.DE);
        Regs.PC -= 2;
        Regs.WZ = (byte)(Regs.PC +1);
    }
    Regs.HL++;
    Regs.DE++;
}

// "cpdr" operation (0xED, 0xB9)
private void Cpdr()
{
    byte value = ReadMemory(Regs.HL);
    byte tmp = (byte)(Regs.A - value);
    var lookup =
    ((Regs.A & 0x08) >> 3) |
        ((value & 0x08) >> 2) |
        ((tmp & 0x08) >> 1);
    TactPlus5(Regs.HL);
    Regs.BC--;
    Regs.F = (byte)
    ((Regs.F & FlagsSetMask.C) |
        (Regs.BC != 0 ? (FlagsSetMask.PV | FlagsSetMask.N) : FlagsSetMask.N) |
        s_HalfCarrySubFlags![lookup] |
        (tmp != 0 ? 0 : FlagsSetMask.Z) |
        (tmp & FlagsSetMask.S));
    if ((Regs.F & FlagsSetMask.H) != 0)
    {
        tmp -= 1;
    }
    Regs.F |= (byte)((tmp & FlagsSetMask.R3) | ((tmp & 0x02) != 0 ? FlagsSetMask.R5 : 0));
    F53Updated = true;
    if ((Regs.F & (FlagsSetMask.PV | FlagsSetMask.Z)) == FlagsSetMask.PV)
    {
        TactPlus5(Regs.HL);
        Regs.PC -= 2;
        Regs.WZ = (byte)(Regs.PC + 1);
    }
    else
    {
        Regs.WZ--;
    }
    Regs.HL--;
}
```

## Testing the Z80 CPU

The Z80 CPU has more than a thousand instructions, each optionally reading and writing registers, memory, I/O ports, and setting flags. Also, the timing of instructions must be accurate.

Testing the operation of the CPU only within the code through the behavior of a retro computer emulator is an error-prone and laborious task.

In this project, I have implemented about 1500 unit tests, which all can be run in about 10-15 seconds, and repeated any time I modify or change any internal implementation detail of the CPU.

> *Note*: You can discuss whether the number of unit tests is sufficient for testing the Z80 CPU. Well, you can always add more and more tests, as I always do. Whenever I find a bug, I also create a regression test for that bug to ensure that it never happens again. The current set provides excellent code coverage and runs quick enough to repeat it even a hundred times a day.

These unit tests encapsulate the Z80 CPU implementation into a sandbox (represented by the `TestZ80Machine` class), which provides a testbed for the CPU. This virtual test machine has 64K RAM and simple I/O devices that catch and log any output operations; mock input operations with preset input values.

You can find the source of the test machine in the `Helpers/TestZ80Machine.cs` file within the `SpectrumEmu.Engine.Tests` project. All unit tests are located within the `Z80` folder.

Units test use the XUnit framework that works in concert with .NET 6. Here is a sample test case that demonstrates the basic concepts:

```csharp
[Fact]
public void RET_NZ_DoesNotReturnWhenZ()
{
    // --- Arrange
    var m = new Z80TestMachine(RunMode.UntilHalt);
    m.InitCode(new byte[]
    {
        0x3E, 0x00,       // LD A,00H
        0xCD, 0x06, 0x00, // CALL 0006H
        0x76,             // HALT
        0xB7,             // OR A
        0xC0,             // RET NZ
        0x3E, 0x24,       // LD A,24H
        0xC9              // RET
    });
    var regs = m.Cpu.Regs;
    regs.SP = 0;

    // --- Act
    m.Run();

    // --- Assert

    regs.A.ShouldBe((byte)0x24);
    m.ShouldKeepRegisters(except: "AF");
    m.ShouldKeepMemory(except: "FFFE-FFFF");

    regs.PC.ShouldBe((ushort)0x0005);
    m.Cpu.Tacts.ShouldBe(54UL);
}
```

The first instruction initializes the test machine so that its `Run` method will run the code until a `HALT` instruction executes. Then, the machine completes its operation, and we can examine its state.

The invocation of the `InitCode` method inject test code into the machine, starting at address `$0000`. You can see this code tests if the `RET NZ` operation works correctly with the Z flag set to 0. The Accumulator should contain `$24` if the code works as expected.

Besides checking the Accumulator value, the code checks if there are no unexpected side effects. A couple of helper methods (`ShouldKeepRegisters`, `ShouldKeepMemory`) can test if only those registers and memory locations change that we affected by the test code.

The last two assertions test if the code has been completed at the expected location, and the number of T-states elapsed is the one we anticipated, in this case, 54.
