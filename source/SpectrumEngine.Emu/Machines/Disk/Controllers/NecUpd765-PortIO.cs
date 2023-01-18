using SpectrumEngine.Emu.Abstractions;
using System.Collections;
using System.Diagnostics;

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
            WriteDebug("Read Data Register:", data);
            return true;
        }
        // reading the FDC main status register port
        else if (port == 0x2ffd)
        {
            data = (byte)ReadMainStatus();
            WriteDebug("Read Main Status Register:", data);
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
            WriteDebug("Write Data Register:", data);
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

    int fileDebugCounter = 1;
    private void WriteDebug(string text, byte data)
    {
        string output = $"{text} {data} ({ListFlags(data)}), Command: {ActiveCommand.CommandCode}, Phase: {ActivePhase}{System.Environment.NewLine}";
        System.IO.File.AppendAllText("C:/temp/spectrum.log", output);
        if (fileDebugCounter == 51)
        {
            Debugger.Break();
        }

        fileDebugCounter++;
    }

    int fileDebugCounter2 = 1;
    private void WriteDebug2(string text)
    {
        System.IO.File.AppendAllText("C:/temp/spectrum2.log", text + System.Environment.NewLine);
        //if (fileDebugCounter == 51)
        //{
        //    Debugger.Break();
        //}

        fileDebugCounter2++;
    }

    public static string ListFlags(byte value)
    {
        var register = (MainStatusRegisters)value;

        string result = string.Empty;
        if (register.HasFlag(MainStatusRegisters.D0B)) result += "D0B ";
        if (register.HasFlag(MainStatusRegisters.D1B)) result += "D1B ";
        if (register.HasFlag(MainStatusRegisters.D2B)) result += "D2B ";
        if (register.HasFlag(MainStatusRegisters.D3B)) result += "D3B ";
        if (register.HasFlag(MainStatusRegisters.CB)) result += "CB ";
        if (register.HasFlag(MainStatusRegisters.EXM)) result += "EXM ";
        if (register.HasFlag(MainStatusRegisters.DIO)) result += "DIO ";
        if (register.HasFlag(MainStatusRegisters.RQM)) result += "RQM ";

        return result;
    }
}
