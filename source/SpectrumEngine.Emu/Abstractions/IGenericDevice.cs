namespace SpectrumEngine.Emu;

/// <summary>
/// This interface represents the operations of a generic device and is intended to be the base interface of all device
/// definitions.
/// </summary>
public interface IGenericDevice<out TMachine> : IDisposable 
    where TMachine : IZ80Machine
{
    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    TMachine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    void Reset();
}
