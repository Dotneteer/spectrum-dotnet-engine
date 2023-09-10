using SpectrumEngine.Emu.Machines.Disk.Controllers;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks.Formats;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Text;

namespace SpectrumEngine.Emu.Machines.Disk.FloppyDisks;

/// <summary>
/// Abstract class with logical floppy disk
/// </summary>
public abstract class FloppyDisk
{
    protected const int SectorHeaderSize = 8;
    protected const int TrackHeaderSize = 24;
    protected const int StartDiskTrackPointer = 0x100;
    protected const int StartDiskSectorPointer = 0x100;

    private static IDictionary<FloppyDiskFormat, string> _formatHeaders = new Dictionary<FloppyDiskFormat, string>
    {
        { FloppyDiskFormat.Cpc, "MV - CPC" },
        { FloppyDiskFormat.CpcExtended, "EXTENDED CPC DSK" },
    };

    protected FloppyDisk(byte[] data)
    {
        DiskData = new ReadOnlyCollection<byte>(data);

        DiskHeader = ReadDiskHeader(data);        
        DiskTracks = new List<Track>();

        ParseDisk();
    }

    public static bool TryCreateDisk(byte[] data, out FloppyDisk? dik)
    {
        // look for standard magic string
        string ident = Encoding.ASCII.GetString(data, 0, 16).ToUpper();

        var format = _formatHeaders.FirstOrDefault(item => ident.Contains(item.Value)).Key;

        dik = format switch
        {
            FloppyDiskFormat.Cpc => new CpcFloppyDisk(data),
            // only 0x1800 bytes are stored
            FloppyDiskFormat.CpcExtended => new CpcExtendedFloppyDisk(data),
            // valid sector size for this format
            _ => null
        };

        return dik != null;
    }

    /// <summary>
    /// Disk format
    /// </summary>
    public abstract FloppyDiskFormat DiskFormat { get; }

    /// <summary>
    /// Disk information header
    /// </summary>
    public Header DiskHeader { get; private set; }

    /// <summary>
    /// Tracks
    /// </summary>
    public IList<Track> DiskTracks { get; protected set; }

    /// <summary>
    /// Signs whether is write-protect tab on the disk
    /// </summary>
    public bool IsWriteProtected { get; protected set; }

    /// <summary>
    /// Disk image data
    /// </summary>
    public ReadOnlyCollection<byte> DiskData { get; protected set; }

    protected abstract int GetTrackSize(int trackIndex);

    protected abstract int GetSectorSize(int trackIndex, int sectorIndex);

    protected abstract bool HasMultipleWeakSectors(Sector sector);

    protected int GetTrackPointer(int trackIndex) => StartDiskTrackPointer + (trackIndex == 0 ? 0 : DiskHeader.TrackSizes.Take(trackIndex).Sum());
    protected int GetEndTrackHeaderPointer(int trackIndex) => GetTrackPointer(trackIndex) + TrackHeaderSize;

    protected int GetSectorPointer(int trackIndex, int sectorIndex) => GetEndTrackHeaderPointer(trackIndex) + (sectorIndex * SectorHeaderSize);
    protected int GetSectorDataPointer(int trackIndex, int sectorIndex)
    {
        var startSectorPointer = StartDiskSectorPointer + (sectorIndex == 0 ? 0 : DiskTracks[trackIndex].Sectors.Take(sectorIndex).Sum(item => item.ActualDataByteLength));
        var startTrackPointer = GetTrackPointer(trackIndex);

        return startTrackPointer + startSectorPointer;
    }

    /// <summary>
    /// Get the track count for the disk
    /// </summary>
    public virtual int GetTrackCount()
    {
        return DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides;
    }

    /// <summary>
    /// parse disk data 
    /// </summary>
    protected void ParseDisk()
    {
        if (DiskHeader.NumberOfSides > 1)
        {
            throw new NotImplementedException(Properties.Resources.InvalidMultiSideImageFormatError);
        }
        else if (DiskHeader.NumberOfTracks > 42)
        {
            throw new NotImplementedException(Properties.Resources.InvalidImageTracksFormatError);
        }

        ReadTracks();

        // protection scheme detector
        // TODO: implement
    }

    protected virtual Header ReadDiskHeader(byte[] data)
    {
        var numberOfTracks = data[0x30];
        var numberOfSides = data[0x31];

        return new Header
        {
            DiskIdentifier = Encoding.ASCII.GetString(data, 0, 16).ToUpper(),
            DiskCreatorString = Encoding.ASCII.GetString(data, 0x22, 14),
            NumberOfTracks = numberOfTracks,
            NumberOfSides = numberOfSides,           
        };
    }

    protected virtual Track ReadTrackHeader(byte[] data, int trackPointer) => new()
    {
        TrackIdentifier = Encoding.ASCII.GetString(data, trackPointer, 12),
        TrackNumber = data[trackPointer + 16],
        SideNumber = data[trackPointer + 17],
        DataRate = data[trackPointer + 18],
        RecordingMode = data[trackPointer + 19],
        SectorSize = data[trackPointer + 20],
        NumberOfSectors = data[trackPointer + 21],
        GAP3Length = data[trackPointer + 22],
        FillerByte = data[trackPointer + 23],
    };

