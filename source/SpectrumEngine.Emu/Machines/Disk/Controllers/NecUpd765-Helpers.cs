using SpectrumEngine.Emu.Extensions;
using System.Collections;
using System.Runtime.CompilerServices;
using static SpectrumEngine.Emu.Machines.Disk.Controllers.NecUpd765;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers;


internal static class NecUpd765RegisterHelpers
{
    public static void SetBits(this ref MainStatusRegisters registers, MainStatusRegisters statusRegisters) =>
        registers |= statusRegisters;

    public static void SetBits(this ref StatusRegisters0 registers, StatusRegisters0 statusRegisters) =>
        registers |= statusRegisters;

    public static void SetBits(this ref StatusRegisters1 registers, StatusRegisters1 statusRegisters) =>
        registers |= statusRegisters;

    public static void SetBits(this ref StatusRegisters2 registers, StatusRegisters2 statusRegisters) =>
        registers |= statusRegisters;

    public static void SetBits(this ref StatusRegisters3 registers, StatusRegisters3 statusRegisters) =>
        registers |= statusRegisters;

    public static void UnSetBits(this ref MainStatusRegisters registers, MainStatusRegisters statusRegisters) =>
        registers &= ~statusRegisters;

    public static void UnSetBits(this ref StatusRegisters0 registers, StatusRegisters0 statusRegisters) =>
        registers &= ~statusRegisters;

    public static void UnSetBits(this ref StatusRegisters1 registers, StatusRegisters1 statusRegisters) =>
        registers &= ~statusRegisters;

    public static void UnSetBits(this ref StatusRegisters2 registers, StatusRegisters2 statusRegisters) =>
        registers &= ~statusRegisters;

    public static void UnSetBits(this ref StatusRegisters3 registers, StatusRegisters3 statusRegisters) =>
        registers &= ~statusRegisters;

    public static void SetAbnormalTerminationCommand(this ref StatusRegisters0 registers)
    {
        registers.SetBits(StatusRegisters0.IC_D6);
        registers.UnSetBits(StatusRegisters0.IC_D7);
    }
}

/// <summary>
/// Static helper methods
/// </summary>
public partial class NecUpd765
{
    /// <summary>
    /// Returns a drive number (0-3) based on the first two bits of the supplied byte
    /// </summary>
    public static int GetUnitSelect(byte dataByte) => dataByte & 0x03;

    /// <summary>
    /// Sets the first two bits of a byte based on the supplied drive number (0-3)
    /// </summary>
    //public static void SetUnitSelect(ref byte dataByte, int driveNumber)
    //{
    //    switch (driveNumber)
    //    {
    //        case 0:
    //            dataByte.UnSetBit((int)StatusRegisters0.US0);
    //            dataByte.UnSetBit((int)StatusRegisters0.US1);
    //            break;
    //        case 1:
    //            dataByte.SetBit((int)StatusRegisters0.US0);
    //            dataByte.UnSetBit((int)StatusRegisters0.US1);
    //            break;
    //        case 2:
    //            dataByte.UnSetBit((int)StatusRegisters0.US0);
    //            dataByte.SetBit((int)StatusRegisters0.US1);
    //            break;
    //        case 3:
    //            dataByte.SetBit((int)StatusRegisters0.US0);
    //            dataByte.SetBit((int)StatusRegisters0.US1);
    //            break;
    //    }
    //}
}
