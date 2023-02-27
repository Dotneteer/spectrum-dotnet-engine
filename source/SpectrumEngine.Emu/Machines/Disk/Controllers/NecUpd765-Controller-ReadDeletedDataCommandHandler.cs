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
        /// Read Deleted Data
        /// COMMAND:    8 parameter bytes
        /// EXECUTION:  Data transfer between the FDD and FDC
        /// RESULT:     7 result bytes
        /// </summary>
        private void ReadDeletedDataCommandHandler()
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

                        int buffPos = 0;
                        int sectorSize = 0;

                        // calculate requested size of data required
                        if (_activeCommandData.SectorSize == 0)
                        {
                            // When N=0, then DTL defines the data length which the FDC must treat as a sector. If DTL is smaller than the actual 
                            // data length in a sector, the data beyond DTL in the sector is not sent to the Data Bus. The FDC reads (internally) 
                            // the complete sector performing the CRC check and, depending upon the manner of command termination, may perform 
                            // a Multi-Sector Read Operation.
                            sectorSize = _activeCommandData.DTL;
                        }
                        else
                        {
                            // When N is non - zero, then DTL has no meaning and should be set to ffh
                            _activeCommandData.DTL = 0xFF;
                            sectorSize = 0x80 << _activeCommandData.SectorSize;
                        }

                        // get the current track
                        var track = ActiveFloppyDiskDrive.Disk.DiskTracks.FirstOrDefault(a => a.TrackNumber == ActiveFloppyDiskDrive.CurrentTrackId);

                        if (track == null || track.NumberOfSectors <= 0)
                        {
                            // track could not be found
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                            CommitResultCHRN();
                            CommitResultStatus();

                            // move to result phase
                            _activePhase = ControllerCommandPhase.Result;
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
                                _activeCommandData.Cylinder = track.TrackNumber;

                                CommitResultCHRN();
                                CommitResultStatus();
                                _activePhase = ControllerCommandPhase.Result;
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
                            if (_commandFlags.SK && _statusRegisters2.HasFlag(StatusRegisters2.CM))
                            {
                                if (_activeCommandData.Sector != _activeCommandData.EOT)
                                {
                                    // increment the sector ID and search again
                                    _activeCommandData.Sector++;
                                    continue;
                                }
                                else
                                {
                                    // no execution phase
                                    _statusRegisters0.SetAbnormalTerminationCommand();

                                    // result requires the actual track id, rather than the sector track id
                                    _activeCommandData.Cylinder = track.TrackNumber;

                                    CommitResultCHRN();
                                    CommitResultStatus();
                                    _activePhase = ControllerCommandPhase.Result;
                                    break;
                                }
                            }







                            // if DAM is not set this will be the last sector to read
                            if (_statusRegisters2.HasFlag(StatusRegisters2.CM))
                            {
                                _activeCommandData.EOT = _activeCommandData.Sector;
                            }

                            // read the sector
                            for (int i = 0; i < sectorSize; i++)
                            {
                                _executionBuffer[buffPos++] = sector.ActualData[i];
                            }

                            // mark the sector read
                            sector.SectorReadCompleted();

                            if (sector.SectorID == _activeCommandData.EOT)
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
                                    _activeCommandData.Cylinder++;

                                    // reset sector
                                    _activeCommandData.Sector = 1;
                                    ActiveFloppyDiskDrive.SectorIndex = 0;
                                }
                                else
                                {
                                    ActiveFloppyDiskDrive.SectorIndex++;
                                }

                                _statusRegisters0.SetAbnormalTerminationCommand();

                                // result requires the actual track id, rather than the sector track id
                                _activeCommandData.Cylinder = track.TrackNumber;

                                // remove CM (appears to be required to defeat Alkatraz copy protection)
                                _statusRegisters2.UnSetBits(StatusRegisters2.CM);

                                CommitResultCHRN();
                                CommitResultStatus();
                                _activePhase = ControllerCommandPhase.Execution;
                                break;
                            }
                            else
                            {
                                // continue with multi-sector read operation
                                _activeCommandData.Sector++;
                            }
                        }

                        if (_activePhase == ControllerCommandPhase.Execution)
                        {
                            _executionLength = buffPos;
                            _executionBufferCounter = buffPos;
                            DriveLight = true;
                        }
                    }
                    break;

                case ControllerCommandPhase.Execution:
                    var index = _executionLength - _executionBufferCounter;

                    _lastSectorDataReadByte = _executionBuffer[index];

                    _overrunCounter--;
                    _executionBufferCounter--;

                    break;

                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
