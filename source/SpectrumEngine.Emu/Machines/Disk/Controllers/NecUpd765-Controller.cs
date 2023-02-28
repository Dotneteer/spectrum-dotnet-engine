using SpectrumEngine.Emu.Extensions;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
using System;
using System.Collections;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers;

/// <summary>
/// FDC State and Methods
/// </summary>
public partial class NecUpd765
{
    /// <summary>
    /// Active command state
    /// </summary>
    private CommandData _activeCommandData = new();

    /// <summary>
    /// Current active phase controller
    /// </summary>
    private ControllerCommandPhase _activePhase = ControllerCommandPhase.Command;

    /// <summary>
    /// Command parameters
    /// </summary>
    private readonly byte[] _commandParameters = new byte[MaxParameterBytes];

    /// <summary>
    /// Current index in command parameters
    /// </summary>
    private int _commandParameterIndex = 0;

    /// <summary>
    /// Command flags
    /// </summary>
    private CommandFlags _commandFlags;

    /// <summary>
    /// In lieu of actual timing, this will count status reads in execution phase
    /// where the CPU hasnt actually read any bytes
    /// </summary>
    private int _overrunCounter;

    /// <summary>
    /// Contains result bytes in result phase
    /// </summary>
    private readonly byte[] _resultBuffer = new byte[7];

    /// <summary>
    /// Current index in result buffer
    /// </summary>

    private int _resultBufferCounter = 0;

    /// <summary>
    /// Contains sector data to be written/read in execution phase
    /// </summary>
    private readonly byte[] _executionBuffer = new byte[0x8000];

    /// <summary>
    /// Index for sector data within the result buffer
    /// </summary>
    private int _executionBufferCounter = 0;

    /// <summary>
    /// The byte length of the currently active command
    /// This may or may not be the same as the actual command resultbytes value
    /// </summary>
    private int _resultLength = 0;

    /// <summary>
    /// The length of the current exec command
    /// </summary>
    private int _executionLength = 0;

    /// <summary>
    /// The last write byte that was received during execution phase
    /// </summary>
    private byte _lastSectorDataWriteByte = 0;

    /// <summary>
    /// The last read byte to be sent during execution phase
    /// </summary>
    private byte _lastSectorDataReadByte = 0;

    /// <summary>
    /// The last parameter byte that was written to the FDC
    /// </summary>
    private byte _lastByteReceived = 0;

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
    /// Current selected command in <see cref="_commands"/>
    /// </summary>
    private int _cmdIndex;
    public int CmdIndex
    {
        get => _cmdIndex;
        set
        {
            _cmdIndex = value;
            _activeCommandConfiguration = _commands[_cmdIndex];
        }
    }

    /// <summary>
    /// signs whether flag motor is on/off
    /// </summary>
    public bool FlagMotor { get; private set; }

    /// <summary>
    /// Signs whether the drive is active
    /// </summary>
    public bool DriveLight { get; private set; }

    /// <summary>
    /// Called when a status register read is required
    /// This can be called at any time
    /// The main status register appears to be queried nearly all the time
    /// so needs to be kept updated. It keeps the CPU informed of the current state
    /// </summary>
    private MainStatusRegisters ReadMainStatus()
    {
        _mainStatusRegisters.SetBits(MainStatusRegisters.RQM);

        switch (_activePhase)
        {
            case ControllerCommandPhase.Idle:
                _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO | MainStatusRegisters.CB | MainStatusRegisters.EXM);
                break;

            case ControllerCommandPhase.Command:
                _mainStatusRegisters.SetBits(MainStatusRegisters.CB);
                _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO | MainStatusRegisters.EXM);
                break;

