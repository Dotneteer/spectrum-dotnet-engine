using SpectrumEngine.Emu.Machines.Disk.Controllers;
using System.Text;

namespace SpectrumEngine.Emu.Machines.Disk.FloppyDisks;

/// <summary>
/// Abstract class with logical floppy disk
/// </summary>
public abstract class FloppyDisk
{
    protected FloppyDisk()
    {
    }

    /// <summary>
    /// Disk format
    /// </summary>
    public abstract FloppyDiskFormat DiskFormat { get; }

    /// <summary>
    /// Attempts to parse disk data
    /// </summary>
    /// <returns>true if is disk parsed, false otherwise</returns>
    public abstract bool ParseDisk(byte[] diskData);

    /// <summary>
    /// Disk information header
    /// </summary>
    public Header DiskHeader { get; } = new Header();

    /// <summary>
    /// Tracks
    /// </summary>
    public Track[]? DiskTracks { get; private set; }

    /// <summary>
    /// Number of tracks per side
    /// </summary>
    public int SideTracksCount { get; private set; }

    /// <summary>
    /// Number of bytes per track
    /// </summary>
    public int BytesPerTrack { get; private set; }

    /// <summary>
    /// The number of physical sides
    /// </summary>
    public int SideCount { get; private set; }

    /// <summary>
    /// Signs whether is write-protect tab on the disk
    /// </summary>
    public bool IsWriteProtected { get; private set; }

    /// <summary>
    /// Disk image data
    /// </summary>
    public byte[]? DiskData { get; private set; }

    /// <summary>
    /// Get the track count for the disk
    /// </summary>
    public virtual int GetTrackCount()
    {
        return DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides;
    }


    public class Header
    {
        public string DiskIdent { get; set; }
        public string DiskCreatorString { get; set; }
        public byte NumberOfTracks { get; set; }
        public byte NumberOfSides { get; set; }
        public int[] TrackSizes { get; set; }
    }

    public class Track
    {
        public string TrackIdent { get; set; }
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

        public int WeakReadIndex { get; private set } = 0;

        public void SectorReadCompleted()
        {
            if (ContainsMultipleWeakSectors)
                WeakReadIndex++;
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

        public NecUpd765CommandParams SectorIDInfo =>
            new NecUpd765CommandParams
            {
                C = TrackNumber,
                H = SideNumber,
                R = SectorID,
                N = SectorSize,
                Flag1 = Status1,
                Flag2 = Status2,
            };
    }
}
