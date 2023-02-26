using SpectrumEngine.Emu.Abstractions;
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
    private FlopyDiskDriveCluster _flopyDiskDriveCluster;

    /// <summary>
    /// Main constructor
    /// </summary>
    public NecUpd765(Z80MachineBase machine, FlopyDiskDriveCluster flopyDiskDriveCluster)
    {
        _machine = machine;
        _flopyDiskDriveCluster = flopyDiskDriveCluster;

        InitCommands();
        Reset();
    }

    /// <summary>
    /// Resets the FDC
    /// </summary>
    public void Reset()
    {
        // setup main status
        _mainStatusRegisters = 0;

        _statusRegisters0 = 0;
        _statusRegisters1 = 0;
        _statusRegisters2 = 0;
        _statusRegisters3 = 0;

        _mainStatusRegisters.SetBits(MainStatusRegisters.RQM);

        SetPhase_Idle();

        SRT = 6;
        HUT = 16;
        HLT = 2;
        HLT_Counter = 0;
        HUT_Counter = 0;
        IndexPulseCounter = 0;
        CMD_FLAG_MF = false;
        ActiveCommand = Commands[_cmdIndex];

    }

    private FlopyDiskDriveDevice? ActiveFloppyDiskDrive => (FlopyDiskDriveDevice?)_flopyDiskDriveCluster.ActiveFloppyDiskDrive;

    /// <summary>
    /// Setup the command structure
    /// Each command represents one of the internal UPD765 commands
    /// </summary>
    private void InitCommands()
    {
        Commands = new List<CommandConfiguration>
        {
			// invalid
            new CommandConfiguration
            {
                CommandHandler = InvalidCommandHandler,
                CommandCode = 0x00,
                CommandFlow = CommandFlow.Out,
                ParameterBytesCount = 0,
                ResultBytesCount = 1
            },
            // read data
            new CommandConfiguration
            {
                CommandHandler = ReadDataCommandHandler,
                CommandCode = 0x06,
                MT = true,
                MF = true,
                SK = true,
                CommandOperation = CommandOperation.Read,
                CommandFlow = CommandFlow.Out,
                ParameterBytesCount = 8,
                ResultBytesCount = 7
            },
            // read deleted data
            new CommandConfiguration
            {
                CommandHandler = ReadDeletedDataCommandHandler,
                CommandCode = 0x0c,
                MT = true,
                MF = true,
                SK = true,
                CommandOperation = CommandOperation.Read,
                CommandFlow = CommandFlow.Out,
                ParameterBytesCount = 8,
                ResultBytesCount = 7
            },
			// read diagnostic
            new CommandConfiguration
            {
                CommandHandler = ReadDiagnosticCommandHandler, 
                CommandCode = 0x02, 
                MF = true, 
                SK = true, 
                CommandOperation = CommandOperation.Read,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 8, 
                ResultBytesCount = 7
            },
            // read id
            new CommandConfiguration 
            { 
                CommandHandler = ReadIdCommandHandler, 
                CommandCode = 0x0a, 
                MF = true, 
                CommandOperation = CommandOperation.Read,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 1, 
                ResultBytesCount = 7 
            },
            // recalibrate (seek track00)
            new CommandConfiguration 
            { 
                CommandHandler = RecalibrateCommandHandler, 
                CommandCode = 0x07,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 1, 
                ResultBytesCount = 0 
            },
            // scan equal
            new CommandConfiguration 
            { 
                CommandHandler = ScanEqualCommandHandler, 
                CommandCode = 0x11, 
                MT = true, 
                MF = true, 
                SK = true, 
                CommandOperation =  CommandOperation.Read,
                CommandFlow = CommandFlow.In, 
                ParameterBytesCount = 8, 
                ResultBytesCount = 7 
            },
            // scan high or equal
            new CommandConfiguration 
            { 
                CommandHandler = ScanHighOrEqualCommandHandler, 
                CommandCode = 0x1d, 
                MT = true, 
                MF = true, 
                SK = true, 
                CommandOperation = CommandOperation.Read,
                CommandFlow = CommandFlow.In, 
                ParameterBytesCount = 8, 
                ResultBytesCount = 7 
            },
            // scan low or equal
            new CommandConfiguration 
            { 
                CommandHandler = ScanLowOrEqualCommandHandler, 
                CommandCode = 0x19, 
                MT = true, 
                MF = true, 
                SK = true, 
                CommandOperation = CommandOperation.Read,
                CommandFlow = CommandFlow.In, 
                ParameterBytesCount = 8, 
                ResultBytesCount = 7 
            },
            // seek
            new CommandConfiguration 
            { 
                CommandHandler = SeekCommandHandler, 
                CommandCode = 0x0f,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 2, 
                ResultBytesCount = 0 
            },
            // sense drive status
            new CommandConfiguration 
            { 
                CommandHandler = SenseDriveStatusCommandHandler, 
                CommandCode = 0x04,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 1, 
                ResultBytesCount = 1 
            },
            // sense interrupt status
            new CommandConfiguration 
            { 
                CommandHandler = SenseInterruptStatusCommandHandler, 
                CommandCode = 0x08,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 0, 
                ResultBytesCount = 2 
            },
            // specify
            new CommandConfiguration 
            { 
                CommandHandler = SpecifyCommandHandler, 
                CommandCode = 0x03,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 2, 
                ResultBytesCount = 0 
            },
            // version
            new CommandConfiguration 
            { 
                CommandHandler = VersionCommandHandler, 
                CommandCode = 0x10,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 0, 
                ResultBytesCount = 1 
            },
            // write data
            new CommandConfiguration 
            { 
                CommandHandler = WriteDataCommandHandler, 
                CommandCode = 0x05, 
                MT = true, 
                MF = true, 
                CommandOperation = CommandOperation.Write,
                CommandFlow = CommandFlow.In, 
                ParameterBytesCount = 8, 
                ResultBytesCount = 7 
            },
            // write deleted data
            new CommandConfiguration 
            { 
                CommandHandler = WriteDeletedDataCommandHandler, 
                CommandCode = 0x09, 
                MT = true, 
                MF = true, 
                CommandOperation = CommandOperation.Write,
                CommandFlow = CommandFlow.In, 
                ParameterBytesCount = 8, 
                ResultBytesCount = 7 
            },
            // write id
            new CommandConfiguration 
            { 
                CommandHandler = WriteIdCommandHandler, 
                CommandCode = 0x0d, 
                MF = true, 
                CommandOperation = CommandOperation.Write,
                CommandFlow = CommandFlow.In, 
                ParameterBytesCount = 5, 
                ResultBytesCount = 7 
            },
        };
    }
}
