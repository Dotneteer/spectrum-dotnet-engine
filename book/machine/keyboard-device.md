# Implementing the ZX Spectrum Keyboard

ZX Spectrum 48K has 40 physical keys arranged in an 8x5 matrix:

![Keyboard matrix](../figures/keyboard-matrix.png)

*Source*: https://www.1000bit.it/support/manuali/sinclair/zxspectrum/sm/supp2.html

Querying the state of a particular key is pretty simple. You can use the Z80 CPU's `IN` instruction to query a particular ULA port (remember, ULA support only event port numbers). When you read an input port, the CPU puts the port address to the address bus. The keyboard uses the top 8 address lines (A8-A15). Each key is a push button. If you press any of them, they connect the corresponding address line value to one of the data bus lines (0 to 4).

You read a value bit 1 on its associated data line when a key is not pressed. To sense a pressed key, you issue an `IN` statement that puts a zero bit on the address line of the button, and the code checks the appropriate bit of the data read back.

## The `IKeyboardDevice` interface

The `IKeyboardDevice` interface describes the operations to emulate the behavior of the keyboard:

```csharp
public interface IKeyboardDevice: IGenericDevice<IZxSpectrum48Machine>
{
    /// <summary>
    /// Set the status of the specified ZX Spectrum key.
    /// </summary>
    /// <param name="key">Key code</param>
    /// <param name="isDown">Indicates if the key is pressed down.</param>
    void SetStatus(SpectrumKeyCode key, bool isDown);

    /// <summary>
    /// Get the status of the specified Spectrum keyboard key.
    /// </summary>
    /// <param name="key">Key code</param>
    /// <returns>True, if the key is down; otherwise, false</returns>
    bool GetStatus(SpectrumKeyCode key);

    /// <summary>
    /// This method queries the status of the keyboard keys using the specified port address.
    /// </summary>
    /// <param name="address">Port address of the line to query</param>
    /// <returns>The data byte representing the keyboard status</returns>
    byte GetKeyLineStatus(ushort address);
}
```

## The `KeyboardDevice` class

A simple class, `KeyboardDevice`, implements the keyboard functionality. It stores the status of address lines in a byte array:

```csharp
public sealed class KeyboardDevice: IKeyboardDevice
{
    private readonly byte[] _lineStatus = new byte[8];

    public void Reset()
    {
        for (var i = 0; i < 8; i++) _lineStatus[i] = 0;
    }

    // ...
}
```

This field stores the status bits of keys. Each byte in the array represents an address line from A8 to A15, and the lower five bits represent the five keys associated with the particular address line. One means the key is pressed; zero stands for an unpressed key.

With this approach, it is easy to implement the `SetStatus` and `GetStatus` methods:

```csharp
public void SetStatus(SpectrumKeyCode key, bool isDown)
{
    var lineIndex = (byte)key / 5;
    var lineMask = 1 << (byte)key % 5;
    _lineStatus[lineIndex] = isDown
        ? (byte)(_lineStatus[lineIndex] | lineMask)
        : (byte)(_lineStatus[lineIndex] & ~lineMask);
}

public bool GetStatus(SpectrumKeyCode key)
{
    var lineIndex = (byte)key / 5;
    var lineMask = 1 << (byte)key % 5;
    return (_lineStatus[lineIndex] & lineMask) != 0;
}
```

The port handler device of the ZX Spectrum invokes the `GetLineStatus` method of `KeyboardDevice` whenever we query a mapped I/O port. This method uses the passed port `address`, takes the upper eight bits (that represent A8-A15), and scans them.

```csharp
public byte GetKeyLineStatus(ushort address)
{
    byte status = 0;
    var lines = (byte)~(address >> 8);
    for (var line = 0; line < 8; line++)
    {
        if ((lines & (1 << line)) != 0)
        {
            status |= _lineStatus[line];
        }
    }
    return (byte)~status;
}
```

> *Note*: In the physical keyboard, zero bits sign a pressed key state. However, within the `_lineStatus` array, bits set represent the pressed state. So, this is why the code complements the status bits in `GetKeyLineStatus`.
