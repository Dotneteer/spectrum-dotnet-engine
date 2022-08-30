using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;

namespace SpectrumEngine.Emu.Abstractions
{
    /// <summary>
    /// This interface defines the properties and operations of the ZX Spectrum's FDD (Flopy Disk Drive) device.
    /// </summary>    
    public interface IFlopyDiskDriveDevice
    {
        /// <summary>
        /// Loaded floppy disk
        /// </summary>
        FloppyDisk? Disk { get; }

        /// <summary>
        /// Signs whether is disk loaded
        /// </summary>   
        bool IsDiskLoaded { get; }

        /// <summary>
        /// Load floppy disk data
        /// </summary>
        /// <exception cref="InvalidOperationException">Invalid disk data</exception>
        void LoadDisk(byte[] diskData);

        /// <summary>
        /// Ejects floppy disk
        /// </summary>
        void EjectDisk();
    }
}
