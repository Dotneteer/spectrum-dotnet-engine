# Implementing the Z80 CPU

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
- **`Tacts`**: The number of T-states (clock cycles) elapsed since the last reset. You know that accurate timing is at the heart of the CPU's implementation. We use a 64-bit counter, representing a long enough period.
- **`F53Updated`**, **`PrevF53Updated`**: These flags keep track of modifications of the bit 3 and 5 flags of Register F. We need to keep this value, as we utilize it within the `SCF` and `CCF` instructions to calculate the new values of F.
- **`OpCode`**: The last fetched opcode. If an instruction is prefixed, it contains the prefix or the opcode following the prefix, depending on which was fetched last.
- **`Prefix`**: The current prefix to consider when processing the subsequent opcode.
- **`EiBacklog`**: We use this variable to handle the EI instruction properly. When an EI instruction is executed, any pending interrupt request is not accepted until after the instruction following EI is executed. This single instruction delay is necessary when the next instruction is a return instruction. Interrupts are not allowed until a return is completed.
- **`RetExecuted`**: We need this flag to implement the step-over debugger function that continues the execution and stops when the current subroutine returns to its caller. The debugger will observe the change of this flag and manage its internal tracking of the call stack accordingly.
- **`AllowExtendedInstructions`**: The ZX Spectrum Next computer uses additional Z80 instructions. This flag indicates if those are allowed.

## Timing

The CPU administers the time elapsed since the last hard or soft reset in the `Tacts` variable. Whenever the clock steps (moves to the next T-state), the code increments this variable. In a computer, other hardware components work parallel with the CPU. For example, in ZX Spectrum, the ULA is such a component; in ZX Spectrum 128/2/2+/3/3+ the AY-3-8912 Programmable Sound Generator chip is another example.

We do not use multiple threads in the emulator to handle such a scenario.

Instead, we let other hardware components perform their duties within that clock cycle whenever `Tacts` increments.

> *Note*: Here, we use the assumption that the CPU's clock-signal resolution is enough to emulate other hardware components. This presumption is valid for the ZX Spectrum family of retro-computers. However, we may need to use a different approach in other cases.

The code contains predefined increment methods named `TactPlus1`, `TactPlus2`, `TactPlus3`, and others to increment `Tacts` by one, two, three, and other values. These methods have this form:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void TactPlus1()
{
    Tacts++;
    TactIncrementedHandler();
}

// ...

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void TactPlus3()
{
    Tacts++;
    TactIncrementedHandler();
    Tacts++;
    TactIncrementedHandler();
    Tacts++;
    TactIncrementedHandler();
}
```

Here, the `TactIncrementedHandler` refers to a method in which we can emulate the behavior of other hardware components. By default, this is set to an empty method. However, when integrating the `Z80Cpu` component with other elements in the emulated hardware (such as the ULA), you can create a method that correctly emulates the simultaneous execution.

You can also observe (see the `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute) that we ask the compiler for inlining these timing methods.

## The Execution Cycle of the CPU

We emulate the behavior of the CPU by invoking its execution cycle in a loop. This cycle focuses on processing the subsequent instruction byte (pointed by the Program Counter register). Standard instructions use a single-byte opcode, so the execution cycle processes the entire instruction. If the opcode uses some operation data (like the `LD BC,nnnn` instruction, which has two data bytes defining the 16-bit lata to load to BC), the CPU reads those bytes in a single cycle.

However, if an instruction has one or more prefix bytes (such as `$CB`, `$ED`, `$DD`, `$FD`, `$DDCB` , or `$FDCB`), the instruction gets processed in multiple cycles: one cycle for each prefix byte plus another cycle for the instruction.

The execution loop contains these steps:
1. The CPU tests if any signal is active. If so, it processes them in this order (priority):
    - RESET
    - Non-Maskable Interrupt (NMI)
    - Maskable Interrupt (INT)
2. If the CPU is halted, it waits for 4 T-states and immediately completes the execution loop.
3. The CPU reads the next opcode byte from the address pointed by the PC register. This activity takes 3 T-states
4. The CPU increments the last seven bits of the R register and puts the value of the IR register-pair to the address bus. Then, it triggers a memory refresh operation on physical hardware. In the emulator, we do not need this step.
5. The CPU completes the processing of the opcode byte (this phase takes one T-state). For the simplest instructions, it means completing the instruction. For instructions with data bytes, this step prepares for processing the rest of the instruction. For prefixed operations, the CPU prepares to carry on further processing. 

