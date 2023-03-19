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
public partial class NecUpd765 : IPortIODevice<byte>
{
    /// <summary>
    /// Device responds to an READ instruction
    /// </summary>
    public bool TryReadPort(ushort port, out byte data)
    {
        data = 0x00;

        // reading the FDC data port
        if (port == 0x3ffd)
        {
            data = ReadDataRegister();
            return true;
        }
        // reading the FDC main status register port
        else if (port == 0x2ffd)
        {
            data = (byte)ReadMainStatus();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Device responds to an WRITE instruction
    /// </summary>
    public bool TryWritePort(ushort port, byte data)
    {
        

        // write into FDC data port
        if (port == 0x3ffd)
        {
            // Z80 is attempting to write to the data register
            WriteDataRegister(data);
            return true;
        }
        
        if (port == 0x1ffd)
        {
            var bits = new BitArray(new byte[] { data });

            // set disk motor on/off
            FlagMotor = bits[3];
            return true;
        }

        return false;
    }
}
