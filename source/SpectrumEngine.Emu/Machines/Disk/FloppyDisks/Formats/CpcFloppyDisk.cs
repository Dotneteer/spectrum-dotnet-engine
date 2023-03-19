using SpectrumEngine.Emu.Extensions;

namespace SpectrumEngine.Emu.Machines.Disk.FloppyDisks.Formats;

/// <summary>
/// Logical object representing a standard +3 disk image
/// </summary>
public class CpcFloppyDisk : FloppyDisk
{
    /// <summary>
    /// The format type
    /// </summary>
    public override FloppyDiskFormat DiskFormat => FloppyDiskFormat.Cpc;

    internal CpcFloppyDisk(byte[] data) : base(data) { }

    protected override int GetTrackSize(int trackIndex) => DiskData.GetWordValue(0x32);

    protected override int GetSectorSize(int trackIndex, int sectorIndex)
    {
        var sectorPointer = GetSectorPointer(trackIndex, sectorIndex);
        var sectorSize = DiskData[sectorPointer + 3];

        return sectorSize switch
        {
            // no sectorsize specified - or invalid
            0 or > 6 => DiskHeader.TrackSizes[trackIndex],
            // only 0x1800 bytes are stored
            6 => 0x1800,
            // valid sector size for this format
            _ => 0x80 << sectorSize,
        };
    }

    protected override bool HasMultipleWeakSectors(Sector sector) => false;
}
