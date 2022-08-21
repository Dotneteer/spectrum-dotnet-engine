namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the properties and operations of the ZX Spectrum's floating bus device.
/// </summary>
public interface IFloatingBusDevice: IGenericDevice<IZxSpectrumMachine>
{
    /// <summary>
    /// Reads the current floating bus value.
    /// </summary>
    /// <returns></returns>
    byte ReadFloatingBus();
}
