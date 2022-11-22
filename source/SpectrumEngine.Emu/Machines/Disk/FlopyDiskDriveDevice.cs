using SpectrumEngine.Emu.Abstractions;
using SpectrumEngine.Emu.Machines.Disk.Controllers;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks.Formats;

namespace SpectrumEngine.Emu.Machines.Disk;

/// <summary>
/// Floppy disk drive device
/// </summary>
public class FlopyDiskDriveDevice : IFlopyDiskDriveDevice
{
    private readonly NecUpd765 _floppyDiskController;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="driveId">Floppy disk Identifier</param>
    /// <param name="floppyDiskController">Floppy disk controller</param>
    public FlopyDiskDriveDevice(int driveId, NecUpd765 floppyDiskController)
    {
        Id = driveId;
        _floppyDiskController = floppyDiskController;
    }

    /// <summary>
    /// The floppy disk drive device ID
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// signs whether is motor running
    /// </summary>
    public bool IsMotorRunning => _floppyDiskController.FlagMotor;

    /// <summary>
    /// Signs whether is drive ready
    /// </summary>
    public bool IsReady
    {
        get
        {
            if (!IsDiskLoaded || Disk?.GetTrackCount() == 0 || !_floppyDiskController.FlagMotor)
                return false;
            else
                return true;
        }
    }

    /// <summary>
    /// Signs whether is disk write protected
    /// </summary>
    public bool IsWriteProtect { get; set; } = false;

    /// <summary>
    /// Counter for seek steps
    /// One step for each index pulse (track index) until seeked track
    /// </summary>
    public int SeekCounter { get; set; }

    /// <summary>
    /// Seek status
    /// </summary>
    public int SeekStatus { get; set; }

    /// <summary>
    /// Age counter
    /// </summary>
    public int SeekAge { get; set; }

    /// <summary>
    /// Track to seeking (used in seek operations)
    /// </summary>
    public int SeekingTrack { get; set; }

    /// <summary>
    /// Current disk side
    /// </summary>
    public byte CurrentSide { get; set; }

    /// <summary>
    /// Current track index in DiskTracks array
    /// </summary>
    public byte TrackIndex { get; set; }

    /// <summary>
    /// Sector index in the Sectors array
    /// </summary>
    public int SectorIndex { get; set; }

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

            if (Disk == null || Disk.DiskTracks == null)
            {
                return (byte)id;
            }
            else if (Disk.DiskTracks.Length == 0)
            {
                return (byte)id;
            }
            else if (TrackIndex >= Disk.GetTrackCount())
            {
                TrackIndex = 0;
            }
            else if (TrackIndex < 0)
            {
                TrackIndex = 0;
            }

            var track = Disk.DiskTracks[TrackIndex];

            id = track.TrackNumber;

            return (byte)id;
        }
        set
        {
            if (Disk == null || Disk.DiskTracks == null)
            {
                return;
            }

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
        FloppyDisk? fdd = null;
        bool found = false;

        foreach (FloppyDiskFormat type in Enum.GetValues(typeof(FloppyDiskFormat)))
        {
            switch (type)
            {
                case FloppyDiskFormat.CpcExtended:
                    fdd = new CpcExtendedFloppyDisk();
                    found = fdd.ParseDisk(diskData);
                    break;
                case FloppyDiskFormat.Cpc:
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
            throw new InvalidOperationException($"{GetType()}{Environment.NewLine}Disk image file could not be parsed. Unknown format.");
        }
    }

    /// <summary>
    /// Ejects floppy disk
    /// </summary>
    public void EjectDisk() => Disk = null;
}
