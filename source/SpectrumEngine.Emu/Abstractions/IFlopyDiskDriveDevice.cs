using SpectrumEngine.Emu.Machines.FloppyDiskDrives.FloppyDisks;

namespace SpectrumEngine.Emu.Abstractions
{
    /// <summary>
    /// This interface defines the properties and operations of the ZX Spectrum's FDD (Flopy Disk Drive) device.
    /// </summary>    
    public interface IFlopyDiskDriveDevice
    {
        /// <summary>
        /// The current disk
        /// </summary>
        FloppyDisk Disk { get; set; }

        /// <summary>
        /// load a new disk into drive
        /// </summary>
        void LoadDisk(byte[] diskData);

        /// <summary>
        /// Ejects the current disk
        /// </summary>
        void EjectDisk();

        /// <summary>
        /// Signs whether is disk loaded
        /// </summary>   
        bool IsDiskLoaded { get; }
    }
}
