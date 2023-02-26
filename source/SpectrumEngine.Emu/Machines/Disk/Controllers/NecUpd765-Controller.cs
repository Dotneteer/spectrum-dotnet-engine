using SpectrumEngine.Emu.Extensions;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static SpectrumEngine.Emu.Machines.Disk.Controllers.NecUpd765;

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
    private CommandState ActiveCommandState = new CommandState();

    /// <summary>
    /// Current active phase controller
    /// </summary>
    private Phase ActivePhase = Phase.Command;

    /// <summary>
    /// Command buffer
    /// </summary>
    private byte[] CommandBuffer = new byte[9];
    // TODO: DGZornoza: debug
    private void SetCommandBuffer(int index, byte value)
    {
        //if (index == 1)
        //{
        //    Debugger.Break();
        //}
        CommandBuffer[index] = value;
    }

    /// <summary>
    /// Current index in command buffer
    /// </summary>
    private int CommandBufferCounter = 0;

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
    /// Keeps track of the current SRT state
    /// </summary>
    private int SRT_Counter;

    /// <summary>
    /// Head Unload Time (supplied via the specify command)
    /// HUT stands for the head unload time after a Read or Write operation has occurred 
    /// (16 to 240 ms in 16 ms Increments)
    /// </summary>
    private int HUT;

    /// <summary>
    /// Keeps track of the current HUT state
    /// </summary>
    private int HUT_Counter;

    /// <summary>
    /// Head load Time (supplied via the specify command)
    /// HLT stands for the head load time in the FDD (2 to 254 ms in 2 ms Increments)
    /// </summary>
    private int HLT;

    /// <summary>
    /// Keeps track of the current HLT state
    /// </summary>
    private int HLT_Counter;

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
    private void SetResultBuffer(int index, byte value)
    {
        ResultBuffer[index] = value;
    }

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
    /// Interrupt result buffer
    /// Persists (and returns when needed) the last result data when a sense interrupt status command happens
    /// </summary>
    private byte[] InterruptResultBuffer = new byte[2];

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
    /// Delay for reading sector
    /// </summary>
    private int SectorDelayCounter = 0;

    /// <summary>
    /// The phyical sector ID
    /// </summary>
    private int SectorID = 0;

    /// <summary>
    /// Counter for index pulses
    /// </summary>
    private int IndexPulseCounter;

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
            case Phase.Idle:
                _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO | MainStatusRegisters.CB | MainStatusRegisters.EXM);
                break;

            case Phase.Command:
                _mainStatusRegisters.SetBits(MainStatusRegisters.CB);
                _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO | MainStatusRegisters.EXM);
                break;

            case Phase.Execution:
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
                    ActivePhase = Phase.Result;

                    // reset the overun counter
                    OverrunCounter = 0;
                }

                break;

            case Phase.Result:
                _mainStatusRegisters.SetBits(MainStatusRegisters.DIO | MainStatusRegisters.CB);
                _mainStatusRegisters.UnSetBits(MainStatusRegisters.EXM);
                break;
        }

        //if (!CheckTiming())
        //{
        //    UnSetBit(MSR_EXM, ref StatusMain);
        //}

        return _mainStatusRegisters;
    }

    //private int testCount = 0;
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
            case Phase.Execution:
                // reset overrun counter
                OverrunCounter = 0;

                // execute read
                ActiveCommand.CommandHandler();

                res = LastSectorDataReadByte;

                if (ExecutionBufferCounter <= 0)
                {
                    // end of execution phase
                    ActivePhase = Phase.Result;
                }

                return res;

            case Phase.Result:

                DriveLight = false;

                ActiveCommand.CommandHandler();

                // result byte reading
                res = ResultBuffer[ResultBufferCounter];

                // increment result counter
                ResultBufferCounter++;

                if (ResultBufferCounter >= ResultLength)
                {
                    ActivePhase = Phase.Idle;
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
            case Phase.Idle:
                ParseCommandByte(data);
                break;
            //// we are in command phase
            case Phase.Command:
                // attempt to process this parameter byte
                //ProcessCommand(data);      
                ActiveCommand.CommandHandler();
                break;
            //// we are in execution phase
            case Phase.Execution:
                // CPU is going to be sending data bytes to the FDC to be written to disk

                // store the byte
                LastSectorDataWriteByte = data;
                ActiveCommand.CommandHandler();

                if (ExecutionBufferCounter <= 0)
                {
                    // end of execution phase
                    ActivePhase = Phase.Result;
                }

                break;
            //// result phase
            case Phase.Result:
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
        CommandBufferCounter = 0;
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
            CmdIndex = Commands.FindIndex(a => a.CommandCode == cmdByte);

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

            /*
                if ((CMD_FLAG_MF && !ActiveCommand.MF) ||
                    (CMD_FLAG_MT && !ActiveCommand.MT) ||
                    (CMD_FLAG_SK && !ActiveCommand.SK))
                {
                    // command byte included spurious bit 5,6 or 7 flags
                    CMDIndex = CommandList.Count - 1;
                }
                */
        }

        CommandBufferCounter = 0;
        ResultBufferCounter = 0;

        // there will now be an active command set
        // move to command phase
        ActivePhase = Phase.Command;

        /*
            // check for invalid SIS
            if (ActiveInterrupt == InterruptState.None && CMDIndex == CC_SENSE_INTSTATUS)
            {
                CMDIndex = CC_INVALID;
                //ActiveCommand.CommandDelegate(InstructionState.StartResult);
            }
            */

        // set reslength
        ResultLength = ActiveCommand.ResultBytesCount;

        // if there are no expected param bytes to receive - go ahead and run the command
        if (ActiveCommand.ParameterBytesCount == 0)
        {
            ActivePhase = Phase.Execution;
            ActiveCommand.CommandHandler();
        }

        return true;
    }

    /// <summary>
    /// Parses the first 5 command argument bytes that are of the standard format
    /// </summary>
    private void ParseParamByteStandard(CommandParameter index)
    {
        byte currByte = CommandBuffer[(int)index];
        BitArray bi = new BitArray(new byte[] { currByte });

        switch (index)
        {
            // HD & US
            case CommandParameter.HEAD:
                if (bi[2])
                    ActiveCommandState.Side = 1;
                else
                    ActiveCommandState.Side = 0;

                ActiveCommandState.UnitSelect = (byte)(currByte & 0x03);
                _flopyDiskDriveCluster.FloppyDiskDriveSlot = ActiveCommandState.UnitSelect;
                break;

            // C
            case CommandParameter.C:
                ActiveCommandState.Cylinder = currByte;
                break;

            // H
            case CommandParameter.H:
                ActiveCommandState.Head = currByte;
                break;

            // R
            case CommandParameter.R:
                ActiveCommandState.Sector = currByte;
                break;

            // N
            case CommandParameter.N:
                ActiveCommandState.SectorSize = currByte;
                break;

            // EOT
            case CommandParameter.EOT:
                ActiveCommandState.EOT = currByte;
                break;

            // GPL
            case CommandParameter.GPL:
                ActiveCommandState.Gap3Length = currByte;
                break;

            // DTL
            case CommandParameter.DTL:
                ActiveCommandState.DTL = currByte;
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
            //ResultBuffer[i] = 0;
            SetResultBuffer(i, 0);
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
            //ResultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
            SetResultBuffer((int)CommandResultParameter.ST0, (byte)_statusRegisters0);
            //ResultBuffer[(int)CommandResultParameter.ST1] = (byte)_statusRegisters1;
            SetResultBuffer((int)CommandResultParameter.ST1, (byte)_statusRegisters1);

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
        //ResultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
        SetResultBuffer((int)CommandResultParameter.ST0, (byte)_statusRegisters0);
        //ResultBuffer[(int)CommandResultParameter.ST1] = (byte)_statusRegisters1;
        SetResultBuffer((int)CommandResultParameter.ST1, (byte)_statusRegisters1);
        //ResultBuffer[(int)CommandResultParameter.ST2] = (byte)_statusRegisters2;
        SetResultBuffer((int)CommandResultParameter.ST2, (byte)_statusRegisters2);
    }

    /// <summary>
    /// Populates the result CHRN values
    /// </summary>
    private void CommitResultCHRN()
    {
        //ResultBuffer[(int)CommandResultParameter.C] = ActiveCommandParams.Cylinder;
        SetResultBuffer((int)CommandResultParameter.C, ActiveCommandState.Cylinder);
        //ResultBuffer[(int)CommandResultParameter.H] = ActiveCommandParams.Head;
        SetResultBuffer((int)CommandResultParameter.H, ActiveCommandState.Head);
        //ResultBuffer[(int)CommandResultParameter.R] = ActiveCommandParams.Sector;
        SetResultBuffer((int)CommandResultParameter.R, ActiveCommandState.Sector);
        //ResultBuffer[(int)CommandResultParameter.N] = ActiveCommandParams.SectorSize;
        SetResultBuffer((int)CommandResultParameter.N, ActiveCommandState.SectorSize);
    }

    /// <summary>
    /// Moves active phase into idle
    /// </summary>
    public void SetPhase_Idle()
    {
        ActivePhase = Phase.Idle;

        // active direction
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO);
        // CB
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.CB);
        // RQM
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.RQM);

        CommandBufferCounter = 0;
        ResultBufferCounter = 0;
    }

    /// <summary>
    /// Moves to result phase
    /// </summary>
    public void SetPhase_Result()
    {
        ActivePhase = Phase.Result;

        // active direction
        _mainStatusRegisters.SetBits(MainStatusRegisters.DIO);
        // CB
        _mainStatusRegisters.SetBits(MainStatusRegisters.CB);
        // RQM
        _mainStatusRegisters.SetBits(MainStatusRegisters.RQM);
        // EXM
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.EXM);

        CommandBufferCounter = 0;
        ResultBufferCounter = 0;
    }

    /// <summary>
    /// Moves to command phase
    /// </summary>
    public void SetPhase_Command()
    {
        ActivePhase = Phase.Command;

        // default 0x80 - just RQM
        _mainStatusRegisters.SetBits(MainStatusRegisters.RQM);
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO);
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.CB);
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.EXM);

        CommandBufferCounter = 0;
        ResultBufferCounter = 0;
    }

    /// <summary>
    /// Moves to execution phase
    /// </summary>
    public void SetPhase_Execution()
    {
        ActivePhase = Phase.Execution;

        // EXM
        _mainStatusRegisters.SetBits(MainStatusRegisters.EXM);
        // CB
        _mainStatusRegisters.SetBits(MainStatusRegisters.CB);
        // RQM
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.RQM);

        CommandBufferCounter = 0;
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
            if (trk.Sectors[index].SectorIDInfo.C == ActiveCommandState.Cylinder &&
                trk.Sectors[index].SectorIDInfo.H == ActiveCommandState.Head &&
                trk.Sectors[index].SectorIDInfo.R == ActiveCommandState.Sector &&
                trk.Sectors[index].SectorIDInfo.N == ActiveCommandState.SectorSize)
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
            else if (trk.Sectors[index].SectorIDInfo.C != ActiveCommandState.Cylinder)
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
