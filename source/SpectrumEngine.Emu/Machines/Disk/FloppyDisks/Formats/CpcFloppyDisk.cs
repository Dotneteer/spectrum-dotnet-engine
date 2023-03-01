using System.Text;
using System;
using System.Collections.Generic;
using SpectrumEngine.Emu.Extensions;
using System.ComponentModel.DataAnnotations;
using static SpectrumEngine.Emu.Machines.Disk.FloppyDisks.FloppyDisk;

namespace SpectrumEngine.Emu.Machines.Disk.FloppyDisks.Formats;

/// <summary>
/// Logical object representing a standard +3 disk image
/// </summary>
public class CpcFloppyDisk : FloppyDisk
{
    /// <summary>
    /// The format type
    /// </summary>
    public override FloppyDiskFormat DiskFormat => FloppyDisks.FloppyDiskFormat.Cpc;

    /// <summary>
    /// Attempts to parse incoming disk data 
    /// </summary>
    /// <returns>
    /// TRUE:   disk parsed
    /// FALSE:  unable to parse disk
    /// </returns>
    public override bool ParseDisk(byte[] data)
    {
        // look for standard magic string
        string ident = Encoding.ASCII.GetString(data, 0, 16);

        if (!ident.ToUpper().Contains("MV - CPC"))
        {
            // incorrect format
            return false;
        }

        // read the disk information block
        DiskHeader.DiskIdent = ident;
        DiskHeader.DiskCreatorString = Encoding.ASCII.GetString(data, 0x22, 14);
        DiskHeader.NumberOfTracks = data[0x30];
        DiskHeader.NumberOfSides = data[0x31];
        DiskHeader.TrackSizes = new int[DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides];
        DiskTracks = new Track[DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides];
        DiskData = data;
        int dataPointer = 0x32;

        if (DiskHeader.NumberOfSides > 1)
        {
            throw new NotImplementedException(Properties.Resources.InvalidMultiSideImageFormatError);
        }
        else if (DiskHeader.NumberOfTracks > 42)
        {
            throw new NotImplementedException(Properties.Resources.InvalidImageTracksFormatError);
        }

        // standard CPC format all track sizes are the same in the image
        for (int i = 0; i < DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides; i++)
        {
            DiskHeader.TrackSizes[i] = data.GetWordValue(dataPointer);
        }

        // move to first track information block
        dataPointer = 0x100;

        // parse each track
        for (int i = 0; i < DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides; i++)
        {
            // check for unformatted track
            if (DiskHeader.TrackSizes[i] == 0)
            {
                DiskTracks[i] = new Track();
                DiskTracks[i].Sectors = new Sector[0];
                continue;
            }

            int trakDataPointer = dataPointer;
            DiskTracks[i] = new Track();

            // track info block
            DiskTracks[i].TrackIdent = Encoding.ASCII.GetString(data, trakDataPointer, 12);
            trakDataPointer += 16;
            DiskTracks[i].TrackNumber = data[trakDataPointer++];
            DiskTracks[i].SideNumber = data[trakDataPointer++];
            trakDataPointer += 2;
            DiskTracks[i].SectorSize = data[trakDataPointer++];
            DiskTracks[i].NumberOfSectors = data[trakDataPointer++];
            DiskTracks[i].GAP3Length = data[trakDataPointer++];
            DiskTracks[i].FillerByte = data[trakDataPointer++];

            int sectorDataPointer = dataPointer + 0x100;

            // sector info list
            DiskTracks[i].Sectors = new Sector[DiskTracks[i].NumberOfSectors];
            for (int s = 0; s < DiskTracks[i].NumberOfSectors; s++)
            {
                DiskTracks[i].Sectors[s] = GetSector(data, i, trakDataPointer, ref sectorDataPointer);

                trakDataPointer += 8;

                // move sectorDataPointer to the next sector data postion
                sectorDataPointer += DiskTracks[i].Sectors[s].ActualDataByteLength;
            }

            // move to the next track info block
            dataPointer += DiskHeader.TrackSizes[i];
        }

        // protection scheme detector
        // TODO: implement

        return true;
    }

    private Sector GetSector(byte[] data, int trakIndex, int trakDataPointer, ref int sectorDataPointer)
    {
        int sectorSize = GetSectorSize(trakIndex, data[trakDataPointer + 3]);

        var sector = new Sector
        {
            TrackNumber = data[trakDataPointer],
            SideNumber = data[trakDataPointer + 1],
            SectorID = data[trakDataPointer + 2],           
            SectorSize = data[trakDataPointer + 3],
            Status1 = data[trakDataPointer + 4],
            Status2 = data[trakDataPointer + 5],
            ActualDataByteLength = sectorSize,
            // sector data - begins at 0x100 offset from the start of the track info block
            SectorData = new byte[sectorSize],
        };

        // copy the data
        for (int i = 0; i < sector.ActualDataByteLength; i++)
        {
            sector.SectorData[i] = data[sectorDataPointer + i];
        }

        return sector;
    }

    private int GetSectorSize(int trakIndex, int sectorSize) => sectorSize switch
    {
        // no sectorsize specified - or invalid
        0 or > 6 => DiskHeader.TrackSizes[trakIndex],
        // only 0x1800 bytes are stored
        6 => 0x1800,
        // valid sector size for this format
        _ => 0x80 << sectorSize,
    };
}
