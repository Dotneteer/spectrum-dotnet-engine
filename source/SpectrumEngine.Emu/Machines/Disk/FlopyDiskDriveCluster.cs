using SpectrumEngine.Emu.Abstractions;
using SpectrumEngine.Emu.Extensions;
using SpectrumEngine.Emu.Machines.FloppyDiskDrives.FloppyDisks;
using System;

namespace SpectrumEngine.Emu.Machines.Disk
{
    /// <summary>
    /// Floppy disk drive cluster
    /// </summary>
    public class FlopyDiskDriveCluster
	{
        private DriveState _activeDrive;
        private int _diskDriveIndex = 0;
        private readonly DriveState[] DriveStates = new DriveState[4];

        /// <summary>
        /// Index of the currently active disk drive
        /// </summary>
        public int DiskDriveIndex
		{
			get => _diskDriveIndex;
			set
			{
				// when index is changed update the ActiveDrive
				_diskDriveIndex = value;
                _activeDrive = DriveStates[_diskDriveIndex];
			}
		}		



		/// <summary>
		/// Initialization / reset of the floppy drive subsystem
		/// </summary>
		private void Init()
		{
			for (int i = 0; i < 4; i++)
			{
				DriveState ds = new DriveState(i, this);
				DriveStates[i] = ds;
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

		// IFDDHost methods that fall through to the currently active drive

		/// <summary>
		/// Parses a new disk image and loads it into this floppy drive
		/// </summary>
		public void LoadDisk(byte[] diskData)
		{
			// we are only going to load into the first drive
			DriveStates[0].LoadDisk(diskData);
		}

		/// <summary>
		/// Ejects the current disk
		/// </summary>
		public void EjectDisk()
		{
			DriveStates[0].EjectDisk();
		}

		/// <summary>
		/// Signs whether the current active drive has a disk inserted
		/// </summary>
		public bool IsDiskLoaded => DriveStates[DiskDriveIndex].IsDiskLoaded;

		/// <summary>
		/// Returns the disk object from drive 0
		/// </summary>
		public FloppyDisk DiskPointer => DriveStates[0].Disk;

		public FloppyDisk Disk { get; set; }
	}
}
