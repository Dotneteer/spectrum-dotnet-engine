using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumEngine.Emu.Machines.Disk
{
	/// <summary>
	/// Represents a device that utilizes port IN &amp; OUT
	/// </summary>
	public interface IPortIODevice
	{
		/// <summary>
		/// Device responds to an IN instruction
		/// </summary>
		bool ReadPort(ushort port, ref int result);

		/// <summary>
		/// Device responds to an OUT instruction
		/// </summary>
		bool WritePort(ushort port, int result);
	}
}
