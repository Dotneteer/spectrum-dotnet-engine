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