> *Note*: Of course, in step 5, executing instructions may take additional time, including memory and I/O port access and 16-bit arithmetic.

## Handling Z80 Signals

The execution cycle first tests if there are any active signals to handle. Those are processed separately, following the priority order (RESET, NMI, INT). The Z80 CPU has other input signals, such as WAIT and BUSRQ. We do not need them for the ZX Spectrum 48K emulation. Nonetheless, we may need them later for emulating the communication with a peripheral device. Actually, the ULA uses the WAIT signal to delay the CPU while reading the screen memory. Nonetheless, we have a relatively simple way to emulate the delay without the need to watch the WAIT signal.

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

> *Note*: In Interrupt Mode 0 and 1, calling the handler routine takes 13 T-states, while in Interrupt Mode 2, it is 19 T-states.

## Memory and I/O Operations

The operation of the CPU is not complete without physical memory that stores the program to execute and the data to operate—also, most hardware use some I/O ports to communicate with the environment.
The `Z80Cpu` class provides four functions that ensure the connection to the physical memory and I/O ports:

```csharp
public class Z80Cpu
{
    // ...
    public Func<ushort, byte> ReadMemoryFunction;
    public Action<ushort, byte> WriteMemoryFunction;
    public Func<ushort, byte> ReadPortFunction;
    public Action<ushort, byte> WritePortFunction;
    // ...
}
```

- `ReadMemoryFunction`: This function reads a byte (8-bit) from the memory using the provided 16-bit address.
- `WriteMemoryFunction`: It writes a byte (8-bit) to the 16-bit memory address provided in the first argument.
- `ReadPortFunction`: This function reads a byte (8-bit) from an I/O port using the provided 16-bit address.
- `WritePortFunction`: It writes a byte (8-bit) to the 16-bit I/O port address provided in the first argument.

The `Z80Cpu` constructor sets up these function references to their default to throw an exception. The default implementation of each raises an exception:

```csharp
public Z80Cpu()
{
    ReadMemoryFunction = (ushort address) 
        => throw new InvalidOperationException("ReadMemoryFunction has not been set.");
    WriteMemoryFunction = (ushort address, byte data)
        => throw new InvalidOperationException("WriteMemoryFunction has not been set.");
    ReadPortFunction = (ushort address)
        => throw new InvalidOperationException("ReadPortFunction has not been set.");
    WritePortFunction = (ushort address, byte data)
        => throw new InvalidOperationException("WritePortFunction has not been set.");
    // ...
}
```
You must set up these function references when you use a `Z80Cpu` instance. This extract shows how the `TestZ80Machine` class (used in the automatic unit tests of `Z80Cpu`) does it:

```csharp
public class Z80TestMachine
{
    // ...
    public Z80Cpu Cpu { get; }
    // ...
    public Z80TestMachine(/* ... */)
    {
        // ...
        Memory = new byte[ushort.MaxValue + 1];
        Cpu = new Z80Cpu
        {
            // ...
            ReadMemoryFunction = ReadMemory,
            WriteMemoryFunction = WriteMemory,
            ReadPortFunction = ReadPort,
            WritePortFunction = WritePort
        };
        // ...
    }

    protected virtual byte ReadMemory(ushort addr)
    {
        var value = Memory[addr];
        MemoryAccessLog.Add(new MemoryOp(addr, value, false));
        return value;
    }

    protected virtual void WriteMemory(ushort addr, byte value)
    {
        Memory[addr] = value;
        MemoryAccessLog.Add(new MemoryOp(addr, value, true));
    }

    protected virtual byte ReadPort(ushort addr)
    {
        var value = IoReadCount >= IoInputSequence.Count
            ? (byte)0x00
            : IoInputSequence[IoReadCount++];
        IoAccessLog.Add(new IoOp(addr, value, false));
        return value;
    }

    protected virtual void WritePort(ushort addr, byte value)
    {
        IoAccessLog.Add(new IoOp(addr, value, true));
    }

    // ...
}
```

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
            _standardInstrs![OpCode]?.Invoke();
            Prefix = OpCodePrefix.None;
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

*TBD*

### Bit Instructions

*TBD*

### Extended Instructions

*TBD*

### Indexed Instructions

*TBD*

### Indexed Bit Instructions

*TBD*

## Testing the Z80 CPU

*TBD*
