using SpectrumEngine.Emu.Machines.Disk.Controllers;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks.Formats;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace SpectrumEngine.Emu.Machines.Disk.FloppyDisks;

/// <summary>
/// Abstract class with logical floppy disk
/// </summary>
public abstract class FloppyDisk
{
    protected const int SectorHeaderSize = 8;
    protected const int TrackHeaderSize = 24;

    private static IDictionary<FloppyDiskFormat, string> _formatHeaders = new Dictionary<FloppyDiskFormat, string>
    {
        { FloppyDiskFormat.Cpc, "MV - CPC" },
        { FloppyDiskFormat.CpcExtended, "EXTENDED CPC DSK" },
    };

    protected FloppyDisk(byte[] data)
    {
        DiskData = new ReadOnlyCollection<byte>(data);

        ReadDiskHeader();

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
    public Header DiskHeader { get; } = new Header();

    /// <summary>
    /// Tracks
    /// </summary>
    public IList<Track>? DiskTracks { get; protected set; }

    /// <summary>
    /// Number of tracks per side
    /// </summary>
    public int SideTracksCount { get; protected set; }

    /// <summary>
    /// Number of bytes per track
    /// </summary>
    public int BytesPerTrack { get; protected set; }

    /// <summary>
    /// The number of physical sides
    /// </summary>
    public int SideCount { get; protected set; }

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

    /// <summary>
    /// Get the track count for the disk
    /// </summary>
    public virtual int GetTrackCount()
    {
        return DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides;
    }

    protected int GetTrackDataPointer(int trackIndex) => (trackIndex == 0 ? 0x100 : 0x100 + DiskHeader.TrackSizes[trackIndex]) + TrackHeaderSize;

    protected int GetSectorDataPointer(int trackIndex, int sectorIndex) => GetTrackDataPointer(trackIndex) + (sectorIndex * SectorHeaderSize);

    /// <summary>
    /// parse disk data 
    /// </summary>
    protected void ParseDisk()
    {
        ReadDiskHeader();

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

    protected virtual void ReadDiskHeader()
    {        
        byte[] data = DiskData.ToArray();
        DiskHeader.DiskIdentifier = Encoding.ASCII.GetString(data, 0, 16).ToUpper();
        DiskHeader.DiskCreatorString = Encoding.ASCII.GetString(data, 0x22, 14);
        DiskHeader.NumberOfTracks = DiskData[0x30];
        DiskHeader.NumberOfSides = DiskData[0x31];
        DiskHeader.TrackSizes = new int[DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides];
        DiskTracks = new Track[DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides];
        
    }

    protected virtual void ReadTracks()
    {
        int dataPointer = 0x34;

        // set track sizes
        for (int i = 0; i < DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides; i++)
        {
            DiskHeader.TrackSizes[i] = GetTrackSize(i);
        }

        // move to first track information block
        dataPointer = 0x100;

        byte[] data = DiskData.ToArray();
        // parse each track
        for (int trackIndex = 0; trackIndex < DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides; trackIndex++)
        {
            // check for unformatted track
            if (DiskHeader.TrackSizes[trackIndex] == 0)
            {
                DiskTracks[trackIndex] = new Track();
                DiskTracks[trackIndex].Sectors = new Sector[0];
                continue;
            }

            int trackDataPointer = dataPointer;
            DiskTracks[trackIndex] = new Track();

            // track info block
            DiskTracks[trackIndex].TrackIdentifier = Encoding.ASCII.GetString(data, trackDataPointer, 12);
            // trackDataPointer += 16;
            DiskTracks[trackIndex].TrackNumber = data[trackDataPointer + 16];
            DiskTracks[trackIndex].SideNumber = data[trackDataPointer + 17];
            DiskTracks[trackIndex].DataRate = data[trackDataPointer + 18];
            DiskTracks[trackIndex].RecordingMode = data[trackDataPointer + 19];
            DiskTracks[trackIndex].SectorSize = data[trackDataPointer + 20];
            DiskTracks[trackIndex].NumberOfSectors = data[trackDataPointer + 21];
            DiskTracks[trackIndex].GAP3Length = data[trackDataPointer + 22];
            DiskTracks[trackIndex].FillerByte = data[trackDataPointer + 23];

            int sectorDataPointer = dataPointer + 0x100;

            DiskTracks[trackIndex].Sectors = ReadSectors(trackIndex, trackDataPointer + TrackHeaderSize, sectorDataPointer);

            // move to the next track info block
            dataPointer += DiskHeader.TrackSizes[trackIndex];
        }
    }

    protected virtual Sector[] ReadSectors(int trackIndex, int trakDataPointer, int sectorDataPointer)
    {
        int numberSectors = DiskTracks[trackIndex].NumberOfSectors;

        // sector info list
        var sectors = new Sector[numberSectors];
        for (int sectorIndex = 0; sectorIndex < numberSectors; sectorIndex++)
        {
            sectors[sectorIndex] = ReadSector(trackIndex, trakDataPointer + (sectorIndex * SectorHeaderSize), sectorDataPointer, sectorIndex);

            // move sectorDataPointer to the next sector data postion
            sectorDataPointer += sectors[sectorIndex].ActualDataByteLength;
        }

        return sectors;
    }

    protected virtual Sector ReadSector(int trackIndex, int trakDataPointer, int sectorDataPointer0, int sectorIndex)
    {
        // var trackDataPointer = GetTrackDataPointer(trackIndex); // (trackIndex == 0 ? 0x100 : 0x100 + DiskHeader.TrackSizes[trackIndex]) + TrackHeaderSize;
        // var sectorDataPointer = GetSectorDataPointer(trackIndex, sectorIndex);// trakDataPointer + (sectorIndex * SectorHeaderSize);

        int sectorSize = GetSectorSize(trackIndex, sectorIndex);

        var sector = new Sector
        {
            TrackNumber = DiskData[trakDataPointer],
            SideNumber = DiskData[trakDataPointer + 1],
            SectorID = DiskData[trakDataPointer + 2],
            SectorSize = DiskData[trakDataPointer + 3],
            Status1 = DiskData[trakDataPointer + 4],
            Status2 = DiskData[trakDataPointer + 5],
            ActualDataByteLength = sectorSize,
            // sector data - begins at 0x100 offset from the start of the track info block
            SectorData = new byte[sectorSize],
        };

        sector.ContainsMultipleWeakSectors = HasMultipleWeakSectors(sector);

        // copy the data
        for (int i = 0; i < sector.ActualDataByteLength; i++)
        {
            sector.SectorData[i] = DiskData[sectorDataPointer0 + i];
        }

        return sector;
    }

    public class Header
    {
        public string DiskIdentifier { get; set; }
        public string DiskCreatorString { get; set; }
        public byte NumberOfTracks { get; set; }
        public byte NumberOfSides { get; set; }
        public int[] TrackSizes { get; set; }
    }

    public class Track
    {
        public string TrackIdentifier { get; set; }
        public byte TrackNumber { get; set; }
        public byte SideNumber { get; set; }
        public byte DataRate { get; set; }
        public byte RecordingMode { get; set; }
        public byte SectorSize { get; set; }
        public byte NumberOfSectors { get; set; }
        public byte GAP3Length { get; set; }
        public byte FillerByte { get; set; }
        public virtual Sector[] Sectors { get; set; }

        public virtual byte TrackType { get; set; }
        public virtual int TLEN { get; set; }
        public virtual int CLEN => TLEN / 8 + TLEN % 8 / 7 / 8;
        public virtual byte[]? TrackData { get; set; }

        /// <summary>
        /// Presents a contiguous byte array of all sector data for this track
        /// (including any multiple weak/random data)
        /// </summary>
        public virtual byte[] TrackSectorData
        {
            get
            {
                List<byte> list = new List<byte>();

                foreach (var sec in Sectors)
                {
                    list.AddRange(sec.ActualData);
                }

                return list.ToArray();
            }
        }
    }

    public class Sector
    {
        public virtual byte TrackNumber { get; set; }
        public virtual byte SideNumber { get; set; }
        public virtual byte SectorID { get; set; }
        public virtual byte SectorSize { get; set; }
        public virtual byte Status1 { get; set; }
        public virtual byte Status2 { get; set; }
        public virtual int ActualDataByteLength { get; set; }
        public virtual byte[]? SectorData { get; set; }
        public virtual bool ContainsMultipleWeakSectors { get; set; }

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


        public int RandSecCounter { get; private set; } = 0;

        public byte[] ActualData
        {
            get
            {
                if (!ContainsMultipleWeakSectors)
                {
                    // check whether filler bytes are needed
                    int size = 0x80 << SectorSize;
                    if (size > ActualDataByteLength)
                    {
                        List<byte> l = new List<byte>();
                        l.AddRange(SectorData);
                        for (int i = 0; i < size - ActualDataByteLength; i++)
                        {
                            //l.Add(SectorData[i]);
                            l.Add(SectorData.Last());
                        }

                        return l.ToArray();
                    }

                    return SectorData;
                }
                else
                {
                    // weak read neccessary
                    int copies = ActualDataByteLength / (0x80 << SectorSize);

                    // handle index wrap-around
                    if (WeakReadIndex > copies - 1)
                        WeakReadIndex = copies - 1;

                    // get the sector data based on the current weakreadindex
                    int step = WeakReadIndex * (0x80 << SectorSize);
                    byte[] res = new byte[(0x80 << SectorSize)];
                    Array.Copy(SectorData, step, res, 0, 0x80 << SectorSize);
                    return res;
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
