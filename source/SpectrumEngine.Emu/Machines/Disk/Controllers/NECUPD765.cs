using System.Collections.Generic;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers;

/// <summary>
/// The NEC floppy disk controller (and floppy drive) found in the +3
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
	/// The emulated spectrum machine
	/// </summary>
	private Z80MachineBase _machine;

	/// <summary>
	/// Main constructor
	/// </summary>
	public NecUpd765(Z80MachineBase machine)
	{
		_machine = machine;

		InitCommandList();				
		TimingInit();
		Reset();

		FDD_Init();
	}

	/// <summary>
	/// Resets the FDC
	/// </summary>
	public void Reset()
	{
		// setup main status
		StatusMain = 0;

		Status0 = 0;
		Status1 = 0;
		Status2 = 0;
		Status3 = 0;

		SetBit(MSR_RQM, ref StatusMain);

		SetPhase_Idle();

		SRT = 6;
		HUT = 16;
		HLT = 2;
		HLT_Counter = 0;
		HUT_Counter = 0;
		IndexPulseCounter = 0;
		CMD_FLAG_MF = false;
	}

	/// <summary>
	/// Setup the command structure
	/// Each command represents one of the internal UPD765 commands
	/// </summary>
	private void InitCommandList()
	{
		CommandList = new List<Command>
		{
                // read data
                new Command { CommandDelegate = UPD_ReadData, CommandCode = 0x06, MT = true, MF = true, SK = true, IsRead = true,
				Direction = CommandDirection.Out, ParameterByteCount = 8, ResultByteCount = 7 },
                // read id
                new Command { CommandDelegate = UPD_ReadID, CommandCode = 0x0a, MF = true, IsRead = true,
				Direction = CommandDirection.Out, ParameterByteCount = 1, ResultByteCount = 7 },
                // specify
                new Command { CommandDelegate = UPD_Specify, CommandCode = 0x03,
				Direction = CommandDirection.Out, ParameterByteCount = 2, ResultByteCount = 0 },
                // read diagnostic
                new Command { CommandDelegate = UPD_ReadDiagnostic, CommandCode = 0x02, MF = true, SK = true, IsRead = true,
				Direction = CommandDirection.Out, ParameterByteCount = 8, ResultByteCount = 7 },
                // scan equal
                new Command { CommandDelegate = UPD_ScanEqual, CommandCode = 0x11, MT = true, MF = true, SK = true, IsRead = true,
				Direction = CommandDirection.In, ParameterByteCount = 8, ResultByteCount = 7 },
                // scan high or equal
                new Command { CommandDelegate = UPD_ScanHighOrEqual, CommandCode = 0x1d, MT = true, MF = true, SK = true, IsRead = true,
				Direction = CommandDirection.In, ParameterByteCount = 8, ResultByteCount = 7 },
                // scan low or equal
                new Command { CommandDelegate = UPD_ScanLowOrEqual, CommandCode = 0x19, MT = true, MF = true, SK = true, IsRead = true,
				Direction = CommandDirection.In, ParameterByteCount = 8, ResultByteCount = 7 },
                // read deleted data
                new Command { CommandDelegate = UPD_ReadDeletedData, CommandCode = 0x0c, MT = true, MF = true, SK = true, IsRead = true,
				Direction = CommandDirection.Out, ParameterByteCount = 8, ResultByteCount = 7 },
                // write data
                new Command { CommandDelegate = UPD_WriteData, CommandCode = 0x05, MT = true, MF = true, IsWrite = true,
				Direction = CommandDirection.In, ParameterByteCount = 8, ResultByteCount = 7 },
                // write id
                new Command { CommandDelegate = UPD_WriteID, CommandCode = 0x0d, MF = true, IsWrite = true,
				Direction = CommandDirection.In, ParameterByteCount = 5, ResultByteCount = 7 },
                // write deleted data
                new Command { CommandDelegate = UPD_WriteDeletedData, CommandCode = 0x09, MT = true, MF = true, IsWrite = true,
				Direction = CommandDirection.In, ParameterByteCount = 8, ResultByteCount = 7 },
                // seek
                new Command { CommandDelegate = UPD_Seek, CommandCode = 0x0f,
				Direction = CommandDirection.Out, ParameterByteCount = 2, ResultByteCount = 0 },
                // recalibrate (seek track00)
                new Command { CommandDelegate = UPD_Recalibrate, CommandCode = 0x07,
				Direction = CommandDirection.Out, ParameterByteCount = 1, ResultByteCount = 0 },
                // sense interrupt status
                new Command { CommandDelegate = UPD_SenseInterruptStatus, CommandCode = 0x08,
				Direction = CommandDirection.Out, ParameterByteCount = 0, ResultByteCount = 2 },
                // sense drive status
                new Command { CommandDelegate = UPD_SenseDriveStatus, CommandCode = 0x04,
				Direction = CommandDirection.Out, ParameterByteCount = 1, ResultByteCount = 1 },
                // version
                new Command { CommandDelegate = UPD_Version, CommandCode = 0x10,
				Direction = CommandDirection.Out, ParameterByteCount = 0, ResultByteCount = 1 },
                // invalid
                new Command { CommandDelegate = UPD_Invalid, CommandCode = 0x00,
				Direction = CommandDirection.Out, ParameterByteCount = 0, ResultByteCount = 1 },
		};
	}
}