            case ControllerCommandPhase.Execution:
                if (_activeCommandConfiguration.CommandFlow == CommandFlow.Out)
                {
                    _mainStatusRegisters.SetBits(MainStatusRegisters.DIO);
                }
                else
                {
                    _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO);
                }

                _mainStatusRegisters.SetBits(MainStatusRegisters.DIO | MainStatusRegisters.EXM);

                // overrun detection                    
                _overrunCounter++;
                if (_overrunCounter >= 64)
                {
                    // CPU has read the status register 64 times without reading the data register, switch the current command into result phase
                    _activePhase = ControllerCommandPhase.Result;
                    _overrunCounter = 0;
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

        // FDC is not ready to return data
        if (!_mainStatusRegisters.HasFlag(MainStatusRegisters.RQM))
        {
            return res;
        }

        // check active direction
        if (!_mainStatusRegisters.HasFlag(MainStatusRegisters.DIO))
        {
            // FDC is expecting to receive, not send data
            return res;
        }

        switch (_activePhase)
        {
            case ControllerCommandPhase.Execution:
                // reset overrun counter
                _overrunCounter = 0;

                // execute read
                _activeCommandConfiguration.CommandHandler();

                res = _lastSectorDataReadByte;

                if (_executionBufferCounter <= 0)
                {
                    // end of execution phase
                    _activePhase = ControllerCommandPhase.Result;
                }

                return res;

            case ControllerCommandPhase.Result:

                DriveLight = false;

                _activeCommandConfiguration.CommandHandler();

                // result byte reading
                res = _resultBuffer[_resultBufferCounter++];
                if (_resultBufferCounter >= _resultLength)
                {
                    _activePhase = ControllerCommandPhase.Idle;
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
        _lastByteReceived = data;

        // process incoming bytes
        switch (_activePhase)
        {
            // controller is idle awaiting the first command byte of a new instruction
            case ControllerCommandPhase.Idle:
                ParseCommandByte(data);
                break;

            // we are in command phase
            case ControllerCommandPhase.Command:
                // attempt to process this parameter byte
                _activeCommandConfiguration.CommandHandler();
                break;

            // we are in execution phase, CPU is going to be sending data bytes to the FDC to be written to disk
            case ControllerCommandPhase.Execution:
                // store the byte
                _lastSectorDataWriteByte = data;
                _activeCommandConfiguration.CommandHandler();

                if (_executionBufferCounter <= 0)
                {
                    // end of execution phase
                    _activePhase = ControllerCommandPhase.Result;
                }
                break;

            // result phase
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
        _commandParameterIndex = 0;
        _resultBufferCounter = 0;

        // get MT, MD and SK flag states
        _commandFlags = new CommandFlags(cmdByte);

        // get the first 4 bytes
        cmdByte = (byte)(cmdByte & 0x0f);

        // lookup the command
        var cmd = _commands.FirstOrDefault(item => item.CommandCode == (CommandCode)cmdByte);

        if (cmd == null)
        {
            // no command found - use invalid
            CmdIndex = _commands.Count - 1;
        }
        else
        {
            // valid command found
            CmdIndex = _commands.FindIndex(item => item.CommandCode == (CommandCode)cmdByte);

            // check validity of command byte flags
            // if a flag is set but not valid for this command then it is invalid
            if ((!_activeCommandConfiguration.CommandFlags.MT && _commandFlags.MT) ||
                (!_activeCommandConfiguration.CommandFlags.MF && _commandFlags.MF) ||
                (!_activeCommandConfiguration.CommandFlags.SK && _commandFlags.SK))
            {
                // command byte included bit 5,6 or 7 flags
                CmdIndex = _commands.Count - 1;
            }
        }

        _commandParameterIndex = 0;
        _resultBufferCounter = 0;

        // there will now be an active command set move to command phase
        _activePhase = ControllerCommandPhase.Command;

        // set reslength
        _resultLength = _activeCommandConfiguration.ResultBytesCount;

        // if there are no expected param bytes to receive - go ahead and run the command
        if (_activeCommandConfiguration.ParameterBytesCount == 0)
        {
            _activePhase = ControllerCommandPhase.Execution;
            _activeCommandConfiguration.CommandHandler();
        }

        return true;
    }

    /// <summary>
    /// Parse command parameter byte
    /// </summary>
    private void ParseParameterByte(CommandParameter index)
    {
        byte currByte = _commandParameters[(int)index];

        switch (index)
        {
            // HD & US
            case CommandParameter.HEAD:
                _activeCommandData.Side = (byte)(currByte & 0x02);
                _activeCommandData.UnitSelect = (byte)(currByte & 0x03);
                _flopyDiskDriveCluster.FloppyDiskDriveSlot = _activeCommandData.UnitSelect;
                break;

            // C
            case CommandParameter.C:
                _activeCommandData.Cylinder = currByte;
                break;

            // H
            case CommandParameter.H:
                _activeCommandData.Head = currByte;
                break;

            // R
            case CommandParameter.R:
                _activeCommandData.Sector = currByte;
                break;

            // N
            case CommandParameter.N:
                _activeCommandData.SectorSize = currByte;
                break;

            // EOT
            case CommandParameter.EOT:
                _activeCommandData.EOT = currByte;
                break;

            // GPL
            case CommandParameter.GPL:
                _activeCommandData.Gap3Length = currByte;
                break;

            // DTL
            case CommandParameter.DTL:
                _activeCommandData.DTL = currByte;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// push last byte received in buffer command
    /// </summary>
    private void PushCommandByteInBuffer()
    {
        // store the parameter
        _commandParameters[_commandParameterIndex] = _lastByteReceived;

        // process parameter byte
        ParseParameterByte((CommandParameter)_commandParameterIndex);

        // increment command parameter counter
        _commandParameterIndex++;
    }

    /// <summary>
    /// Clears the result buffer
    /// </summary>
    private void ClearResultBuffer()
    {
        for (int i = 0; i < _resultBuffer.Length; i++)
        {
            _resultBuffer[i] = 0;
        }
    }

    /// <summary>
    /// Clears the result buffer
    /// </summary>
    private void ClearExecBuffer()
    {
        for (int i = 0; i < _executionBuffer.Length; i++)
        {
            _executionBuffer[i] = 0;
        }
    }

    private void ClearStatusRegisters()
    {
        _statusRegisters0 = 0;
        _statusRegisters1 = 0;
        _statusRegisters2 = 0;
        _statusRegisters3 = 0;
    }

    /// <summary>
    /// Populates the result status registers
    /// </summary>
    private void CommitResultStatus()
    {
        // check for read diagnostic
        if (_activeCommandConfiguration.CommandCode == CommandCode.ReadDiagnostic)
        {
            // commit to result buffer
            _resultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
            _resultBuffer[(int)CommandResultParameter.ST1] = (byte)_statusRegisters1;

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
        _resultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
        _resultBuffer[(int)CommandResultParameter.ST1] = (byte)_statusRegisters1;
        _resultBuffer[(int)CommandResultParameter.ST2] = (byte)_statusRegisters2;
    }

    /// <summary>
    /// Populates the result CHRN values
    /// </summary>
    private void CommitResultCHRN()
    {
        _resultBuffer[(int)CommandResultParameter.C] = _activeCommandData.Cylinder;
        _resultBuffer[(int)CommandResultParameter.H] = _activeCommandData.Head;
        _resultBuffer[(int)CommandResultParameter.R] = _activeCommandData.Sector;
        _resultBuffer[(int)CommandResultParameter.N] = _activeCommandData.SectorSize;
    }

    /// <summary>
    /// Moves active phase into idle
    /// </summary>
    public void SetPhaseIdle()
    {
        _activePhase = ControllerCommandPhase.Idle;

        // active direction
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.DIO);
        // CB
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.CB);
        // RQM
        _mainStatusRegisters.UnSetBits(MainStatusRegisters.RQM);

        _commandParameterIndex = 0;
        _resultBufferCounter = 0;
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
        int indexHole = 0;

        // loop through the sectors in a track, the loop ends with either the sector being found or the index hole being passed twice
        while (indexHole <= 2)
        {
            // does the requested sector match the current sector
            if (trk.Sectors[index].SectorIDInfo.C == _activeCommandData.Cylinder &&
                trk.Sectors[index].SectorIDInfo.H == _activeCommandData.Head &&
                trk.Sectors[index].SectorIDInfo.R == _activeCommandData.Sector &&
                trk.Sectors[index].SectorIDInfo.N == _activeCommandData.SectorSize)
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
            else if (trk.Sectors[index].SectorIDInfo.C != _activeCommandData.Cylinder)
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
                indexHole++;
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
