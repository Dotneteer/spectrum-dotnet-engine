namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the properties and operations of the ZX Spectrum's floating bus device.
/// </summary>
public interface IFloatingBusDevice: IGenericDevice<IZxSpectrum48Machine>
{
    /// <summary>
    /// Reads the current floating bus value.
    /// </summary>
    /// <returns></returns>
    byte ReadFloatingPort();
}
