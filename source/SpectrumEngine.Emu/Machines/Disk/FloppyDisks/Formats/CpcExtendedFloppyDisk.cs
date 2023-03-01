﻿using System.Text;
using System;
using System.Collections.Generic;
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

        if (!ident.ToUpper().Contains("EXTENDED CPC DSK"))
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
        int pos = 0x34;

        if (DiskHeader.NumberOfSides > 1)
        {
            throw new NotImplementedException(Properties.Resources.InvalidMultiSideImageFormatError);
        }
        else if (DiskHeader.NumberOfTracks > 42)
        {
            throw new NotImplementedException(Properties.Resources.InvalidImageTracksFormatError);
        }

        for (int i = 0; i < DiskHeader.NumberOfTracks * DiskHeader.NumberOfSides; i++)
        {
            DiskHeader.TrackSizes[i] = data[pos++] * 256;
        }

        // move to first track information block
        pos = 0x100;

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

            int p = pos;
            DiskTracks[i] = new Track();

            // track info block
            DiskTracks[i].TrackIdent = Encoding.ASCII.GetString(data, p, 12);
            p += 16;
            DiskTracks[i].TrackNumber = data[p++];
            DiskTracks[i].SideNumber = data[p++];
            DiskTracks[i].DataRate = data[p++];
            DiskTracks[i].RecordingMode = data[p++];
            DiskTracks[i].SectorSize = data[p++];
            DiskTracks[i].NumberOfSectors = data[p++];
            DiskTracks[i].GAP3Length = data[p++];
            DiskTracks[i].FillerByte = data[p++];

            int dpos = pos + 0x100;

            // sector info list
            DiskTracks[i].Sectors = new Sector[DiskTracks[i].NumberOfSectors];
            for (int s = 0; s < DiskTracks[i].NumberOfSectors; s++)
            {
                DiskTracks[i].Sectors[s] = new Sector();

                DiskTracks[i].Sectors[s].TrackNumber = data[p++];
                DiskTracks[i].Sectors[s].SideNumber = data[p++];
                DiskTracks[i].Sectors[s].SectorID = data[p++];
                DiskTracks[i].Sectors[s].SectorSize = data[p++];
                DiskTracks[i].Sectors[s].Status1 = data[p++];
                DiskTracks[i].Sectors[s].Status2 = data[p++];
                DiskTracks[i].Sectors[s].ActualDataByteLength = data.GetWordValue(p);
                p += 2;

                // sector data - begins at 0x100 offset from the start of the track info block (in this case dpos)
                DiskTracks[i].Sectors[s].SectorData = new byte[DiskTracks[i].Sectors[s].ActualDataByteLength];

                // copy the data
                for (int b = 0; b < DiskTracks[i].Sectors[s].ActualDataByteLength; b++)
                {
                    DiskTracks[i].Sectors[s].SectorData[b] = data[dpos + b];
                }

                // check for multiple weak/random sectors stored
                if (DiskTracks[i].Sectors[s].SectorSize <= 7)
                {
                    // sectorsize n=8 is equivilent to n=0 - FDC will use DTL for length
                    int specifiedSize = 0x80 << DiskTracks[i].Sectors[s].SectorSize;

                    if (specifiedSize < DiskTracks[i].Sectors[s].ActualDataByteLength)
                    {
                        // more data stored than sectorsize defines
                        // check for multiple weak/random copies
                        if (DiskTracks[i].Sectors[s].ActualDataByteLength % specifiedSize != 0)
                        {
                            DiskTracks[i].Sectors[s].ContainsMultipleWeakSectors = true;
                        }
                    }
                }

                // move dpos to the next sector data postion
                dpos += DiskTracks[i].Sectors[s].ActualDataByteLength;
            }

            // move to the next track info block
            pos += DiskHeader.TrackSizes[i];
        }

        // protection scheme detector
        // TODO: dgzornoza to implement

        return true;
    }

    /// <summary>
    /// Takes a double-sided disk byte array and converts into 2 single-sided arrays
    /// </summary>
    public static bool SplitDoubleSided(byte[] data, List<byte[]> results)
    {
        // look for standard magic string
        string ident = Encoding.ASCII.GetString(data, 0, 16);
        if (!ident.ToUpper().Contains("EXTENDED CPC DSK"))
        {
            // incorrect format
            return false;
        }

        byte[] S0 = new byte[data.Length];
        byte[] S1 = new byte[data.Length];

        // disk info block
        Array.Copy(data, 0, S0, 0, 0x100);
        Array.Copy(data, 0, S1, 0, 0x100);
        // change side number
        S0[0x31] = 1;
        S1[0x31] = 1;

        // extended format can have different track sizes
        int[] trkSizes = new int[data[0x30] * data[0x31]];

        int pos = 0x34;
        for (int i = 0; i < data[0x30] * data[0x31]; i++)
        {
            trkSizes[i] = data[pos] * 256;
            // clear destination trk sizes (will be added later)
            S0[pos] = 0;
            S1[pos] = 0;
            pos++;
        }

        // start at track info blocks
        int mPos = 0x100;
        int s0Pos = 0x100;
        int s0tCount = 0;
        int s1tCount = 0;
        int s1Pos = 0x100;
        int tCount = 0;

        while (tCount < data[0x30] * data[0x31])
        {
            // which side is this?
            var side = data[mPos + 0x11];
            if (side == 0)
            {
                // side 1
                Array.Copy(data, mPos, S0, s0Pos, trkSizes[tCount]);
                s0Pos += trkSizes[tCount];
                // trk size table
                S0[0x34 + s0tCount++] = (byte)(trkSizes[tCount] / 256);
            }
            else if (side == 1)
            {
                // side 2
                Array.Copy(data, mPos, S1, s1Pos, trkSizes[tCount]);
                s1Pos += trkSizes[tCount];
                // trk size table
                S1[0x34 + s1tCount++] = (byte)(trkSizes[tCount] / 256);
            }

            mPos += trkSizes[tCount++];
        }

        byte[] s0final = new byte[s0Pos];
        byte[] s1final = new byte[s1Pos];
        Array.Copy(S0, 0, s0final, 0, s0Pos);
        Array.Copy(S1, 0, s1final, 0, s1Pos);

        results.Add(s0final);
        results.Add(s1final);

        return true;
    }
}
