using System.Collections;
using System.Runtime.CompilerServices;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers;

/// <summary>
/// Static helper methods
/// </summary>
/*
    Implementation based on the information contained here:
    http://www.cpcwiki.eu/index.php/765_FDC
    and here:
    http://www.cpcwiki.eu/imgs/f/f3/UPD765_Datasheet_OCRed.pdf
*/
public partial class NecUpd765
{
    /// <summary>
    /// Returns the specified bit value from supplied byte
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBit(int bitNumber, byte dataByte)
    {
        if (bitNumber < 0 || bitNumber > 7)
            return false;

        BitArray bi = new BitArray(new byte[] { dataByte });

        return bi[bitNumber];
    }

    /// <summary>
    /// Sets the specified bit of the supplied byte to 1
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(int bitNumber, ref byte dataByte)
    {
        if (bitNumber < 0 || bitNumber > 7)
        {
            return;
        }

        int db = (int)dataByte;
        db |= 1 << bitNumber;
        dataByte = (byte)db;
    }

    /// <summary>
    /// Sets the specified bit of the supplied byte to 0
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnSetBit(int bitNumber, ref byte dataByte)
    {
        if (bitNumber < 0 || bitNumber > 7)
            return;

        int db = (int)dataByte;
        db &= ~(1 << bitNumber);
        dataByte = (byte)db;
    }

    /// <summary>
    /// Returns a drive number (0-3) based on the first two bits of the supplied byte
    /// </summary>
    public static int GetUnitSelect(byte dataByte)
    {
        int driveNumber = dataByte & 0x03;
        return driveNumber;
    }

    /// <summary>
    /// Sets the first two bits of a byte based on the supplied drive number (0-3)
    /// </summary>
    public static void SetUnitSelect(int driveNumber, ref byte dataByte)
    {
        switch (driveNumber)
        {
            case 0:
                UnSetBit(SR0_US0, ref dataByte);
                UnSetBit(SR0_US1, ref dataByte);
                break;
            case 1:
                SetBit(SR0_US0, ref dataByte);
                UnSetBit(SR0_US1, ref dataByte);
                break;
            case 2:
                SetBit(SR0_US1, ref dataByte);
                UnSetBit(SR0_US0, ref dataByte);
                break;
            case 3:
                SetBit(SR0_US0, ref dataByte);
                SetBit(SR0_US1, ref dataByte);
                break;
        }
    }
}
