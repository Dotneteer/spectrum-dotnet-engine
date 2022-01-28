# The Implementation of the ZX Spectrum I/O handler

The I/O handler of the emulator is responsible for managing the I/O ports' read and write operations.

The ZX Spectrum 48K model handles a single partially mapped I/O port. When the lowest bit of the port address is zero (so the port number is even), the ULA processes the I/O read and write requests. (The ZX Spectrum ROM uses the `$FE` port address.)

## I/O Handler Implementation Source Code Files

- `Abstractions/IIoHandler.cs`: An abstract definition of the I/O handler.
- `Machines/ZxSpectrum48/ZxSpectrum48IoHandler.cs`: The class that implements the I/O handlers's functionality.

## Handling Output Operations

When you write a byte to the ZX Spectrum output port, the ULA uses the value this way:
- It stores the lowest three bits (Bit 0-Bit 2) to the border color register. When the ULA renders the next border pixel, it uses this value.
- Bit 3 activates the MIC bit. Alternating this value creates the signal that the ZX Spectrum uses to record data or code to the tape device.
- Bit 4 activates the EAR bit. Alternating this value generates sound in the internal speaker.
- The values of Bit 5-Bit 7 are unused.

## Handling Input Operations

When you read a byte from the ZX Spectrum input port, use the data bits the following way:
- The lowest five bits (Bit 0-Bit 4) represent the status of the keyboard lines. Learn more details about using the keyboard in the article about the Keyboard Device Implementation.
- Bit 5 is always 1.
- Bit 6 represents the EAR bit, the signal coming from the tape device.
- Bit 7 is always 1.

Because the ULA chip uses the same pin for the MIC socket (output), the EAR socket (input), and the internal speaker (output), the Bit 3 and 4 output values influence the Bit 6 (EAR bit) input. Moreover, a capacitor component in the hardware causes some delay:

- When the output value of Bit 4 changes from 0 to 1, there is no delay; the Bit 4 output immediately appears on the Bit 6 input.
- However, when the Bit 4 output changes from 1 to 0, there is a delay that depends on how long was Bit 4 active (1) previously. The delay varies approximately between 50 and 800 microseconds (and this delay also depends on whether the ULA is issue 2 or 3).

The implementation of the I/O port handler emulates the behavior of Bit 6 input.

## I/O Contention

When using an I/O port, the Z80 CPU contends with the ULA because they both want to use the address and data bus. In such cases, the ULA has priority over the CPU, and the Z80 must wait while the ULA releases the bus.

The amount of delay depends on the port address because of two effects:
1. If the lowest bit of the address is 0, the ULA has an active port read or write request to respond. Otherwise, the ULA may ignore the request.
2. If the port address is in the contended range (`$4000`-`$7fff`), the ULA perceives the operation as access to the contended memory and introduces delay.

According to these effects, there are four different delay schemes:

1. **The lowest bit of the port address is 1; the address is in the non-contended range**:
    - There is no contention. 
    - The CPU runs uninterrupted for 4 T-states while preparing for the I/O operation and completing it.
2. **The lowest bit of the port address is 0; the address is in the non-contended range**:
    - There is contention.
    - The CPU runs uninterrupted for a single T-states while preparing for the operation.
    - Then, the CPU waits for a few cycles, according to the current CPU tact within the current machine frame cycle. This delay comes from the ULA's behavior to hold the address bus while reading the screen memory.
    - At last, the CPU runs uninterrupted for 3 T-states while completing the I/O operation.
3. **The lowest bit of the port address is 1; the address is in the contended range**:
    - There is contention. 
    - The CPU needs 4 T-state to prepare and complete the I/O operation. However, the ULA also might want to keep control over the address bus, so the following steps are repeated four times:
        - According to the current CPU tact within the current machine frame cycle, the CPU may wait for a few cycles (it can even be zero).
        - After this delay, the CPU runs uninterrupted for the subsequent single T-states and can progress with the I/O operation.
4. **The lowest bit of the port address is 0; the address is in the contended range**:
    - There is contention. 
    - According to the current CPU tact within the current machine frame cycle, the CPU may wait for a few cycles (even zero) while preparing the I/O operation.
    - The CPU runs uninterrupted for the subsequent single T-states and prepares the I/O operation.
    - According to the current CPU tact within the current machine frame cycle, the CPU may wait for a few cycles (even zero) while preparing the I/O operation.
    - At last, the CPU runs uninterrupted for 3 T-states while completing the I/O operation.

## The Floating Bus

_TBD_

## The `IIoHandler` interface

The `IIoHandler` interface describes the operations to emulate the I/O operations of the emulated machine:

```csharp
public interface IIoHandler<TMachine>: IGenericDevice<TMachine> where TMachine : IZ80Machine
{
    /// <summary>
    /// Read a byte from the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <returns>The byte read from the port</returns>
    byte ReadPort(ushort address);

    /// <summary>
    /// Write the given byte to the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <param name="value">Byte to write to the I/O port</param>
    void WritePort(ushort address, byte value);
}
```

## The `ZxSpectrum48IoHandler` class

This class implements the peculiarities of the ZX Spectrum I/O port behavior. The class uses partial address mapping to dispatch the I/O operation value to the corresponding device:

```csharp
public class ZxSpectrum48IoHandler : IIoHandler<IZxSpectrum48Machine>
{
    // ...
    public byte ReadPort(ushort address)
    {
        return (address & 0x0001) == 0
            ? ReadPort0xFE(address)
            : Machine.FloatingBusDevice.ReadFloatingPort();
    }

    public void WritePort(ushort address, byte value)
    {
        if ((address & 0x0001) == 0)
        {
            WritePort0xFE(value);
        }
    }
    // ...
}
```

You can observe that reading from an unmapped (every odd) port number returns the value read from the floating bus.
