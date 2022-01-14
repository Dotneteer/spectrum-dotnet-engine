# Implementing the Z80 CPU

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
- **HALT**: This signal signs that the CPU must be halted.
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
    Halted = 0x08,
}
```

Besides the registers and signals, we keep other CPU state information:

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
    - HALT. When this signal is active, the CPU waits for 4 T-states and immediately completes the execution loop.
2. The CPU reads the next opcode byte from the address pointed by the PC register. This activity takes 3 T-states
3. The CPU increments the last seven bits of the R register and puts the value of the IR register-pair to the address bus. Then, it triggers a memory refresh operation on physical hardware. In the emulator, we do not need this step.
4. The CPU completes the processing of the opcode byte (this phase takes one T-state). For the simplest instructions, it means completing the instruction. For instructions with data bytes, this step prepares for processing the rest of the instruction. For prefixed operations, the CPU prepares to carry on further processing. 

> *Note*: Of course, in step 4, executing instructions may take additional time, including memory and I/O port access and 16-bit arithmetic.


## Handling Z80 Signals

*TBD*

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

*TBD*

### Handling the Maskable Interrupt (INT)

*TBD*

### Handling the HALT Signal

*TBD*

## Memory and I/O Operations

*TBD*

## Executing Instructions

*TBD*

### Standard Instructions

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
