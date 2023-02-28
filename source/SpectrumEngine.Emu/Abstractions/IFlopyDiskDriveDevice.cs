using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;

namespace SpectrumEngine.Emu.Abstractions
{
    /// <summary>
    /// This interface defines the properties and operations of the ZX Spectrum's FDD (Flopy Disk Drive) device.
    /// </summary>    
    public interface IFlopyDiskDriveDevice
    {
        /// <summary>
        /// Device Id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// signs whether is motor running
        /// </summary>
        bool IsMotorRunning { get; }

        /// <summary>
        /// Signs whether is drive ready
        /// </summary>
        bool IsReady { get; }

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
