using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumEngine.Emu.Machines.Disk
{
	/// <summary>
	/// Defines an object that can load a floppy disk image
	/// </summary>
	public interface IFDDHost
	{
		/// <summary>
		/// The currently inserted diskimage
		/// </summary>
		FloppyDisk Disk { get; set; }

		/// <summary>
		/// Parses a new disk image and loads it into this floppy drive
		/// </summary>
		void FDD_LoadDisk(byte[] diskData);

		/// <summary>
		/// Ejects the current disk
		/// </summary>
		void FDD_EjectDisk();

		/// <summary>
		/// Signs whether the current active drive has a disk inserted
		/// </summary>   
		bool FDD_IsDiskLoaded { get; }
	}
}
