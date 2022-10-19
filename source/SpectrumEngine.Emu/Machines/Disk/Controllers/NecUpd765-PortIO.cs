using SpectrumEngine.Emu.Abstractions;
using System.Collections;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers;

/// <summary>
/// IPortIODevice
/// </summary>
public partial class NecUpd765 : IPortIODevice
{
    /// <summary>
    /// Device responds to an IN instruction
    /// </summary>
    public bool ReadPort(ushort port, out int result)
    {
        result = -1;

        if (port == 0x3ffd)
        {
            result = ReadDataRegister();
            return true;
        }
        else if (port == 0x2ffd)
        {
            result = ReadMainStatus();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Device responds to an OUT instruction
    /// </summary>
    public bool WritePort(ushort port, int value)
    {
        BitArray bits = new BitArray(new byte[] { (byte)value });

        if (port == 0x3ffd)
        {
            // Z80 is attempting to write to the data register
            WriteDataRegister((byte)value);
            return true;
        }

        if (port == 0x1ffd)
        {
            // set disk motor on/off
            FDD_FLAG_MOTOR = bits[3];
            return true;
        }

        return false;
    }
}
