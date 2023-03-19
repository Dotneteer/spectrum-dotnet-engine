using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumEngine.Emu.Extensions
{
    public static class CollectionExtensions
    {
		/// <summary>
		/// Returns an uint16 from a byte array based on offset
		/// </summary>
		public static ushort GetWordValue(this IList<byte> buf, int offsetIndex)
		{
			return (ushort)(buf[offsetIndex] | buf[offsetIndex + 1] << 8);
		}
    }
}
