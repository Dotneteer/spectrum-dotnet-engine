using SpectrumEngine.Emu.Abstractions;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;

namespace SpectrumEngine.Emu.Machines.Disk;

/// <summary>
/// Floppy disk drive device
/// </summary>
public class FlopyDiskDriveDevice : IFlopyDiskDriveDevice
{
    private readonly NECUPD765 _floppyDiskController;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="driveId">Floppy disk Identifier</param>
    /// <param name="floppyDiskController">Floppy disk controller</param>
    public FlopyDiskDriveDevice(int driveId, NECUPD765 floppyDiskController)
    {
        Id = driveId;
        _floppyDiskController = floppyDiskController;
    }

    /// <summary>
    /// Signs whether is motor running 
    /// </summary>
    public bool IsMotorRunning { get; private set; }

    /// <summary>
    /// The floppy disk drive device ID
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Signs whether is drive ready
    /// </summary>
    public bool IsReady
    {
        get
        {
            if (!IsDiskLoaded || Disk.GetTrackCount() == 0 || !_floppyDiskController.FDD_FLAG_MOTOR)
                return false;
            else
                return true;
        }
    }

    /// <summary>
    /// Signs whether is disk write protected
    /// </summary>
    public bool IsWriteProtect { get; private set; } = false;

    /// <summary>
    /// Counter for seek steps
    /// One step for each index pulse (track index) until seeked track
    /// </summary>
    public int SeekCounter { get; private set; }

    /// <summary>
    /// Seek status
    /// </summary>
    public int SeekStatus { get; private set; }

    /// <summary>
    /// Age counter
    /// </summary>
    public int SeekAge { get; private set; }

    /// <summary>
    /// Track to seeking (used in seek operations)
    /// </summary>
    public int SeekingTrack { get; private set; }

    /// <summary>
    /// Current disk side
    /// </summary>
    public byte CurrentSide { get; private set; }

    /// <summary>
    /// Current track index in DiskTracks array
    /// </summary>
    public byte TrackIndex { get; private set; }

    /// <summary>
    /// Sector index in the Sectors array
    /// </summary>
    public int SectorIndex { get; private set; }

    /// <summary>
    /// Loaded floppy disk
    /// </summary>
    public FloppyDisk? Disk { get; private set; }

    /// <summary>
    /// Signs whether is disk loaded
    /// </summary>        
    public bool IsDiskLoaded => Disk != null;

    /// <summary>
    /// Current cylinder track ID
    /// </summary>
    public byte CurrentTrackId
    {
        get
        {
            // default invalid track
            int id = 0xff;

            if (Disk == null)
                return (byte)id;

            if (Disk.DiskTracks.Length == 0)
                return (byte)id;

            if (TrackIndex >= Disk.GetTrackCount())
                TrackIndex = 0;
            else if (TrackIndex < 0)
                TrackIndex = 0;

            var track = Disk.DiskTracks[TrackIndex];

            id = track.TrackNumber;

            return (byte)id;
        }
        set
        {
            for (int i = 0; i < Disk.GetTrackCount(); i++)
            {
                if (Disk.DiskTracks[i].TrackNumber == value)
                {
                    TrackIndex = (byte)i;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Load floppy disk data
    /// </summary>
    /// <exception cref="InvalidOperationException">Invalid disk data</exception>
    public void LoadDisk(byte[] diskData)
    {
        // try dsk first
        FloppyDisk fdd = null;
        bool found = false;

        foreach (FloppyDiskFormat type in Enum.GetValues(typeof(FloppyDiskFormat)))
        {
            switch (type)
            {
                case FloppyDiskFormat.CPCExtended:
                    fdd = new CpcExtendedFloppyDisk();
                    found = fdd.ParseDisk(diskData);
                    break;
                case FloppyDiskFormat.CPC:
                    fdd = new CpcFloppyDisk();
                    found = fdd.ParseDisk(diskData);
                    break;
            }

            if (found)
            {
                Disk = fdd;
                break;
            }
        }

        if (!found)
        {
            throw new InvalidOperationException($"{this.GetType()}{Environment.NewLine}Disk image file could not be parsed. Unknown format.");
        }
    }

    /// <summary>
    /// Ejects floppy disk
    /// </summary>
    public void EjectDisk() => Disk = null;
}
