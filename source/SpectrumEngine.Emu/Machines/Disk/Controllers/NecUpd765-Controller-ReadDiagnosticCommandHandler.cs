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
        /// Read Diagnostic (read track)
        /// COMMAND:    8 parameter bytes
        /// EXECUTION:  Data transfer between FDD and FDC. FDC reads all data fields from index hole to EDT
        /// RESULT:     7 result bytes
        /// </summary>
        private void ReadDiagnosticCommandHandler()
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
                            //ResBuffer[RS_ST0] = Status0;

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

                            //ResBuffer[RS_ST0] = Status0;

                            // move to result phase
                            _activePhase = ControllerCommandPhase.Result;
                            break;
                        }

                        //FloppyDisk.Sector sector = null;
                        ActiveFloppyDiskDrive.SectorIndex = 0;

                        int secCount = 0;

                        // read the whole track
                        for (int i = 0; i < track.Sectors.Length; i++)
                        {
                            if (secCount >= _activeCommandData.EOT)
                            {
                                break;
                            }

                            var sec = track.Sectors[i];
                            for (int b = 0; b < sec.ActualData.Length; b++)
                            {
                                _executionBuffer[buffPos++] = sec.ActualData[b];
                            }

                            // mark the sector read
                            sec.SectorReadCompleted();

                            // end of sector - compare IDs
                            if (sec.TrackNumber != _activeCommandData.Cylinder ||
                                sec.SideNumber != _activeCommandData.Head ||
                                sec.SectorID != _activeCommandData.Sector ||
                                sec.SectorSize != _activeCommandData.SectorSize)
                            {
                                _statusRegisters1.SetBits(StatusRegisters1.ND);
                            }

                            secCount++;
                            ActiveFloppyDiskDrive.SectorIndex = i;
                        }

                        if (secCount == _activeCommandData.EOT)
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
                                _activeCommandData.Cylinder++;

                                // reset sector
                                _activeCommandData.Sector = 1;
                                ActiveFloppyDiskDrive.SectorIndex = 0;
                            }
                            else
                            {
                                ActiveFloppyDiskDrive.SectorIndex++;
                            }

                            _statusRegisters0.UnSetBits(StatusRegisters0.IC_D6 | StatusRegisters0.IC_D7);

                            CommitResultCHRN();
                            CommitResultStatus();
                            _activePhase = ControllerCommandPhase.Execution;
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
