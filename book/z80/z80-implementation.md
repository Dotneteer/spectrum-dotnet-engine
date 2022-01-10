# Implemeting the Z80 CPU

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

## Hard Reset and Soft Reset

When the CPU is powered up, it executes a hard reset that sets the registers to these values:

- **`AF`**, **`AF'`**, **`SP`**: `0xffff`
- **`BC`**, **`BC'`**, **`DE`**, **`DE'`**, **`HL`**, **`HL'`**, **`IX`**, **`IY`**, **`IR`**, **`PC`**, **`WZ`**: `0x0000`

The hard reset disables the interrupt by resetting the `IFF1` and `IFF2` interrupt flip-flops. The CPU enters into interrupt mode 0.

When the RESET signal gets active, the CPU executes a soft reset. Is sets the values of these registers:

- **`AF`**, **`AF'`**, **`SP`**: `0xffff`
- **`PC`**, **`WZ`**: `0x0000`

All other registers keep their value before the reset. Like the hard reset, the soft reset disables the interrupt by resetting the `IFF1` and `IFF2` interrupt flip-flops. The CPU enters into interrupt mode 0.