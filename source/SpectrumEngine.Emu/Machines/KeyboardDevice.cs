namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum keyboard device.
/// </summary>
public sealed class KeyboardDevice: IKeyboardDevice
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
    public KeyboardDevice(IZxSpectrum48Machine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrum48Machine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        // --- Nothing to do
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
    /// This method queries the status of the keyboard keys using the specified port address.
    /// </summary>
    /// <param name="address">Port address of the line to query</param>
    /// <returns>The data byte representing the keyboard status</returns>
    public byte GetKeyLineStatus(ushort address)
    {
        byte status = 0;
        var lines = (byte)~(address >> 8);

        var lineIndex = 0;
        while (lines > 0)
        {
            if ((lines & 0x01) != 0)
            {
                status |= _lineStatus[lineIndex];
            }
            lineIndex++;
            lines >>= 1;
        }
        return (byte)~status;
    }
}
