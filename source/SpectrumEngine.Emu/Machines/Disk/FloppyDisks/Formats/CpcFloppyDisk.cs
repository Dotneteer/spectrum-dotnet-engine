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
    private int _sectorHeaderSize = 8;
    private int _trackIndex;

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
    public override bool TryParseDisk(byte[] data)
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

        // set track sizes, standard CPC format all track sizes are the same in the image
        for (int i = 0; i < DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides; i++)
        {
            DiskHeader.TrackSizes[i] = data.GetWordValue(dataPointer);
        }

        // move to first track information block
        dataPointer = 0x100;

        GetTracks(data, dataPointer);

        // protection scheme detector
        // TODO: implement

        return true;
    }

    private int GetTracks(byte[] data, int dataPointer)
    {
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
            DiskTracks[trackIndex].TrackIdent = Encoding.ASCII.GetString(data, trackDataPointer, 12);
            trackDataPointer += 16;
            DiskTracks[trackIndex].TrackNumber = data[trackDataPointer++];
            DiskTracks[trackIndex].SideNumber = data[trackDataPointer++];
            trackDataPointer += 2;
            DiskTracks[trackIndex].SectorSize = data[trackDataPointer++];
            DiskTracks[trackIndex].NumberOfSectors = data[trackDataPointer++];
            DiskTracks[trackIndex].GAP3Length = data[trackDataPointer++];
            DiskTracks[trackIndex].FillerByte = data[trackDataPointer++];

            int sectorDataPointer = dataPointer + 0x100;

            DiskTracks[trackIndex].Sectors = GetSectors(data, trackIndex, DiskTracks[trackIndex].NumberOfSectors, trackDataPointer, ref sectorDataPointer);

            trackDataPointer += _sectorHeaderSize * DiskTracks[trackIndex].NumberOfSectors;

            // move to the next track info block
            dataPointer += DiskHeader.TrackSizes[trackIndex];
        }

        return dataPointer;
    }

    private Sector[] GetSectors(byte[] data, int trackIndex, int numberSectors, int trakDataPointer, ref int sectorDataPointer)
    {
        // sector info list
        var sectors = new Sector[numberSectors];
        for (int sectorIndex = 0; sectorIndex < numberSectors; sectorIndex++)
        {
            sectors[sectorIndex] = GetSector(data, trackIndex, trakDataPointer + (sectorIndex * _sectorHeaderSize), ref sectorDataPointer);

            // move sectorDataPointer to the next sector data postion
            sectorDataPointer += sectors[sectorIndex].ActualDataByteLength;
        }

        return sectors;
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
