namespace SpectrumEngine.Emu.Abstractions
{
    /// <summary>
    /// Interface for devices with IN / OUT ports
    /// </summary>
    public interface IPortIODevice
    {
        /// <summary>
        /// Read from device input port
        /// </summary>
        bool ReadPort(ushort port, out int result);

        /// <summary>
        /// Write in device ouput port
        /// </summary>
        bool WritePort(ushort port, int value);
    }
}
