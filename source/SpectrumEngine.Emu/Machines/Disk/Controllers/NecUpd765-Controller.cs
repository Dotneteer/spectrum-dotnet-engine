using SpectrumEngine.Emu.Extensions;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
using System;
using System.Collections;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers;

/// <summary>
/// FDC State and Methods
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
    /// Signs whether the drive is active
    /// </summary>
    public bool DriveLight { get; private set; }

    /// <summary>
    /// Collection of possible commands
    /// </summary>
    private List<CommandConfiguration> Commands;

    /// <summary>
    /// Active command state
    /// </summary>
    private CommandData ActiveCommandData = new();

    /// <summary>
    /// Current active phase controller
    /// </summary>
    private ControllerCommandPhase ActivePhase = ControllerCommandPhase.Command;

    /// <summary>
    /// Command parameters
    /// </summary>
    private byte[] CommandParameters = new byte[9];

    /// <summary>
    /// Current index in command parameters
    /// </summary>
    private int CommandParameterIndex = 0;

    /// <summary>
    /// Initial command byte flag
    /// Bit7  Multi Track (continue multi-sector-function on other head)
    /// </summary>
    private bool CMD_FLAG_MT;

    /// <summary>
    /// Initial command byte flag
    /// Bit6  MFM-Mode-Bit (Default 1=Double Density)
    /// </summary>
    private bool CMD_FLAG_MF;

    /// <summary>
    /// Initial command byte flag
    /// Bit5  Skip-Bit (set if secs with deleted DAM shall be skipped)
    /// </summary>
    private bool CMD_FLAG_SK;

    /// <summary>
    /// Step Rate Time (supplied via the specify command)
    /// SRT stands for the steooino rate for the FDD ( 1 to 16 ms in 1 ms increments). 
    /// Stepping rate applies to all drives(FH= 1ms, EH= 2ms, etc.).
    /// </summary>
    private int SRT;

    /// <summary>
    /// Head Unload Time (supplied via the specify command)
    /// HUT stands for the head unload time after a Read or Write operation has occurred 
    /// (16 to 240 ms in 16 ms Increments)
    /// </summary>
    private int HUT;

    /// <summary>
    /// Head load Time (supplied via the specify command)
    /// HLT stands for the head load time in the FDD (2 to 254 ms in 2 ms Increments)
    /// </summary>
    private int HLT;

    /// <summary>
    /// Non-DMA Mode (supplied via the specify command)
    /// ND stands for operation in the non-DMA mode
    /// </summary>
    private bool ND;

    /// <summary>
    /// In lieu of actual timing, this will count status reads in execution phase
    /// where the CPU hasnt actually read any bytes
    /// </summary>
    private int OverrunCounter;

    /// <summary>
    /// Contains result bytes in result phase
    /// </summary>
    private byte[] ResultBuffer = new byte[7];

    /// <summary>
    /// Current index in result buffer
    /// </summary>

    private int ResultBufferCounter = 0;

    /// <summary>
    /// Contains sector data to be written/read in execution phase
    /// </summary>
    private byte[] ExecutionBuffer = new byte[0x8000];

    /// <summary>
    /// Index for sector data within the result buffer
    /// </summary>
    private int ExecutionBufferCounter = 0;

    /// <summary>
    /// The byte length of the currently active command
    /// This may or may not be the same as the actual command resultbytes value
    /// </summary>
    private int ResultLength = 0;

    /// <summary>
    /// The length of the current exec command
    /// </summary>
    private int ExecutionLength = 0;

    /// <summary>
    /// The last write byte that was received during execution phase
    /// </summary>
    private byte LastSectorDataWriteByte = 0;

    /// <summary>
    /// The last read byte to be sent during execution phase
    /// </summary>
    private byte LastSectorDataReadByte = 0;

    /// <summary>
    /// The last parameter byte that was written to the FDC
    /// </summary>
    private byte LastByteReceived = 0;

    /// <summary>
    /// signs whether flag motor is on/off
    /// </summary>
    public bool FlagMotor { get; private set; }

    /// <summary>
    /// Current selected command in <see cref="Commands"/>
    /// </summary>
    private int _cmdIndex;
    public int CmdIndex
    {
        get => _cmdIndex;
        set
        {
            _cmdIndex = value;
            ActiveCommand = Commands[_cmdIndex];
        }
    }

    /// <summary>
    /// The currently active command
    /// </summary>
    private CommandConfiguration ActiveCommand;

    /// <summary>
    /// Main status register (accessed via reads to port 0x2ffd)
    /// </summary>
    private MainStatusRegisters _mainStatusRegisters;

    /// <summary>
    /// Status Register 0
    /// </summary>
    private StatusRegisters0 _statusRegisters0;

    /// <summary>
    /// Status Register 1
    /// </summary>
    private StatusRegisters1 _statusRegisters1;

    /// <summary>
    /// Status Register 2
    /// </summary>
    private StatusRegisters2 _statusRegisters2;

    /// <summary>
    /// Status Register 3
    /// </summary>
    private StatusRegisters3 _statusRegisters3;

    /// <summary>
    /// Called when a status register read is required
    /// This can be called at any time
    /// The main status register appears to be queried nearly all the time
    /// so needs to be kept updated. It keeps the CPU informed of the current state
    /// </summary>
    private MainStatusRegisters ReadMainStatus()
    {
        _mainStatusRegisters.SetBits(MainStatusRegisters.RQM);

        switch (ActivePhase)
        {
            case ControllerCommandPhase.Idle:
                _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO | MainStatusRegisters.CB | MainStatusRegisters.EXM);
                break;

            case ControllerCommandPhase.Command:
                _mainStatusRegisters.SetBits(MainStatusRegisters.CB);
                _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO | MainStatusRegisters.EXM);
                break;

            case ControllerCommandPhase.Execution:
                if (ActiveCommand.CommandFlow == CommandFlow.Out)
                {
                    _mainStatusRegisters.SetBits(MainStatusRegisters.DIO);
                }
                else
                {
                    _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO);
                }

                _mainStatusRegisters.SetBits(MainStatusRegisters.DIO | MainStatusRegisters.EXM);

                // overrun detection                    
                OverrunCounter++;
                if (OverrunCounter >= 64)
                {
                    // CPU has read the status register 64 times without reading the data register
                    // switch the current command into result phase
                    ActivePhase = ControllerCommandPhase.Result;

                    // reset the overun counter
                    OverrunCounter = 0;
                }

                break;

            case ControllerCommandPhase.Result:
                _mainStatusRegisters.SetBits(MainStatusRegisters.DIO | MainStatusRegisters.CB);
                _mainStatusRegisters.UnSetBits(MainStatusRegisters.EXM);
                break;
        }

        return _mainStatusRegisters;
    }

    /// <summary>
    /// Handles CPU reading from the data register
    /// </summary>
    private byte ReadDataRegister()
    {
        // default return value
        byte res = 0xff;

        // check RQM flag status
        if (!_mainStatusRegisters.HasFlag(MainStatusRegisters.RQM))
        {
            // FDC is not ready to return data
            return res;
        }

        // check active direction
        if (!_mainStatusRegisters.HasFlag(MainStatusRegisters.DIO))
        {
            // FDC is expecting to receive, not send data
            return res;
        }

        switch (ActivePhase)
        {
            case ControllerCommandPhase.Execution:
                // reset overrun counter
                OverrunCounter = 0;

                // execute read
                ActiveCommand.CommandHandler();

                res = LastSectorDataReadByte;

                if (ExecutionBufferCounter <= 0)
                {
                    // end of execution phase
                    ActivePhase = ControllerCommandPhase.Result;
                }

                return res;

            case ControllerCommandPhase.Result:

                DriveLight = false;

                ActiveCommand.CommandHandler();

                // result byte reading
                res = ResultBuffer[ResultBufferCounter];

                // increment result counter
                ResultBufferCounter++;

                if (ResultBufferCounter >= ResultLength)
                {
                    ActivePhase = ControllerCommandPhase.Idle;
                }

                break;
        }

        return res;
    }

    /// <summary>
    /// Handles CPU writing to the data register
    /// </summary>
    private void WriteDataRegister(byte data)
    {
        if (!_mainStatusRegisters.HasFlag(MainStatusRegisters.RQM) || _mainStatusRegisters.HasFlag(MainStatusRegisters.DIO))
        {
            // FDC will not receive and process any bytes
            return;
        }

        // store the incoming byte
        LastByteReceived = data;

        // process incoming bytes
        switch (ActivePhase)
        {
            //// controller is idle awaiting the first command byte of a new instruction
            case ControllerCommandPhase.Idle:
                ParseCommandByte(data);
                break;
            //// we are in command phase
            case ControllerCommandPhase.Command:
                // attempt to process this parameter byte
                //ProcessCommand(data);      
                ActiveCommand.CommandHandler();
                break;
            //// we are in execution phase
            case ControllerCommandPhase.Execution:
                // CPU is going to be sending data bytes to the FDC to be written to disk

                // store the byte
                LastSectorDataWriteByte = data;
                ActiveCommand.CommandHandler();

                if (ExecutionBufferCounter <= 0)
                {
                    // end of execution phase
                    ActivePhase = ControllerCommandPhase.Result;
                }

                break;
            //// result phase
            case ControllerCommandPhase.Result:
                // data register will not receive bytes during result phase
                break;
        }
    }

    /// <summary>
    /// Processes the first command byte (within a command instruction)
    /// Returns TRUE if successful. FALSE if otherwise
    /// Called only in idle phase
    /// </summary>
    private bool ParseCommandByte(byte cmdByte)
    {
        // clear counters
        CommandParameterIndex = 0;
        ResultBufferCounter = 0;

        // get the first 4 bytes
        byte cByte = (byte)(cmdByte & 0x0f);

        // get MT, MD and SK states
        CMD_FLAG_MT = cmdByte.HasBit(7);
        CMD_FLAG_MF = cmdByte.HasBit(6);
        CMD_FLAG_SK = cmdByte.HasBit(5);

        cmdByte = cByte;

        // lookup the command
        var cmd = Commands.FirstOrDefault(a => a.CommandCode == cmdByte);

        if (cmd == null)
        {
            // no command found - use invalid
            CmdIndex = Commands.Count - 1;
        }
        else
        {
            // valid command found
            CmdIndex = Commands.FindIndex(item => item.CommandCode == cmdByte);

            // check validity of command byte flags
            // if a flag is set but not valid for this command then it is invalid
            bool invalid = false;

            if (!ActiveCommand.MT)
                if (CMD_FLAG_MT)
                    invalid = true;
            if (!ActiveCommand.MF)
                if (CMD_FLAG_MF)
                    invalid = true;
            if (!ActiveCommand.SK)
                if (CMD_FLAG_SK)
                    invalid = true;

            if (invalid)
            {
                // command byte included spurious bit 5,6 or 7 flags
                CmdIndex = Commands.Count - 1;
            }
        }

        CommandParameterIndex = 0;
        ResultBufferCounter = 0;

        // there will now be an active command set
        // move to command phase
        ActivePhase = ControllerCommandPhase.Command;

        // set reslength
        ResultLength = ActiveCommand.ResultBytesCount;

        // if there are no expected param bytes to receive - go ahead and run the command
        if (ActiveCommand.ParameterBytesCount == 0)
        {
            ActivePhase = ControllerCommandPhase.Execution;
            ActiveCommand.CommandHandler();
        }

        return true;
    }

    /// <summary>
    /// Parse command parameter byte
    /// </summary>
    private void ParseParameterByte(CommandParameter index)
    {
        byte currByte = CommandParameters[(int)index];
        BitArray bi = new BitArray(new byte[] { currByte });

        switch (index)
        {
            // HD & US
            case CommandParameter.HEAD:
                if (bi[2])
                    ActiveCommandData.Side = 1;
                else
                    ActiveCommandData.Side = 0;

                ActiveCommandData.UnitSelect = (byte)(currByte & 0x03);
                _flopyDiskDriveCluster.FloppyDiskDriveSlot = ActiveCommandData.UnitSelect;
                break;

            // C
            case CommandParameter.C:
                ActiveCommandData.Cylinder = currByte;
                break;

            // H
            case CommandParameter.H:
                ActiveCommandData.Head = currByte;
                break;

            // R
            case CommandParameter.R:
                ActiveCommandData.Sector = currByte;
                break;

            // N
            case CommandParameter.N:
                ActiveCommandData.SectorSize = currByte;
                break;

            // EOT
            case CommandParameter.EOT:
                ActiveCommandData.EOT = currByte;
                break;

            // GPL
            case CommandParameter.GPL:
                ActiveCommandData.Gap3Length = currByte;
                break;

            // DTL
            case CommandParameter.DTL:
                ActiveCommandData.DTL = currByte;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Clears the result buffer
    /// </summary>
    public void ClearResultBuffer()
    {
        for (int i = 0; i < ResultBuffer.Length; i++)
        {
            ResultBuffer[i] = 0;
        }
    }

    /// <summary>
    /// Clears the result buffer
    /// </summary>
    public void ClearExecBuffer()
    {
        for (int i = 0; i < ExecutionBuffer.Length; i++)
        {
            ExecutionBuffer[i] = 0;
        }
    }

    /// <summary>
    /// Populates the result status registers
    /// </summary>
    private void CommitResultStatus()
    {
        // check for read diag
        if (ActiveCommand.CommandCode == 0x02)
        {
            // commit to result buffer
            ResultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
            ResultBuffer[(int)CommandResultParameter.ST1] = (byte)_statusRegisters1;

            return;
        }

        // check for error bits
        if (_statusRegisters1.HasFlag(StatusRegisters1.DE) ||
            _statusRegisters1.HasFlag(StatusRegisters1.MA) ||
            _statusRegisters1.HasFlag(StatusRegisters1.ND) ||
            _statusRegisters1.HasFlag(StatusRegisters1.NW) ||
            _statusRegisters1.HasFlag(StatusRegisters1.OR) ||
            _statusRegisters2.HasFlag(StatusRegisters2.BC) ||
            _statusRegisters2.HasFlag(StatusRegisters2.CM) ||
            _statusRegisters2.HasFlag(StatusRegisters2.DD) ||
            _statusRegisters2.HasFlag(StatusRegisters2.MD) ||
            _statusRegisters2.HasFlag(StatusRegisters2.SN) ||
            _statusRegisters2.HasFlag(StatusRegisters2.WC))
        {
            // error bits set - unset end of track
            _statusRegisters1.UnSetBits(StatusRegisters1.EN);            
        }

        // check for data errors
        if (_statusRegisters1.HasFlag(StatusRegisters1.DE) || _statusRegisters2.HasFlag(StatusRegisters2.DD))
        {
            // unset control mark
            _statusRegisters2.UnSetBits(StatusRegisters2.CM);
        }
        else if (_statusRegisters2.HasFlag(StatusRegisters2.CM))
        {
            // DAM found - unset IC and US0
            _statusRegisters0.UnSetBits(StatusRegisters0.IC_D6);
            _statusRegisters0.UnSetBits(StatusRegisters0.US0);
        }

        // commit to result buffer
        ResultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
        ResultBuffer[(int)CommandResultParameter.ST1] = (byte)_statusRegisters1;
        ResultBuffer[(int)CommandResultParameter.ST2] = (byte)_statusRegisters2;
    }

    /// <summary>
    /// Populates the result CHRN values
    /// </summary>
    private void CommitResultCHRN()
    {
        ResultBuffer[(int)CommandResultParameter.C] = ActiveCommandData.Cylinder;
        ResultBuffer[(int)CommandResultParameter.H] = ActiveCommandData.Head;
        ResultBuffer[(int)CommandResultParameter.R] = ActiveCommandData.Sector;
        ResultBuffer[(int)CommandResultParameter.N] = ActiveCommandData.SectorSize;
    }

    /// <summary>
    /// Moves active phase into idle
    /// </summary>
    public void SetPhaseIdle()
    {
        ActivePhase = ControllerCommandPhase.Idle;

        // active direction
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO);
        // CB
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.CB);
        // RQM
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.RQM);

        CommandParameterIndex = 0;
        ResultBufferCounter = 0;
    }




    /// <summary>
    /// Searches for the requested sector
    /// </summary>
    private FloppyDisk.Sector? GetSector()
    {
        if (ActiveFloppyDiskDrive?.Disk?.DiskTracks == null)
        {
            return null;
        }

        FloppyDisk.Sector? result = null;

        // get the current track
        var trk = ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveFloppyDiskDrive.TrackIndex];

        // get the current sector index
        int index = this.ActiveFloppyDiskDrive.SectorIndex;

        // make sure this index exists
        if (index > trk.Sectors.Length)
        {
            index = 0;
        }

        // index hole count
        int iHole = 0;

        // loop through the sectors in a track
        // the loop ends with either the sector being found
        // or the index hole being passed twice
        while (iHole <= 2)
        {
            // does the requested sector match the current sector
            if (trk.Sectors[index].SectorIDInfo.C == ActiveCommandData.Cylinder &&
                trk.Sectors[index].SectorIDInfo.H == ActiveCommandData.Head &&
                trk.Sectors[index].SectorIDInfo.R == ActiveCommandData.Sector &&
                trk.Sectors[index].SectorIDInfo.N == ActiveCommandData.SectorSize)
            {
                // sector has been found
                result = trk.Sectors[index];

                _statusRegisters2.UnSetBits(StatusRegisters2.BC | StatusRegisters2.WC);
                break;
            }

            // check for bad cylinder
            if (trk.Sectors[index].SectorIDInfo.C == 255)
            {
                _statusRegisters2.SetBits(StatusRegisters2.BC);
            }
            // check for no cylinder
            else if (trk.Sectors[index].SectorIDInfo.C != ActiveCommandData.Cylinder)
            {
                _statusRegisters2.SetBits(StatusRegisters2.WC);
            }

            // incrememnt sector index
            index++;

            // have we reached the index hole?
            if (trk.Sectors.Length <= index)
            {
                // wrap around
                index = 0;
                iHole++;
            }
        }

        // search loop has completed and the sector may or may not have been found

        // bad cylinder detected?
        if (_statusRegisters2.HasFlag(StatusRegisters2.BC))
        {
            // remove WC
            _statusRegisters2.UnSetBits(StatusRegisters2.WC);
        }

        // update sectorindex on drive
        ActiveFloppyDiskDrive.SectorIndex = index;

        return result;
    }
}
