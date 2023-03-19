using SpectrumEngine.Emu.Extensions;

namespace SpectrumEngine.Emu.Machines.Disk.FloppyDisks.Formats;

/// <summary>
/// Logical object representing a standard +3 disk image
/// </summary>
public class CpcExtendedFloppyDisk : FloppyDisk
{
    /// <summary>
    /// The format type
    /// </summary>
    public override FloppyDiskFormat DiskFormat => FloppyDiskFormat.CpcExtended;

    internal CpcExtendedFloppyDisk(byte[] data) : base(data) { }

    protected override int GetTrackSize(int trackIndex) => DiskData[0x34 + trackIndex] * 256;

    protected override int GetSectorSize(int trackIndex, int sectorIndex)
    {
        var sectorDataPointer = GetSectorDataPointer(trackIndex, sectorIndex);
        return DiskData.GetWordValue(sectorDataPointer + 6);
    }

    protected override bool HasMultipleWeakSectors(Sector sector)
    {
        var result = false;

        // check for multiple weak/random sectors stored
        if (sector.SectorSize <= 7)
        {
            // sectorsize n=8 is equivilent to n=0 - FDC will use DTL for length
            int specifiedSize = 0x80 << sector.SectorSize;

            if (specifiedSize < sector.ActualDataByteLength)
            {
                // more data stored than sectorsize defines
                // check for multiple weak/random copies
                if (sector.ActualDataByteLength % specifiedSize != 0)
                {
                    result = true;
                }
            }
        }

        return result;
    }
}
