# The Floating Bus Device Implementation

The `ZxSpectrum48FloatingBusDevice` class implements the behavior of the ZX Spectrum 48K's [floating bus]. This device takes care to emulate that the ULA puts the last read screen memory byte (either a pixel byte or an attribute byte value) onto the data bus.

Whenever the CPU reads an unhandled I/O port (the least significant bit of the port is set to one), the `WritePort0Xfe` method of `ZxSpectrum48Machine` delegates the responsibility of reading the value to the `ZxSpectrum48FloatingBusDevice` class.

The essence of the class is the `ReadFloatingBus` method. It checks the current screen rendering phase and sets the data bus value accordingly:

```csharp
public byte ReadFloatingBus()
{
    var screen = Machine.ScreenDevice;
    var renderingTact = screen.RenderingTactTable[Machine.CurrentFrameTact - 5];
    switch (renderingTact.Phase)
    {
        case RenderingPhase.BorderFetchPixel:
        case RenderingPhase.DisplayB1FetchB2:
        case RenderingPhase.DisplayB2FetchB1:
            return Machine.DoReadMemory((ushort)(screen.MemoryScreenOffset + renderingTact.PixelAddress));

        case RenderingPhase.BorderFetchAttr:
        case RenderingPhase.DisplayB1FetchA2:
        case RenderingPhase.DisplayB2FetchA1:
            return Machine.DoReadMemory((ushort)(screen.MemoryScreenOffset + renderingTact.AttributeAddress));

        default:
            return 0xff;
    }
}
```

> *Note*: Because of the timing of the ULA, the phase used 5 T-states back determines the screen memory value to put onto the data bus.



