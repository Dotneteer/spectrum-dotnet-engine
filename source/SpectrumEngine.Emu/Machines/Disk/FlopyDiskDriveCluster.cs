using SpectrumEngine.Emu.Abstractions;
using SpectrumEngine.Emu.Extensions;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
using System;

namespace SpectrumEngine.Emu.Machines.Disk;

/// <summary>
/// Floppy disk drive cluster
/// </summary>
public class FlopyDiskDriveCluster
{
	private readonly FlopyDiskDriveDevice[] _floppyDiskDrives = new FlopyDiskDriveDevice[2];

	private int _floppyDiskDriveSlot = 0;
	private FlopyDiskDriveDevice? _activeFloppyDiskDrive;

	public FlopyDiskDriveCluster()
        {
	}

	/// <summary>
	/// Signs whether is disk inserted in active drive slot
	/// </summary>
	public bool IsFloppyDiskLoaded => _floppyDiskDrives[FloppyDiskDriveSlot] != null && _floppyDiskDrives[FloppyDiskDriveSlot].IsDiskLoaded;

	/// <summary>
	/// Active floppy disk
	/// </summary>
	public FloppyDisk? FloppyDisk { get; set; }

	/// <summary>
	/// Active floppy disk drive slot
	/// </summary>
	public int FloppyDiskDriveSlot
	{
		get => _floppyDiskDriveSlot;
		set
		{
			_floppyDiskDriveSlot = value;
                _activeFloppyDiskDrive = _floppyDiskDrives[_floppyDiskDriveSlot];
		}
	}

	/// <summary>
	/// Load floppy disk data in active slot
	/// </summary>
	/// <exception cref="InvalidOperationException">Invalid disk data</exception>
	public void LoadDisk(byte[] diskData)
	{
		_floppyDiskDrives[_floppyDiskDriveSlot].LoadDisk(diskData);
	}

	/// <summary>
	/// Ejects floppy disk from active slot
	/// </summary>
	public void EjectDisk()
	{
		_floppyDiskDrives[_floppyDiskDriveSlot].EjectDisk();
	}

	/// <summary>
	/// Initialization / reset of the floppy drive subsystem
	/// </summary>
	private void Init()
	{
		for (int i = 0; i < 4; i++)
		{
			FlopyDiskDriveDevice ds = new FlopyDiskDriveDevice(i, this);
			_floppyDiskDrives[i] = ds;
		}
	}

	/// <summary>
	/// Searches for the requested sector
	/// </summary>
	private FloppyDisk.Sector GetSector()
	{
		FloppyDisk.Sector sector = null;

		// get the current track
		var trk = ActiveDrive.Disk.DiskTracks[ActiveDrive.TrackIndex];

		// get the current sector index
		int index = ActiveDrive.SectorIndex;

		// make sure this index exists
		if (index > trk.Sectors.Length)
		{
			index = 0;
		}

		// index hole count
		int iHole = 0;

		// loop through the sectors in a track
		// the loop ends with either the sector being found
		// or the index hole being passed twice
		while (iHole <= 2)
		{
			// does the requested sector match the current sector
			if (trk.Sectors[index].SectorIDInfo.C == ActiveCommandParams.Cylinder &&
				trk.Sectors[index].SectorIDInfo.H == ActiveCommandParams.Head &&
				trk.Sectors[index].SectorIDInfo.R == ActiveCommandParams.Sector &&
				trk.Sectors[index].SectorIDInfo.N == ActiveCommandParams.SectorSize)
			{
				// sector has been found
				sector = trk.Sectors[index];

				UnSetBit(SR2_BC, ref Status2);
				UnSetBit(SR2_WC, ref Status2);
				break;
			}

			// check for bad cylinder
			if (trk.Sectors[index].SectorIDInfo.C == 255)
			{
				SetBit(SR2_BC, ref Status2);
			}
			// check for no cylinder
			else if (trk.Sectors[index].SectorIDInfo.C != ActiveCommandParams.Cylinder)
			{
				SetBit(SR2_WC, ref Status2);
			}

			// incrememnt sector index
			index++;

			// have we reached the index hole?
			if (trk.Sectors.Length <= index)
			{
				// wrap around
				index = 0;
				iHole++;
			}
		}

		// search loop has completed and the sector may or may not have been found

		// bad cylinder detected?
		if (Status2.HasBit(SR2_BC))
		{
			// remove WC
			UnSetBit(SR2_WC, ref Status2);
		}

		// update sectorindex on drive
		ActiveDrive.SectorIndex = index;

		return sector;
	}
}
