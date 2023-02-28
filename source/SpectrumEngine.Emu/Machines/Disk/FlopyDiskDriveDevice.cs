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

    public int Id { get; private set; }
    
    public bool IsMotorRunning => _floppyDiskController.FlagMotor;

    public bool IsReady => IsDiskLoaded && (Disk?.DiskTracks) != null && Disk.DiskTracks.Any() && _floppyDiskController.FlagMotor;

    public bool IsWriteProtect => Disk?.IsWriteProtected ?? false;

    public FloppyDisk? Disk { get; private set; }

    public bool IsDiskLoaded => Disk != null;

    /// <summary>
    /// Seek status
    /// </summary>
    internal int SeekStatus { get; set; }

    /// <summary>
    /// Track to seeking (used in seek operations)
    /// </summary>
    internal int SeekingTrack { get; set; }

    /// <summary>
    /// Current track index in DiskTracks array
    /// </summary>
    internal byte TrackIndex { get; set; }

    /// <summary>
    /// Sector index in the Sectors array
    /// </summary>
    internal int SectorIndex { get; set; }

    /// <summary>
    /// Current cylinder track ID
    /// </summary>
    internal byte CurrentTrackId
    {
        get
        {
            // default invalid track
            var id = (byte)0xff;

            if (Disk?.DiskTracks == null || !Disk.DiskTracks.Any())
            {
                return id;
            }
            else if (TrackIndex >= Disk.DiskTracks.Count || TrackIndex < 0)
            {
                TrackIndex = 0;
            }

            return Disk.DiskTracks[TrackIndex].TrackNumber;
        }
        set
        {
            var track = Disk?.DiskTracks?.FirstOrDefault(item => item.TrackNumber == value);
            if (track != null)
            {
                TrackIndex = (byte)Disk.DiskTracks.IndexOf(track);
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
                //case FloppyDiskFormat.Cpc:
                //    fdd = new CpcFloppyDisk();
                //    found = fdd.TryParseDisk(diskData);
                //    break;
            }

            if (found)
            {
                Disk = fdd;
                break;
            }
        }

        if (!found)
        {
            throw new InvalidOperationException(Properties.Resources.UnknownImageFormatError);
        }
    }

    /// <summary>
    /// Ejects floppy disk
    /// </summary>
    public void EjectDisk() => Disk = null;
}
