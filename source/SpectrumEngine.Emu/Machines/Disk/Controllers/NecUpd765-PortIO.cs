using SpectrumEngine.Emu.Abstractions;
using System.Collections;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers;

/// <summary>
/// IPortIODevice
/// The disk drive on the +3 is controlled by three ports:
/// 0x1ffd: Setting bit 3 high will turn the drive motor (or motors, if you have more than one drive attached) on. Setting bit 3 low will turn them off again. (0x1ffd is also used for memory control).
/// 0x2ffd: Reading from this port will return the main status register of the uPD765A (the FDC used in the +3).
/// 0x3ffd: Bytes written to this port are sent to the FDC, whilst reading from this port will read bytes from the FDC.
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
            result = (int)ReadMainStatus();
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

        ///
        if (port == 0x1ffd)
        {
            // set disk motor on/off
            FlagMotor = bits[3];
            return true;
        }

        return false;
    }
}
