namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum keyboard device.
/// </summary>
public sealed class KeyboardDevice: IKeyboardDevice, IDisposable
{
    /// <summary>
    /// This field stores the status bits of keys. Each byte in the array represents an address line from A8 to A15, 
    /// and the lower five bits represent the five keys associated with the particular address line. One means the key
    /// is pressed; zero stands for an unpressed key.
    /// </summary>
    private readonly byte[] _lineStatus = new byte[8];

    /// <summary>
    /// Initialize the keyboard device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public KeyboardDevice(IZxSpectrumMachine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Release resources
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrumMachine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        for (var i = 0; i < 8; i++) _lineStatus[i] = 0;
    }

    /// <summary>
    /// Set the status of the specified ZX Spectrum key.
    /// </summary>
    /// <param name="key">Key code</param>
    /// <param name="isDown">Indicates if the key is pressed down.</param>
    public void SetStatus(SpectrumKeyCode key, bool isDown)
    {
        var lineIndex = (byte)key / 5;
        var lineMask = 1 << (byte)key % 5;
        _lineStatus[lineIndex] = isDown
            ? (byte)(_lineStatus[lineIndex] | lineMask)
            : (byte)(_lineStatus[lineIndex] & ~lineMask);
    }

    /// <summary>
    /// Get the status of the specified Spectrum keyboard key.
    /// </summary>
    /// <param name="key">Key code</param>
    /// <returns>True, if the key is down; otherwise, false</returns>
    public bool GetStatus(SpectrumKeyCode key)
    {
        var lineIndex = (byte)key / 5;
        var lineMask = 1 << (byte)key % 5;
        return (_lineStatus[lineIndex] & lineMask) != 0;
    }

    /// <summary>
    /// Gets the value of the specified keyline
    /// </summary>
    /// <param name="line">Key line index</param>
    /// <returns>Key line value</returns>
    public byte GetKeyLineValue(byte line)
    {
        return _lineStatus[line & 0x07];
    }

    /// <summary>
    /// This method queries the status of the keyboard keys using the specified port address.
    /// </summary>
    /// <param name="address">Port address of the line to query</param>
    /// <returns>The data byte representing the keyboard status</returns>
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
}
