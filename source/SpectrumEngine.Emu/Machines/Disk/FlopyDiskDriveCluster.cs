using SpectrumEngine.Emu.Abstractions;
using SpectrumEngine.Emu.Machines.Disk.Controllers;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;

namespace SpectrumEngine.Emu.Machines.Disk;

/// <summary>
/// Floppy disk drive cluster
/// </summary>
public class FlopyDiskDriveCluster
{
    private const int _floppyDiskDrivesUsed = 2;
    private readonly NecUpd765 _floppyDiskController;
    private readonly IFlopyDiskDriveDevice[] _floppyDiskDrives = new FlopyDiskDriveDevice[_floppyDiskDrivesUsed];
    private readonly IZxSpectrumMachine _machine;

    private int _floppyDiskDriveSlot = 0;

    public FlopyDiskDriveCluster(IZxSpectrumMachine machine)
    {
        _machine = machine;
        _floppyDiskController = new NecUpd765(this);

        Reset();
    }

    /// <summary>
    /// Signs whether is motor running 
    /// </summary>
    public bool IsMotorRunning => _floppyDiskController.FlagMotor;

    /// <summary>
    /// Signs whether is disk inserted in active drive slot
    /// </summary>
    public bool IsFloppyDiskLoaded => ActiveFloppyDiskDrive != null && ActiveFloppyDiskDrive.IsDiskLoaded;

    /// <summary>
    /// Active floppy disk
    /// </summary>
    public FloppyDisk? FloppyDisk => ActiveFloppyDiskDrive?.Disk;

    /// <summary>
    /// Try read from FDC (Floppy disk controller) port
    /// </summary>
    /// <param name="port">port number to try read</param>
    /// <param name="data">byte read if can read, 0 otherwise</param>
    /// <returns>true if can read port, false otherwise</returns>
    public bool TryReadPort(ushort port, out byte data) => _floppyDiskController.TryReadPort(port, out data);

    /// <summary>
    /// Try write in FDC (Floppy disk controller) port
    /// </summary>
    /// <param name="port">port number to try write</param>
    /// <param name="data">byte to write</param>
    /// <returns>true if can write port, false otherwise</returns>
    public bool TryWritePort(ushort port, byte data) => _floppyDiskController.TryWritePort(port, data);

    /// <summary>
    /// Currently active floppy drive device
    /// </summary>
    public IFlopyDiskDriveDevice? ActiveFloppyDiskDrive { get; private set; }

    /// <summary>
    /// Active floppy disk drive slot
    /// </summary>
    public int FloppyDiskDriveSlot
    {
        get => _floppyDiskDriveSlot;
        internal set
        {
            _floppyDiskDriveSlot = value;
            ActiveFloppyDiskDrive = _floppyDiskDrives[_floppyDiskDriveSlot];
        }
    }

    /// <summary>
    /// Load floppy disk data in active slot
    /// </summary>
    /// <exception cref="InvalidOperationException">Invalid disk data</exception>
    public void LoadDisk(byte[] diskData) => ActiveFloppyDiskDrive?.LoadDisk(diskData);

    /// <summary>
    /// Ejects floppy disk from active slot
    /// </summary>
    public void EjectDisk() => ActiveFloppyDiskDrive?.EjectDisk();

    /// <summary>
    /// Initialization / reset of the floppy drive subsystem
    /// </summary>
    private void Reset()
    {
        for (int i = 0; i < _floppyDiskDrivesUsed; i++)
        {
            _floppyDiskDrives[i] = new FlopyDiskDriveDevice(i, _floppyDiskController);
        }

        FloppyDiskDriveSlot = 0;
    }   
}
