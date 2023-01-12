namespace SpectrumEngine.Emu.Abstractions
{
    /// <summary>
    /// Interface for devices with IN / OUT ports
    /// </summary>
    public interface IPortIODevice<TOut>
    {
        /// <summary>
        /// Read from device input port
        /// </summary>
        bool TryReadPort(ushort port, out TOut result);

        /// <summary>
        /// Write in device ouput port
        /// </summary>
        bool TryWritePort(ushort port, TOut value);
    }
}
