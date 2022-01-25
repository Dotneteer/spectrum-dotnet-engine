namespace SpectrumEngine.Emu;

/// <summary>
/// This interface represents the operations of a generic device and is intended to be the base interface of all device
/// definitions.
/// </summary>
public interface IGenericDevice
{
    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    void Reset();
}
