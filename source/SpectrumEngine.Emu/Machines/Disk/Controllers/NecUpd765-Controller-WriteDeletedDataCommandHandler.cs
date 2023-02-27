using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers
{
    public partial class NecUpd765
    {
        /// <summary>
        /// Write Deleted Data
        /// COMMAND:    8 parameter bytes
        /// EXECUTION:  Data transfer between FDC and FDD
        /// RESULT:     7 result bytes
        /// </summary>
        private void WriteDeletedDataCommandHandler()
        {
            if (ActiveFloppyDiskDrive == null)
            {
                return;
            }

            switch (_activePhase)
            {
                case ControllerCommandPhase.Idle:
                    break;

                case ControllerCommandPhase.Command:

                    PushCommandByteInBuffer();

                    // was that the last parameter byte?
                    if (_commandParameterIndex == _activeCommand.ParameterBytesCount)
                    {
                        // all parameter bytes received - setup for execution phase

                        // clear exec buffer and status registers
                        ClearExecBuffer();
                        _statusRegisters0 = 0;
                        _statusRegisters1 = 0;
                        _statusRegisters2 = 0;
                        _statusRegisters3 = 0;

                        // temp sector index
                        byte secIdx = _activeCommandData.Sector;

                        // do we have a valid disk inserted?
                        if (!ActiveFloppyDiskDrive.IsReady)
                        {
                            // no disk, no tracks or motor is not on
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                            CommitResultCHRN();
                            CommitResultStatus();

                            // move to result phase
                            _activePhase = ControllerCommandPhase.Result;
                            break;
                        }

                        // check write protect tab
                        if (ActiveFloppyDiskDrive.IsWriteProtect)
                        {
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                            _statusRegisters1.SetBits(StatusRegisters1.NW);

                            CommitResultCHRN();
                            CommitResultStatus();

                            // move to result phase
                            _activePhase = ControllerCommandPhase.Result;
                            break;
                        }
                        else
                        {

                            // calculate the number of bytes to write
                            int byteCounter = 0;
                            byte endSecID = _activeCommandData.EOT;
                            bool lastSec = false;

                            // get the first sector
                            var track = ActiveFloppyDiskDrive.Disk.DiskTracks[_activeCommandData.Cylinder];
                            //int secIndex = 0;
                            for (int s = 0; s < track.Sectors.Length; s++)
                            {
                                if (track.Sectors[s].SectorID == endSecID)
                                    lastSec = true;

                                for (int i = 0; i < 0x80 << _activeCommandData.SectorSize; i++)
                                {
                                    byteCounter++;

                                    if (i == (0x80 << _activeCommandData.SectorSize) - 1 && lastSec)
                                    {
                                        break;
                                    }
                                }

                                if (lastSec)
                                    break;
                            }

                            _executionBufferCounter = byteCounter;
                            _executionLength = byteCounter;
                            _activePhase = ControllerCommandPhase.Execution;
                            DriveLight = true;
                            break;
                        }
                    }

                    break;

                case ControllerCommandPhase.Execution:

                    var index = _executionLength - _executionBufferCounter;

                    _executionBuffer[index] = _lastSectorDataWriteByte;

                    _overrunCounter--;
                    _executionBufferCounter--;

                    if (_executionBufferCounter <= 0)
                    {
                        int cnt = 0;

                        // all data received
                        byte endSecID = _activeCommandData.EOT;
                        bool lastSec = false;
                        var track = ActiveFloppyDiskDrive.Disk.DiskTracks[_activeCommandData.Cylinder];

                        for (int s = 0; s < track.Sectors.Length; s++)
                        {
                            if (cnt == _executionLength)
                                break;

                            _activeCommandData.Sector = track.Sectors[s].SectorID;

                            if (track.Sectors[s].SectorID == endSecID)
                                lastSec = true;

                            int size = 0x80 << track.Sectors[s].SectorSize;

                            for (int d = 0; d < size; d++)
                            {
                                track.Sectors[s].SectorData[d] = _executionBuffer[cnt++];
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

                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
