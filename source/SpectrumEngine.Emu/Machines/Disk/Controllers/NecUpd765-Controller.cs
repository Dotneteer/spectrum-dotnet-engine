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
    private List<Command> CommandList;

    /// <summary>
    /// Active command parameters
    /// </summary>
    private CommandParams ActiveCommandParams = new CommandParams();

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
        WriteDebug2($"SetResultBuffer: {index} - {value}");
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
    /// Current selected command in <see cref="CommandList"/>
    /// </summary>
    private int _cmdIndex;
    public int CmdIndex
    {
        get => _cmdIndex;
        set
        {
            _cmdIndex = value;
            ActiveCommand = CommandList[_cmdIndex];
        }
    }

    /// <summary>
    /// The currently active command
    /// </summary>
    private Command ActiveCommand;

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
    /// Read Data
    /// COMMAND:    8 parameter bytes
    /// EXECUTION:  Data transfer between FDD and FDC
    /// RESULT:     7 result bytes
    /// </summary>
    private void ReadData()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:

                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received - setup for execution phase

                    // clear exec buffer and status registers
                    ClearExecBuffer();
                    _statusRegisters0 = 0;
                    _statusRegisters1 = 0;
                    _statusRegisters2 = 0;
                    _statusRegisters3 = 0;

                    // temp sector index
                    byte secIdx = ActiveCommandParams.Sector;

                    // do we have a valid disk inserted?
                    if (!ActiveFloppyDiskDrive.IsReady)
                    {
                        // no disk, no tracks or motor is not on
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                        CommitResultCHRN();
                        CommitResultStatus();

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    int buffPos = 0;
                    int sectorSize = 0;
                    int maxTransferCap = 0;

                    // calculate requested size of data required
                    if (ActiveCommandParams.SectorSize == 0)
                    {
                        // When N=0, then DTL defines the data length which the FDC must treat as a sector. If DTL is smaller than the actual 
                        // data length in a sector, the data beyond DTL in the sector is not sent to the Data Bus. The FDC reads (internally) 
                        // the complete sector performing the CRC check and, depending upon the manner of command termination, may perform 
                        // a Multi-Sector Read Operation.
                        sectorSize = ActiveCommandParams.DTL;

                        // calculate maximum transfer capacity
                        if (!CMD_FLAG_MF)
                        {
                            maxTransferCap = 3328;
                        }

                        if (maxTransferCap == 0) { }
                    }
                    else
                    {
                        // When N is non - zero, then DTL has no meaning and should be set to ffh
                        ActiveCommandParams.DTL = 0xFF;

                        // calculate maximum transfer capacity
                        switch (ActiveCommandParams.SectorSize)
                        {
                            case 1:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 6656;
                                else
                                    maxTransferCap = 3840;
                                break;
                            case 2:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 7680;
                                else
                                    maxTransferCap = 4096;
                                break;
                            case 3:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 8192;
                                else
                                    maxTransferCap = 4096;
                                break;
                        }

                        sectorSize = 0x80 << ActiveCommandParams.SectorSize;
                    }

                    // get the current track
                    var track = ActiveFloppyDiskDrive.Disk?.DiskTracks?.FirstOrDefault(a => a.TrackNumber == ActiveFloppyDiskDrive.CurrentTrackId);

                    if (track == null || track.NumberOfSectors <= 0)
                    {
                        // track could not be found
                        // TODO: abnormal terminacion del comando (DRY)
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                        CommitResultCHRN();
                        CommitResultStatus();

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    FloppyDisk.Sector sector = null;

                    // sector read loop
                    for (; ; )
                    {
                        bool terminate = false;

                        // lookup the sector
                        sector = GetSector();

                        if (sector == null)
                        {
                            // sector was not found after two passes of the disk index hole
                            _statusRegisters0.SetAbnormalTerminationCommand();
                            _statusRegisters1.SetBits(StatusRegisters1.ND);

                            // result requires the actual track id, rather than the sector track id
                            ActiveCommandParams.Cylinder = track.TrackNumber;

                            CommitResultCHRN();
                            CommitResultStatus();
                            ActivePhase = Phase.Result;
                            break;
                        }

                        // sector ID was found on this track

                        // get status regs from sector
                        _statusRegisters1 = (StatusRegisters1)sector.Status1;
                        _statusRegisters2 = (StatusRegisters2)sector.Status2;

                        // we don't need EN
                        _statusRegisters1.UnSetBits(StatusRegisters1.EN);

                        // If SK=1, the FDC skips the sector with the Deleted Data Address Mark and reads the next sector. 
                        // The CRC bits in the deleted data field are not checked when SK=1
                        if (CMD_FLAG_SK && _statusRegisters2.HasFlag(StatusRegisters2.CM))
                        {
                            if (ActiveCommandParams.Sector != ActiveCommandParams.EOT)
                            {
                                // increment the sector ID and search again
                                ActiveCommandParams.Sector++;
                                continue;
                            }
                            else
                            {
                                // no execution phase
                                _statusRegisters0.SetAbnormalTerminationCommand();

                                // result requires the actual track id, rather than the sector track id
                                ActiveCommandParams.Cylinder = track.TrackNumber;

                                CommitResultCHRN();
                                CommitResultStatus();
                                ActivePhase = Phase.Result;
                                break;
                            }
                        }

                        // read the sector
                        for (int i = 0; i < sector.DataLen; i++)
                        {
                            ExecutionBuffer[buffPos++] = sector.ActualData[i];
                        }

                        // mark the sector read
                        sector.SectorReadCompleted();

                        // any CRC errors?
                        if (_statusRegisters1.HasFlag(StatusRegisters1.DE) || _statusRegisters2.HasFlag(StatusRegisters2.DD))
                        {
                            _statusRegisters0.SetAbnormalTerminationCommand();

                            terminate = true;
                        }

                        if (!CMD_FLAG_SK && _statusRegisters2.HasFlag(StatusRegisters2.CM))
                        {
                            // deleted address mark was detected with NO skip flag set
                            ActiveCommandParams.EOT = ActiveCommandParams.Sector;
                            _statusRegisters2.SetBits(StatusRegisters2.CM);
                            _statusRegisters0.SetAbnormalTerminationCommand();

                            terminate = true;
                        }

                        if (sector.SectorID == ActiveCommandParams.EOT || terminate)
                        {
                            // this was the last sector to read
                            // or termination requested
                            _statusRegisters1.SetBits(StatusRegisters1.EN);

                            int keyIndex = 0;
                            for (int i = 0; i < track.Sectors.Length; i++)
                            {
                                if (track.Sectors[i].SectorID == sector.SectorID)
                                {
                                    keyIndex = i;
                                    break;
                                }
                            }

                            if (keyIndex == track.Sectors.Length - 1)
                            {
                                // last sector on the cylinder, set EN
                                _statusRegisters1.SetBits(StatusRegisters1.EN);

                                // increment cylinder
                                ActiveCommandParams.Cylinder++;

                                // reset sector
                                ActiveCommandParams.Sector = sector.SectorID; // 1;
                                ActiveFloppyDiskDrive.SectorIndex = 0;
                            }
                            else
                            {
                                ActiveFloppyDiskDrive.SectorIndex++;
                            }

                            _statusRegisters0.SetAbnormalTerminationCommand();

                            // result requires the actual track id, rather than the sector track id
                            ActiveCommandParams.Cylinder = track.TrackNumber;

                            CommitResultCHRN();
                            CommitResultStatus();
                            ActivePhase = Phase.Execution;
                            break;
                        }
                        else
                        {
                            // continue with multi-sector read operation
                            ActiveCommandParams.Sector++;
                            //ActiveFloppyDiskDrive.SectorIndex++;
                        }
                    }

                    if (ActivePhase == Phase.Execution)
                    {
                        ExecutionLength = buffPos;
                        ExecutionBufferCounter = buffPos;

                        DriveLight = true;
                    }
                }

                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:

                var index = ExecutionLength - ExecutionBufferCounter;

                LastSectorDataReadByte = ExecutionBuffer[index];

                OverrunCounter--;
                ExecutionBufferCounter--;

                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Read Deleted Data
    /// COMMAND:    8 parameter bytes
    /// EXECUTION:  Data transfer between the FDD and FDC
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_ReadDeletedData()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received - setup for execution phase

                    // clear exec buffer and status registers
                    ClearExecBuffer();
                    _statusRegisters0 = 0;
                    _statusRegisters1 = 0;
                    _statusRegisters2 = 0;
                    _statusRegisters3 = 0;

                    // temp sector index
                    byte secIdx = ActiveCommandParams.Sector;

                    // do we have a valid disk inserted?
                    if (!ActiveFloppyDiskDrive.IsReady)
                    {
                        // no disk, no tracks or motor is not on
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);                        

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    int buffPos = 0;
                    int sectorSize = 0;
                    int maxTransferCap = 0;
                    if (maxTransferCap > 0) { }

                    // calculate requested size of data required
                    if (ActiveCommandParams.SectorSize == 0)
                    {
                        // When N=0, then DTL defines the data length which the FDC must treat as a sector. If DTL is smaller than the actual 
                        // data length in a sector, the data beyond DTL in the sector is not sent to the Data Bus. The FDC reads (internally) 
                        // the complete sector performing the CRC check and, depending upon the manner of command termination, may perform 
                        // a Multi-Sector Read Operation.
                        sectorSize = ActiveCommandParams.DTL;

                        // calculate maximum transfer capacity
                        if (!CMD_FLAG_MF)
                            maxTransferCap = 3328;
                    }
                    else
                    {
                        // When N is non - zero, then DTL has no meaning and should be set to ffh
                        ActiveCommandParams.DTL = 0xFF;

                        // calculate maximum transfer capacity
                        switch (ActiveCommandParams.SectorSize)
                        {
                            case 1:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 6656;
                                else
                                    maxTransferCap = 3840;
                                break;
                            case 2:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 7680;
                                else
                                    maxTransferCap = 4096;
                                break;
                            case 3:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 8192;
                                else
                                    maxTransferCap = 4096;
                                break;
                        }

                        sectorSize = 0x80 << ActiveCommandParams.SectorSize;
                    }

                    // get the current track
                    var track = ActiveFloppyDiskDrive.Disk.DiskTracks.FirstOrDefault(a => a.TrackNumber == ActiveFloppyDiskDrive.CurrentTrackId);

                    if (track == null || track.NumberOfSectors <= 0)
                    {
                        // track could not be found
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                        _statusRegisters0.SetBits(StatusRegisters0.NR);

                        CommitResultCHRN();
                        CommitResultStatus();

                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    FloppyDisk.Sector sector = null;

                    // sector read loop
                    for (; ; )
                    {
                        bool terminate = false;

                        // lookup the sector
                        sector = GetSector();

                        if (sector == null)
                        {
                            // sector was not found after two passes of the disk index hole
                            _statusRegisters0.SetAbnormalTerminationCommand();
                            _statusRegisters1.SetBits(StatusRegisters1.ND);

                            // result requires the actual track id, rather than the sector track id
                            ActiveCommandParams.Cylinder = track.TrackNumber;

                            CommitResultCHRN();
                            CommitResultStatus();
                            ActivePhase = Phase.Result;
                            break;
                        }

                        // sector ID was found on this track

                        // get status regs from sector
                        _statusRegisters1 = (StatusRegisters1)sector.Status1;
                        _statusRegisters2 = (StatusRegisters2)sector.Status2;

                        // we don't need EN
                        _statusRegisters1.UnSetBits(StatusRegisters1.EN);

                        // invert CM for read deleted data command
                        if (_statusRegisters2.HasFlag(StatusRegisters2.CM))
                        {
                            _statusRegisters2.UnSetBits(StatusRegisters2.CM);
                        }
                        else
                        {
                            _statusRegisters2.SetBits(StatusRegisters2.CM);
                        }

                        // skip flag is set and no DAM found
                        if (CMD_FLAG_SK && _statusRegisters2.HasFlag(StatusRegisters2.CM))
                        {
                            if (ActiveCommandParams.Sector != ActiveCommandParams.EOT)
                            {
                                // increment the sector ID and search again
                                ActiveCommandParams.Sector++;
                                continue;
                            }
                            else
                            {
                                // no execution phase
                                _statusRegisters0.SetAbnormalTerminationCommand();

                                // result requires the actual track id, rather than the sector track id
                                ActiveCommandParams.Cylinder = track.TrackNumber;

                                CommitResultCHRN();
                                CommitResultStatus();
                                ActivePhase = Phase.Result;
                                break;
                            }
                        }
                        // we can read this sector
                        else
                        {
                            // if DAM is not set this will be the last sector to read
                            if (_statusRegisters2.HasFlag(StatusRegisters2.CM))
                            {
                                ActiveCommandParams.EOT = ActiveCommandParams.Sector;
                            }

                            //if (!CMD_FLAG_SK && !_statusRegisters2.HasFlag(StatusRegisters2.CM) &&
                            //    ActiveFloppyDiskDrive.Disk.Protection == ProtectionType.PaulOwens)
                            //{
                            //    ActiveCommandParams.EOT = ActiveCommandParams.Sector;
                            //    _statusRegisters2.SetBits(StatusRegisters2.CM);
                            //    _statusRegisters0.SetAbnormalTerminationCommand();
                            //    terminate = true;
                            //}

                            // read the sector
                            for (int i = 0; i < sectorSize; i++)
                            {
                                ExecutionBuffer[buffPos++] = sector.ActualData[i];
                            }

                            // mark the sector read
                            sector.SectorReadCompleted();

                            if (sector.SectorID == ActiveCommandParams.EOT)
                            {
                                // this was the last sector to read
                                _statusRegisters1.SetBits(StatusRegisters1.EN);

                                int keyIndex = 0;
                                for (int i = 0; i < track.Sectors.Length; i++)
                                {
                                    if (track.Sectors[i].SectorID == sector.SectorID)
                                    {
                                        keyIndex = i;
                                        break;
                                    }
                                }

                                if (keyIndex == track.Sectors.Length - 1)
                                {
                                    // last sector on the cylinder, set EN
                                    _statusRegisters1.SetBits(StatusRegisters1.EN);

                                    // increment cylinder
                                    ActiveCommandParams.Cylinder++;

                                    // reset sector
                                    ActiveCommandParams.Sector = 1;
                                    ActiveFloppyDiskDrive.SectorIndex = 0;
                                }
                                else
                                {
                                    ActiveFloppyDiskDrive.SectorIndex++;
                                }

                                _statusRegisters0.SetAbnormalTerminationCommand();

                                // result requires the actual track id, rather than the sector track id
                                ActiveCommandParams.Cylinder = track.TrackNumber;

                                // remove CM (appears to be required to defeat Alkatraz copy protection)
                                _statusRegisters2.UnSetBits(StatusRegisters2.CM);                                

                                CommitResultCHRN();
                                CommitResultStatus();
                                ActivePhase = Phase.Execution;
                                break;
                            }
                            else
                            {
                                // continue with multi-sector read operation
                                ActiveCommandParams.Sector++;
                                //ActiveFloppyDiskDrive.SectorIndex++;
                            }
                        }
                    }

                    if (ActivePhase == Phase.Execution)
                    {
                        ExecutionLength = buffPos;
                        ExecutionBufferCounter = buffPos;
                        DriveLight = true;
                    }
                }
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                var index = ExecutionLength - ExecutionBufferCounter;

                LastSectorDataReadByte = ExecutionBuffer[index];

                OverrunCounter--;
                ExecutionBufferCounter--;

                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Read Diagnostic (read track)
    /// COMMAND:    8 parameter bytes
    /// EXECUTION:  Data transfer between FDD and FDC. FDC reads all data fields from index hole to EDT
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_ReadDiagnostic()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:

                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received - setup for execution phase

                    // clear exec buffer and status registers
                    ClearExecBuffer();
                    _statusRegisters0 = 0;
                    _statusRegisters1 = 0;
                    _statusRegisters2 = 0;
                    _statusRegisters3 = 0;

                    // temp sector index
                    byte secIdx = ActiveCommandParams.Sector;

                    // do we have a valid disk inserted?
                    if (!ActiveFloppyDiskDrive.IsReady)
                    {
                        // no disk, no tracks or motor is not on
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    int buffPos = 0;
                    int sectorSize = 0;
                    int maxTransferCap = 0;
                    if (maxTransferCap > 0) { }

                    // calculate requested size of data required
                    if (ActiveCommandParams.SectorSize == 0)
                    {
                        // When N=0, then DTL defines the data length which the FDC must treat as a sector. If DTL is smaller than the actual 
                        // data length in a sector, the data beyond DTL in the sector is not sent to the Data Bus. The FDC reads (internally) 
                        // the complete sector performing the CRC check and, depending upon the manner of command termination, may perform 
                        // a Multi-Sector Read Operation.
                        sectorSize = ActiveCommandParams.DTL;

                        // calculate maximum transfer capacity
                        if (!CMD_FLAG_MF)
                            maxTransferCap = 3328;
                    }
                    else
                    {
                        // When N is non - zero, then DTL has no meaning and should be set to ffh
                        ActiveCommandParams.DTL = 0xFF;

                        // calculate maximum transfer capacity
                        switch (ActiveCommandParams.SectorSize)
                        {
                            case 1:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 6656;
                                else
                                    maxTransferCap = 3840;
                                break;
                            case 2:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 7680;
                                else
                                    maxTransferCap = 4096;
                                break;
                            case 3:
                                if (CMD_FLAG_MF)
                                    maxTransferCap = 8192;
                                else
                                    maxTransferCap = 4096;
                                break;
                        }

                        sectorSize = 0x80 << ActiveCommandParams.SectorSize;
                    }

                    // get the current track
                    var track = ActiveFloppyDiskDrive.Disk.DiskTracks.FirstOrDefault(a => a.TrackNumber == ActiveFloppyDiskDrive.CurrentTrackId);

                    if (track == null || track.NumberOfSectors <= 0)
                    {
                        // track could not be found
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                        CommitResultCHRN();
                        CommitResultStatus();

                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    //FloppyDisk.Sector sector = null;
                    ActiveFloppyDiskDrive.SectorIndex = 0;

                    int secCount = 0;

                    // read the whole track
                    for (int i = 0; i < track.Sectors.Length; i++)
                    {
                        if (secCount >= ActiveCommandParams.EOT)
                        {
                            break;
                        }

                        var sec = track.Sectors[i];
                        for (int b = 0; b < sec.ActualData.Length; b++)
                        {
                            ExecutionBuffer[buffPos++] = sec.ActualData[b];
                        }

                        // mark the sector read
                        sec.SectorReadCompleted();

                        // end of sector - compare IDs
                        if (sec.TrackNumber != ActiveCommandParams.Cylinder ||
                            sec.SideNumber != ActiveCommandParams.Head ||
                            sec.SectorID != ActiveCommandParams.Sector ||
                            sec.SectorSize != ActiveCommandParams.SectorSize)
                        {
                            _statusRegisters1.SetBits(StatusRegisters1.ND);
                        }

                        secCount++;
                        ActiveFloppyDiskDrive.SectorIndex = i;
                    }

                    if (secCount == ActiveCommandParams.EOT)
                    {
                        // this was the last sector to read
                        // or termination requested

                        int keyIndex = 0;
                        for (int i = 0; i < track.Sectors.Length; i++)
                        {
                            if (track.Sectors[i].SectorID == track.Sectors[ActiveFloppyDiskDrive.SectorIndex].SectorID)
                            {
                                keyIndex = i;
                                break;
                            }
                        }

                        if (keyIndex == track.Sectors.Length - 1)
                        {
                            // last sector on the cylinder, set EN
                            _statusRegisters1.SetBits(StatusRegisters1.EN);

                            // increment cylinder
                            ActiveCommandParams.Cylinder++;

                            // reset sector
                            ActiveCommandParams.Sector = 1;
                            ActiveFloppyDiskDrive.SectorIndex = 0;
                        }
                        else
                        {
                            ActiveFloppyDiskDrive.SectorIndex++;
                        }

                        _statusRegisters0.UnSetBits(StatusRegisters0.IC_D6 | StatusRegisters0.IC_D7);

                        CommitResultCHRN();
                        CommitResultStatus();
                        ActivePhase = Phase.Execution;
                    }

                    if (ActivePhase == Phase.Execution)
                    {
                        ExecutionLength = buffPos;
                        ExecutionBufferCounter = buffPos;

                        DriveLight = true;
                    }
                }

                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:

                var index = ExecutionLength - ExecutionBufferCounter;

                LastSectorDataReadByte = ExecutionBuffer[index];

                OverrunCounter--;
                ExecutionBufferCounter--;

                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Read ID
    /// COMMAND:    1 parameter byte
    /// EXECUTION:  The first correct ID information on the cylinder is stored in the data register
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_ReadID()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:

                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    DriveLight = true;

                    // all parameter bytes received
                    ClearResultBuffer();
                    _statusRegisters0 = 0;
                    _statusRegisters1 = 0;
                    _statusRegisters2 = 0;
                    _statusRegisters3 = 0;

                    // set unit select
                    //SetUnitSelect(ActiveFloppyDiskDrive.ID, ref Status0);

                    // HD should always be 0
                    _statusRegisters0.UnSetBits(StatusRegisters0.HD);

                    if (!ActiveFloppyDiskDrive.IsReady)
                    {
                        // no disk, no tracks or motor is not on
                        // it is at this point the +3 detects whether a disk is present
                        // if not (and after another readid and SIS) it will eventually proceed to loading from tape
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                        // setup the result buffer
                        //ResultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
                        SetResultBuffer((int)CommandResultParameter.ST0, (byte)_statusRegisters0);
                        for (int i = 1; i < 7; i++)
                        {
                            // ResultBuffer[i] = 0;
                            SetResultBuffer(i, 0);
                        }
                            
                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    var track = ActiveFloppyDiskDrive.Disk.DiskTracks.FirstOrDefault(a => a.TrackNumber == ActiveFloppyDiskDrive.CurrentTrackId);

                    if (track != null && track.NumberOfSectors > 0 && track.TrackNumber != 0xff)
                    {
                        // formatted track

                        // is the index out of bounds?
                        if (ActiveFloppyDiskDrive.SectorIndex >= track.NumberOfSectors)
                        {
                            // reset the index
                            ActiveFloppyDiskDrive.SectorIndex = 0;
                        }

                        if (ActiveFloppyDiskDrive.SectorIndex == 0 && ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveFloppyDiskDrive.CurrentTrackId].Sectors.Length > 1)
                        {
                            // looks like readid always skips the first sector on a track
                            ActiveFloppyDiskDrive.SectorIndex++;
                        }

                        // read the sector data
                        var data = track.Sectors[ActiveFloppyDiskDrive.SectorIndex]; //.GetCHRN();
                        //ResultBuffer[(int)CommandResultParameter.C] = data.TrackNumber;
                        SetResultBuffer((int)CommandResultParameter.C, data.TrackNumber);
                        //ResultBuffer[(int)CommandResultParameter.H] = data.SideNumber;
                        SetResultBuffer((int)CommandResultParameter.H, data.SideNumber);
                        //ResultBuffer[(int)CommandResultParameter.R] = data.SectorID;
                        SetResultBuffer((int)CommandResultParameter.R, data.SectorID);
                        //ResultBuffer[(int)CommandResultParameter.N] = data.SectorSize;
                        SetResultBuffer((int)CommandResultParameter.N, data.SectorSize);

                        //ResultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
                        SetResultBuffer((int)CommandResultParameter.ST0, (byte)_statusRegisters0);

                        // check for DAM & CRC
                        //if (data.Status2.Bit(SR2_CM))
                        //SetBit(SR2_CM, ref ResBuffer[RS_ST2]);


                        // increment the current sector
                        ActiveFloppyDiskDrive.SectorIndex++;

                        // is the index out of bounds?
                        if (ActiveFloppyDiskDrive.SectorIndex >= track.NumberOfSectors)
                        {
                            // reset the index
                            ActiveFloppyDiskDrive.SectorIndex = 0;
                        }
                    }
                    else
                    {
                        // unformatted track?
                        CommitResultCHRN();

                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                        //ResultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
                        SetResultBuffer((int)CommandResultParameter.ST0, (byte)_statusRegisters0);
                        //ResultBuffer[(int)CommandResultParameter.ST1] = 0x01;
                        SetResultBuffer((int)CommandResultParameter.ST1, 0x01);
                    }

                    ActivePhase = Phase.Result;
                }

                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Write Data
    /// COMMAND:    8 parameter bytes
    /// EXECUTION:  Data transfer between FDC and FDD
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_WriteData()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:

                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received - setup for execution phase

                    // clear exec buffer and status registers
                    ClearExecBuffer();
                    _statusRegisters0 = 0;
                    _statusRegisters1 = 0;
                    _statusRegisters2 = 0;
                    _statusRegisters3 = 0;

                    // temp sector index
                    byte secIdx = ActiveCommandParams.Sector;

                    // do we have a valid disk inserted?
                    if (!ActiveFloppyDiskDrive.IsReady)
                    {
                        // no disk, no tracks or motor is not on
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    // check write protect tab
                    if (ActiveFloppyDiskDrive.IsWriteProtect)
                    {
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                        _statusRegisters1.SetBits(StatusRegisters1.NW);

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }
                    else
                    {

                        // calculate the number of bytes to write
                        int byteCounter = 0;
                        byte startSecID = ActiveCommandParams.Sector;
                        byte endSecID = ActiveCommandParams.EOT;
                        bool lastSec = false;

                        // get the first sector
                        var track = ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveCommandParams.Cylinder];
                        //int secIndex = 0;
                        for (int s = 0; s < track.Sectors.Length; s++)
                        {
                            if (track.Sectors[s].SectorID == endSecID)
                                lastSec = true;

                            for (int i = 0; i < 0x80 << ActiveCommandParams.SectorSize; i++)
                            {
                                byteCounter++;

                                if (i == (0x80 << ActiveCommandParams.SectorSize) - 1 && lastSec)
                                {
                                    break;
                                }
                            }

                            if (lastSec)
                                break;
                        }

                        ExecutionBufferCounter = byteCounter;
                        ExecutionLength = byteCounter;
                        ActivePhase = Phase.Execution;
                        DriveLight = true;
                        break;
                    }
                }

                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:

                var index = ExecutionLength - ExecutionBufferCounter;

                ExecutionBuffer[index] = LastSectorDataWriteByte;

                OverrunCounter--;
                ExecutionBufferCounter--;

                if (ExecutionBufferCounter <= 0)
                {
                    int cnt = 0;

                    // all data received
                    byte startSecID = ActiveCommandParams.Sector;
                    byte endSecID = ActiveCommandParams.EOT;
                    bool lastSec = false;
                    var track = ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveCommandParams.Cylinder];
                    //int secIndex = 0;

                    for (int s = 0; s < track.Sectors.Length; s++)
                    {
                        if (cnt == ExecutionLength)
                            break;

                        ActiveCommandParams.Sector = track.Sectors[s].SectorID;

                        if (track.Sectors[s].SectorID == endSecID)
                            lastSec = true;

                        int size = 0x80 << track.Sectors[s].SectorSize;

                        for (int d = 0; d < size; d++)
                        {
                            track.Sectors[s].SectorData[d] = ExecutionBuffer[cnt++];
                        }

                        if (lastSec)
                            break;
                    }

                    _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                    _statusRegisters1.SetBits(StatusRegisters1.EN);

                    CommitResultCHRN();
                    CommitResultStatus();
                }

                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Write ID (format write)
    /// COMMAND:    5 parameter bytes
    /// EXECUTION:  Entire track is formatted
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_WriteID()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:

                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received - setup for execution phase
                    DriveLight = true;

                    // clear exec buffer and status registers
                    ClearExecBuffer();
                    _statusRegisters0 = 0;
                    _statusRegisters1 = 0;
                    _statusRegisters2 = 0;
                    _statusRegisters3 = 0;

                    // temp sector index
                    byte secIdx = ActiveCommandParams.Sector;

                    // do we have a valid disk inserted?
                    if (!ActiveFloppyDiskDrive.IsReady)
                    {
                        // no disk, no tracks or motor is not on
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    // check write protect tab
                    if (ActiveFloppyDiskDrive.IsWriteProtect)
                    {
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                        _statusRegisters1.SetBits(StatusRegisters1.NW);

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }
                    else
                    {
                        // not implemented yet
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                        _statusRegisters1.SetBits(StatusRegisters1.NW);

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }
                }

                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Write Deleted Data
    /// COMMAND:    8 parameter bytes
    /// EXECUTION:  Data transfer between FDC and FDD
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_WriteDeletedData()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:

                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received - setup for execution phase

                    // clear exec buffer and status registers
                    ClearExecBuffer();
                    _statusRegisters0 = 0;
                    _statusRegisters1 = 0;
                    _statusRegisters2 = 0;
                    _statusRegisters3 = 0;

                    // temp sector index
                    byte secIdx = ActiveCommandParams.Sector;                       

                    // do we have a valid disk inserted?
                    if (!ActiveFloppyDiskDrive.IsReady)
                    {
                        // no disk, no tracks or motor is not on
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }

                    // check write protect tab
                    if (ActiveFloppyDiskDrive.IsWriteProtect)
                    {
                        _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                        _statusRegisters1.SetBits(StatusRegisters1.NW);

                        CommitResultCHRN();
                        CommitResultStatus();
                        //ResBuffer[RS_ST0] = Status0;

                        // move to result phase
                        ActivePhase = Phase.Result;
                        break;
                    }
                    else
                    {

                        // calculate the number of bytes to write
                        int byteCounter = 0;
                        byte startSecID = ActiveCommandParams.Sector;
                        byte endSecID = ActiveCommandParams.EOT;
                        bool lastSec = false;

                        // get the first sector
                        var track = ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveCommandParams.Cylinder];
                        //int secIndex = 0;
                        for (int s = 0; s < track.Sectors.Length; s++)
                        {
                            if (track.Sectors[s].SectorID == endSecID)
                                lastSec = true;

                            for (int i = 0; i < 0x80 << ActiveCommandParams.SectorSize; i++)
                            {
                                byteCounter++;

                                if (i == (0x80 << ActiveCommandParams.SectorSize) - 1 && lastSec)
                                {
                                    break;
                                }
                            }

                            if (lastSec)
                                break;
                        }

                        ExecutionBufferCounter = byteCounter;
                        ExecutionLength = byteCounter;
                        ActivePhase = Phase.Execution;
                        DriveLight = true;
                        break;
                    }
                }

                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:

                var index = ExecutionLength - ExecutionBufferCounter;

                ExecutionBuffer[index] = LastSectorDataWriteByte;

                OverrunCounter--;
                ExecutionBufferCounter--;

                if (ExecutionBufferCounter <= 0)
                {
                    int cnt = 0;

                    // all data received
                    byte startSecID = ActiveCommandParams.Sector;
                    byte endSecID = ActiveCommandParams.EOT;
                    bool lastSec = false;
                    var track = ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveCommandParams.Cylinder];
                    //int secIndex = 0;

                    for (int s = 0; s < track.Sectors.Length; s++)
                    {
                        if (cnt == ExecutionLength)
                            break;

                        ActiveCommandParams.Sector = track.Sectors[s].SectorID;

                        if (track.Sectors[s].SectorID == endSecID)
                            lastSec = true;

                        int size = 0x80 << track.Sectors[s].SectorSize;

                        for (int d = 0; d < size; d++)
                        {
                            track.Sectors[s].SectorData[d] = ExecutionBuffer[cnt++];
                        }

                        if (lastSec)
                            break;
                    }

                    _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                    _statusRegisters1.SetBits(StatusRegisters1.EN);

                    CommitResultCHRN();
                    CommitResultStatus();
                }

                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Scan Equal
    /// COMMAND:    8 parameter bytes
    /// EXECUTION:  Data compared between the FDD and FDC
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_ScanEqual()
    {
        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Scan Low or Equal
    /// COMMAND:    8 parameter bytes
    /// EXECUTION:  Data compared between the FDD and FDC
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_ScanLowOrEqual()
    {
        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Scan High or Equal
    /// COMMAND:    8 parameter bytes
    /// EXECUTION:  Data compared between the FDD and FDC
    /// RESULT:     7 result bytes
    /// </summary>
    private void UPD_ScanHighOrEqual()
    {
        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Specify
    /// COMMAND:    2 parameter bytes
    /// EXECUTION:  NO execution phase
    /// RESULT:     NO result phase
    /// 
    /// Looks like specify command returns status 0x80 throughout its lifecycle
    /// so CB is NOT set
    /// </summary>
    private void UPD_Specify()
    {
        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:

                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                byte currByte = CommandBuffer[CommandBufferCounter];
                BitArray bi = new BitArray(new byte[] { currByte });

                switch (CommandBufferCounter)
                {
                    // SRT & HUT
                    case 0:
                        SRT = 16 - (currByte >> 4) & 0x0f;
                        HUT = (currByte & 0x0f) << 4;
                        if (HUT == 0)
                        {
                            HUT = 255;
                        }
                        break;
                    // HLT & ND
                    case 1:
                        if (bi[0])
                            ND = true;
                        else
                            ND = false;

                        HLT = currByte & 0xfe;
                        if (HLT == 0)
                        {
                            HLT = 255;
                        }
                        break;
                }

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received
                    ActivePhase = Phase.Idle;
                }

                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Seek
    /// COMMAND:    2 parameter bytes
    /// EXECUTION:  Head is positioned over proper cylinder on disk
    /// RESULT:     NO result phase
    /// </summary>
    private void UPD_Seek()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                byte currByte = CommandBuffer[CommandBufferCounter];
                switch (CommandBufferCounter)
                {
                    case 0:
                        ParseParamByteStandard((CommandParameter)CommandBufferCounter);
                        break;
                    case 1:
                        ActiveFloppyDiskDrive.SeekingTrack = currByte;
                        break;
                }

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received
                    DriveLight = true;
                    ActivePhase = Phase.Execution;
                    ActiveCommand.CommandDelegate();
                }
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                // set seek flag
                ActiveFloppyDiskDrive.SeekStatus = (int)DriveSeekState.Seek;

                if (ActiveFloppyDiskDrive.CurrentTrackId == CommandBuffer[(int)CommandParameter.C])
                {
                    // we are already on the correct track
                    ActiveFloppyDiskDrive.SectorIndex = 0;
                }
                else
                {
                    // immediate seek
                    ActiveFloppyDiskDrive.CurrentTrackId = CommandBuffer[(int)CommandParameter.C];

                    ActiveFloppyDiskDrive.SectorIndex = 0;

                    if (ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveFloppyDiskDrive.CurrentTrackId].Sectors.Length > 1)
                    {
                        // always read the first sector
                        //ActiveFloppyDiskDrive.SectorIndex++;
                    }
                }

                // skip execution mode and go directly to idle
                // result is determined by SIS command
                ActivePhase = Phase.Idle;
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Recalibrate (seek track 0)
    /// COMMAND:    1 parameter byte
    /// EXECUTION:  Head retracted to track 0
    /// RESULT:     NO result phase
    /// </summary>
    private void UPD_Recalibrate()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received
                    DriveLight = true;
                    ActivePhase = Phase.Execution;
                    ActiveCommand.CommandDelegate();
                }
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:

                // immediate recalibration
                ActiveFloppyDiskDrive.TrackIndex = 0;
                ActiveFloppyDiskDrive.SectorIndex = 0;

                // recalibrate appears to always skip the first sector
                //if (ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveFloppyDiskDrive.TrackIndex].Sectors.Length > 1)
                //ActiveFloppyDiskDrive.SectorIndex++;

                // set seek flag
                ActiveFloppyDiskDrive.SeekStatus = (int)DriveSeekState.Recalibrate;

                // skip execution mode and go directly to idle
                // result is determined by SIS command
                ActivePhase = Phase.Idle;
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Sense Interrupt Status
    /// COMMAND:    NO parameter bytes
    /// EXECUTION:  NO execution phase
    /// RESULT:     2 result bytes
    /// </summary>
    private void UPD_SenseInterruptStatus()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                // SIS should return 2 bytes if sucessfully sensed an interrupt
                // 1 byte otherwise

                // it seems like the +3 ROM makes 3 SIS calls for each seek/recalibrate call for some reason
                // possibly one for each drive???
                // 1 - the interrupt is acknowleged with ST0 = 32 and track number
                // 2 - second sis returns 1 ST0 byte with 192
                // 3 - third SIS call returns standard 1 byte 0x80 (unknown cmd or SIS with no interrupt occured)
                // for now I will assume that the first call is aimed at DriveA, the second at DriveB (which we are NOT implementing)

                // check active drive first
                if (ActiveFloppyDiskDrive.SeekStatus == (int)DriveSeekState.Recalibrate ||
                    ActiveFloppyDiskDrive.SeekStatus == (int)DriveSeekState.Seek)
                {
                    // interrupt has been raised for this drive
                    // acknowledge
                    ActiveFloppyDiskDrive.SeekStatus = (int)DriveSeekState.Idle;

                    // result length 2
                    ResultLength = 2;

                    // first byte ST0 0x20
                    _statusRegisters0 = (StatusRegisters0)0x20;
                    //ResultBuffer[0] = (byte)_statusRegisters0;
                    SetResultBuffer(0, (byte)_statusRegisters0);
                    // second byte is the current track id
                    //ResultBuffer[1] = ActiveFloppyDiskDrive.CurrentTrackId;
                    SetResultBuffer(1, ActiveFloppyDiskDrive.CurrentTrackId);
                }
                /*
                    else if (ActiveFloppyDiskDrive.SeekStatus == SEEK_INTACKNOWLEDGED)
                    {
                        // DriveA interrupt has already been acknowledged
                        ActiveFloppyDiskDrive.SeekStatus = SEEK_IDLE;

                        ResLength = 1;
                        Status0 = 192;
                        ResBuffer[0] = Status0;
                    }
                    */
                else if (ActiveFloppyDiskDrive.SeekStatus == (int)DriveSeekState.Idle)
                {
                    // SIS with no interrupt
                    ResultLength = 1;
                    _statusRegisters0 = (StatusRegisters0)0x80;
                    //ResultBuffer[0] = (byte)_statusRegisters0;
                    SetResultBuffer(0, (byte)_statusRegisters0);
                }

                ActivePhase = Phase.Result;

                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Sense Drive Status
    /// COMMAND:    1 parameter byte
    /// EXECUTION:  NO execution phase
    /// RESULT:     1 result byte
    /// 
    /// The ZX spectrum appears to only specify drive 1 as the parameter byte, NOT drive 0
    /// After the final param byte is received main status changes to 0xd0
    /// Data register (ST3) result is 0x51 if drive/disk not available
    /// 0x71 if disk is present in 2nd drive
    /// </summary>
    private void UPD_SenseDriveStatus()
    {
        if (ActiveFloppyDiskDrive == null)
        {
            return;
        }

        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                // store the parameter in the command buffer
                //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                // process parameter byte
                ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                // increment command parameter counter
                CommandBufferCounter++;

                // was that the last parameter byte?
                if (CommandBufferCounter == ActiveCommand.ParameterByteCount)
                {
                    // all parameter bytes received
                    ActivePhase = Phase.Execution;
                    UPD_SenseDriveStatus();
                }
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                // one ST3 byte required

                // TODO: (Falta verificar que se esta añadiendo) set US
                _statusRegisters3 = (StatusRegisters3)ActiveFloppyDiskDrive.Id;

                if (_statusRegisters3 != 0)
                {
                    // we only support 1 drive
                    _statusRegisters3.SetBits(StatusRegisters3.FT);
                }
                else
                {
                    // HD - only one side
                    _statusRegisters3.UnSetBits(StatusRegisters3.HD);

                    // write protect
                    if (ActiveFloppyDiskDrive.IsWriteProtect)
                    {
                        _statusRegisters3.SetBits(StatusRegisters3.WP);
                    }

                    // track 0
                    if (ActiveFloppyDiskDrive.TrackIndex == 0)
                    {
                        _statusRegisters3.SetBits(StatusRegisters3.T0);
                    }

                    // rdy
                    if (ActiveFloppyDiskDrive.Disk != null)
                    {
                        _statusRegisters3.SetBits(StatusRegisters3.RY);
                    }
                }

                //ResultBuffer[0] = (byte)_statusRegisters3;
                SetResultBuffer(0, (byte)_statusRegisters3);
                ActivePhase = Phase.Result;

                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                break;
        }
    }

    /// <summary>
    /// Version
    /// COMMAND:    NO parameter bytes
    /// EXECUTION:  NO execution phase
    /// RESULT:     1 result byte
    /// </summary>
    private void UPD_Version()
    {
        switch (ActivePhase)
        {
            case Phase.Idle:
            case Phase.Command:
            case Phase.Execution:
            case Phase.Result:
                UPD_Invalid();
                break;
        }
    }

    /// <summary>
    /// Invalid
    /// COMMAND:    NO parameter bytes
    /// EXECUTION:  NO execution phase
    /// RESULT:     1 result byte
    /// </summary>
    private void UPD_Invalid()
    {
        switch (ActivePhase)
        {
            //----------------------------------------
            //  FDC is waiting for a command byte
            //----------------------------------------
            case Phase.Idle:
                break;

            //----------------------------------------
            //  Receiving command parameter bytes
            //----------------------------------------
            case Phase.Command:
                break;

            //----------------------------------------
            //  FDC in execution phase reading/writing bytes
            //----------------------------------------
            case Phase.Execution:
                // no execution phase
                ActivePhase = Phase.Result;
                UPD_Invalid();
                break;

            //----------------------------------------
            //  Result bytes being sent to CPU
            //----------------------------------------
            case Phase.Result:
                // ResultBuffer[0] = 0x80;
                SetResultBuffer(0, 0x80);
                break;
        }
    }

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
                if (ActiveCommand.Direction == CommandDirection.Out)
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
                ActiveCommand.CommandDelegate();

                res = LastSectorDataReadByte;

                if (ExecutionBufferCounter <= 0)
                {
                    // end of execution phase
                    ActivePhase = Phase.Result;
                }

                return res;

            case Phase.Result:

                DriveLight = false;

                ActiveCommand.CommandDelegate();

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
                ActiveCommand.CommandDelegate();
                break;
            //// we are in execution phase
            case Phase.Execution:
                // CPU is going to be sending data bytes to the FDC to be written to disk

                // store the byte
                LastSectorDataWriteByte = data;
                ActiveCommand.CommandDelegate();

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
        var cmd = CommandList.FirstOrDefault(a => a.CommandCode == cmdByte);

        if (cmd == null)
        {
            // no command found - use invalid
            CmdIndex = CommandList.Count - 1;
        }
        else
        {
            // valid command found
            CmdIndex = CommandList.FindIndex(a => a.CommandCode == cmdByte);

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
                CmdIndex = CommandList.Count - 1;
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
        ResultLength = ActiveCommand.ResultByteCount;

        // if there are no expected param bytes to receive - go ahead and run the command
        if (ActiveCommand.ParameterByteCount == 0)
        {
            ActivePhase = Phase.Execution;
            ActiveCommand.CommandDelegate();
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
                    ActiveCommandParams.Side = 1;
                else
                    ActiveCommandParams.Side = 0;

                ActiveCommandParams.UnitSelect = (byte)(GetUnitSelect(currByte));
                _flopyDiskDriveCluster.FloppyDiskDriveSlot = ActiveCommandParams.UnitSelect;
                break;

            // C
            case CommandParameter.C:
                ActiveCommandParams.Cylinder = currByte;
                break;

            // H
            case CommandParameter.H:
                ActiveCommandParams.Head = currByte;
                break;

            // R
            case CommandParameter.R:
                ActiveCommandParams.Sector = currByte;
                break;

            // N
            case CommandParameter.N:
                ActiveCommandParams.SectorSize = currByte;
                break;

            // EOT
            case CommandParameter.EOT:
                ActiveCommandParams.EOT = currByte;
                break;

            // GPL
            case CommandParameter.GPL:
                ActiveCommandParams.Gap3Length = currByte;
                break;

            // DTL
            case CommandParameter.DTL:
                ActiveCommandParams.DTL = currByte;
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
        SetResultBuffer((int)CommandResultParameter.C, ActiveCommandParams.Cylinder);
        //ResultBuffer[(int)CommandResultParameter.H] = ActiveCommandParams.Head;
        SetResultBuffer((int)CommandResultParameter.H, ActiveCommandParams.Head);
        //ResultBuffer[(int)CommandResultParameter.R] = ActiveCommandParams.Sector;
        SetResultBuffer((int)CommandResultParameter.R, ActiveCommandParams.Sector);
        //ResultBuffer[(int)CommandResultParameter.N] = ActiveCommandParams.SectorSize;
        SetResultBuffer((int)CommandResultParameter.N, ActiveCommandParams.SectorSize);
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
            if (trk.Sectors[index].SectorIDInfo.C == ActiveCommandParams.Cylinder &&
                trk.Sectors[index].SectorIDInfo.H == ActiveCommandParams.Head &&
                trk.Sectors[index].SectorIDInfo.R == ActiveCommandParams.Sector &&
                trk.Sectors[index].SectorIDInfo.N == ActiveCommandParams.SectorSize)
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
            else if (trk.Sectors[index].SectorIDInfo.C != ActiveCommandParams.Cylinder)
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