    protected virtual void ReadTracks()
    {
        // set track sizes
        for (int i = 0; i < DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides; i++)
        {
            DiskHeader.TrackSizes.Add(GetTrackSize(i));
        }

        byte[] data = DiskData.ToArray();
        // parse each track
        for (int trackIndex = 0; trackIndex < DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides; trackIndex++)
        {
            // check for unformatted track
            if (DiskHeader.TrackSizes[trackIndex] == 0)
            {
                DiskTracks.Add(new Track());
                continue;
            }

            int trackPointer = GetTrackPointer(trackIndex);

            DiskTracks.Add(ReadTrackHeader(data, trackPointer));

            // add sectors
            for (int sectorIndex = 0; sectorIndex < DiskTracks[trackIndex].NumberOfSectors; sectorIndex++)
            {
                DiskTracks[trackIndex].Sectors.Add(ReadSector(trackIndex, sectorIndex));
            }
        }
    }

    protected virtual Sector ReadSector(int trackIndex, int sectorIndex)
    {
        var sectorPointer = GetSectorPointer(trackIndex, sectorIndex);
        int sectorSize = GetSectorSize(trackIndex, sectorIndex);

        var sector = new Sector
        {
            TrackNumber = DiskData[sectorPointer],
            SideNumber = DiskData[sectorPointer + 1],
            SectorID = DiskData[sectorPointer + 2],
            SectorSize = DiskData[sectorPointer + 3],
            Status1 = DiskData[sectorPointer + 4],
            Status2 = DiskData[sectorPointer + 5],
            ActualDataByteLength = sectorSize,
            // sector data - begins at 0x100 offset from the start of the track info block
            SectorData = new byte[sectorSize],
        };

        sector.ContainsMultipleWeakSectors = HasMultipleWeakSectors(sector);

        // copy the data
        var sectorDataPointer = GetSectorDataPointer(trackIndex, sectorIndex);
        for (int i = 0; i < sector.ActualDataByteLength; i++)
        {
            sector.SectorData[i] = DiskData[sectorDataPointer + i];
        }

        return sector;
    }

    public record Header
    {
        public string DiskIdentifier { get; init; } = default!;
        public string DiskCreatorString { get; init; } = default!;
        public byte NumberOfTracks { get; init; } = default!;
        public byte NumberOfSides { get; init; } = default!;
        public IList<int> TrackSizes { get; init; } = new List<int>();
    }

    public class Track
    {
        public string TrackIdentifier { get; init; } = default!;
        public byte TrackNumber { get; init; } = default!;
        public byte SideNumber { get; init; } = default!;
        public byte DataRate { get; init; } = default!;
        public byte RecordingMode { get; init; } = default!;
        public byte SectorSize { get; init; } = default!;
        public byte NumberOfSectors { get; init; } = default!;
        public byte GAP3Length { get; init; } = default!;
        public byte FillerByte { get; init; } = default!;
        public IList<Sector> Sectors { get; init; } = new List<Sector>();
    }

    public class Sector
    {
        public byte TrackNumber { get; set; }
        public byte SideNumber { get; set; }
        public byte SectorID { get; set; }
        public byte SectorSize { get; set; }
        public byte Status1 { get; set; }
        public byte Status2 { get; set; }
        public int ActualDataByteLength { get; set; }
        public IList<byte> SectorData { get; set; }
        public bool ContainsMultipleWeakSectors { get; set; }

        public int WeakReadIndex { get; private set; } = 0;

        public void SectorReadCompleted()
        {
            if (ContainsMultipleWeakSectors)
            {
                WeakReadIndex++;
            }
        }

        public int DataLen
        {
            get
            {
                if (!ContainsMultipleWeakSectors)
                {
                    return ActualDataByteLength;
                }

                return ActualDataByteLength / (ActualDataByteLength / (0x80 << SectorSize));
            }
        }

        public IList<byte> ActualData
        {
            get
            {
                if (!ContainsMultipleWeakSectors)
                {
                    // check whether filler bytes are needed
                    int size = 0x80 << SectorSize;
                    if (size > ActualDataByteLength)
                    {
                        var result = new List<byte>(SectorData);
                        for (int i = 0; i < size - ActualDataByteLength; i++)
                        {
                            result.Add(SectorData.Last());
                        }

                        return result;
                    }

                    return SectorData;
                }
                else
                {
                    // weak read neccessary
                    int copies = ActualDataByteLength / (0x80 << SectorSize);

                    // handle index wrap-around
                    if (WeakReadIndex > copies - 1)
                    {
                        WeakReadIndex = copies - 1;
                    }

                    // get the sector data based on the current weakreadindex
                    int step = WeakReadIndex * (0x80 << SectorSize);
                    byte[] result = new byte[(0x80 << SectorSize)];
                    Array.Copy(SectorData.ToArray(), step, result, 0, 0x80 << SectorSize);
                    return result;
                }
            }
        }

        public Command SectorIDInfo =>
            new()
            {
                C = TrackNumber,
                H = SideNumber,
                R = SectorID,
                N = SectorSize,
                St1 = Status1,
                St2 = Status2,
            };
    }
}
