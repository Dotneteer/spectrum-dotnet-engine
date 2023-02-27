﻿using SpectrumEngine.Emu.Abstractions;
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

        SetPhaseIdle();

        SRT = 6;
        HUT = 16;
        HLT = 2;
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
                CommandCode = CommandCode.Invalid,
                CommandFlow = CommandFlow.Out,
                ParameterBytesCount = 0,
                ResultBytesCount = 1
            },
            // read data
            new CommandConfiguration
            {
                CommandHandler = ReadDataCommandHandler,
                CommandCode = CommandCode.ReadData,
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
                CommandCode = CommandCode.ReadDeletedData,
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
                CommandCode = CommandCode.ReadDiagnostic, 
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
                CommandCode = CommandCode.ReadId, 
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
                CommandCode = CommandCode.Recalibrate,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 1, 
                ResultBytesCount = 0 
            },
            // scan equal
            new CommandConfiguration 
            { 
                CommandHandler = ScanEqualCommandHandler, 
                CommandCode = CommandCode.ScanEqual, 
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
                CommandCode = CommandCode.ScanHighOrEqual, 
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
                CommandCode = CommandCode.ScanLowOrEqual, 
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
                CommandCode = CommandCode.Seek,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 2, 
                ResultBytesCount = 0 
            },
            // sense drive status
            new CommandConfiguration 
            { 
                CommandHandler = SenseDriveStatusCommandHandler, 
                CommandCode = CommandCode.SenseDriveStatus,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 1, 
                ResultBytesCount = 1 
            },
            // sense interrupt status
            new CommandConfiguration 
            { 
                CommandHandler = SenseInterruptStatusCommandHandler, 
                CommandCode = CommandCode.SenseInterruptStatus,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 0, 
                ResultBytesCount = 2 
            },
            // specify
            new CommandConfiguration 
            { 
                CommandHandler = SpecifyCommandHandler, 
                CommandCode = CommandCode.Specify,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 2, 
                ResultBytesCount = 0 
            },
            // version
            new CommandConfiguration 
            { 
                CommandHandler = VersionCommandHandler, 
                CommandCode = CommandCode.Version,
                CommandFlow = CommandFlow.Out, 
                ParameterBytesCount = 0, 
                ResultBytesCount = 1 
            },
            // write data
            new CommandConfiguration 
            { 
                CommandHandler = WriteDataCommandHandler, 
                CommandCode = CommandCode.WriteData, 
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
                CommandCode = CommandCode.WriteDeletedData, 
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
                CommandCode = CommandCode.WriteId, 
                MF = true, 
                CommandOperation = CommandOperation.Write,
                CommandFlow = CommandFlow.In, 
                ParameterBytesCount = 5, 
                ResultBytesCount = 7 
            },
        };
    }
}
