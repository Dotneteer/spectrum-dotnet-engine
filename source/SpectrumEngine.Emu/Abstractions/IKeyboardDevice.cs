namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the properties and operations of the ZX Spectrum's keyboard device.
/// </summary>
public interface IKeyboardDevice: IGenericDevice<IZxSpectrumMachine>
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
    /// Gets the value of the specified keyline
    /// </summary>
    /// <param name="line">Key line index</param>
    /// <returns>Key line value</returns>
    byte GetKeyLineValue(byte line);
    
    /// <summary>
    /// This method queries the status of the keyboard keys using the specified port address.
    /// </summary>
    /// <param name="address">Port address of the line to query</param>
    /// <returns>The data byte representing the keyboard status</returns>
    byte GetKeyLineStatus(ushort address);
}
