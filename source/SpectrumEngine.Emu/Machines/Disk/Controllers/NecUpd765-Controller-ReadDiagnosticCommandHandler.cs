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

            switch (ActivePhase)
            {
                case ControllerCommandPhase.Idle:
                    break;

                //----------------------------------------
                //  Receiving command parameter bytes
                //----------------------------------------
                case ControllerCommandPhase.Command:

                    // store the parameter in the command buffer
                    CommandParameters[CommandParameterIndex] = LastByteReceived;

                    // process parameter byte
                    ParseParameterByte((CommandParameter)CommandParameterIndex);

                    // increment command parameter counter
                    CommandParameterIndex++;

                    // was that the last parameter byte?
                    if (CommandParameterIndex == ActiveCommand.ParameterBytesCount)
                    {
                        // all parameter bytes received - setup for execution phase

                        // clear exec buffer and status registers
                        ClearExecBuffer();
                        _statusRegisters0 = 0;
                        _statusRegisters1 = 0;
                        _statusRegisters2 = 0;
                        _statusRegisters3 = 0;

                        // temp sector index
                        byte secIdx = ActiveCommandData.Sector;

                        // do we have a valid disk inserted?
                        if (!ActiveFloppyDiskDrive.IsReady)
                        {
                            // no disk, no tracks or motor is not on
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                            CommitResultCHRN();
                            CommitResultStatus();
                            //ResBuffer[RS_ST0] = Status0;

                            // move to result phase
                            ActivePhase = ControllerCommandPhase.Result;
                            break;
                        }

                        int buffPos = 0;
                        int sectorSize = 0;
                        int maxTransferCap = 0;
                        if (maxTransferCap > 0) { }

                        // calculate requested size of data required
                        if (ActiveCommandData.SectorSize == 0)
                        {
                            // When N=0, then DTL defines the data length which the FDC must treat as a sector. If DTL is smaller than the actual 
                            // data length in a sector, the data beyond DTL in the sector is not sent to the Data Bus. The FDC reads (internally) 
                            // the complete sector performing the CRC check and, depending upon the manner of command termination, may perform 
                            // a Multi-Sector Read Operation.
                            sectorSize = ActiveCommandData.DTL;

                            // calculate maximum transfer capacity
                            if (!CMD_FLAG_MF)
                                maxTransferCap = 3328;
                        }
                        else
                        {
                            // When N is non - zero, then DTL has no meaning and should be set to ffh
                            ActiveCommandData.DTL = 0xFF;

                            // calculate maximum transfer capacity
                            switch (ActiveCommandData.SectorSize)
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

                            sectorSize = 0x80 << ActiveCommandData.SectorSize;
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
                            ActivePhase = ControllerCommandPhase.Result;
                            break;
                        }

                        //FloppyDisk.Sector sector = null;
                        ActiveFloppyDiskDrive.SectorIndex = 0;

                        int secCount = 0;

                        // read the whole track
                        for (int i = 0; i < track.Sectors.Length; i++)
                        {
                            if (secCount >= ActiveCommandData.EOT)
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
                            if (sec.TrackNumber != ActiveCommandData.Cylinder ||
                                sec.SideNumber != ActiveCommandData.Head ||
                                sec.SectorID != ActiveCommandData.Sector ||
                                sec.SectorSize != ActiveCommandData.SectorSize)
                            {
                                _statusRegisters1.SetBits(StatusRegisters1.ND);
                            }

                            secCount++;
                            ActiveFloppyDiskDrive.SectorIndex = i;
                        }

                        if (secCount == ActiveCommandData.EOT)
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
                                ActiveCommandData.Cylinder++;

                                // reset sector
                                ActiveCommandData.Sector = 1;
                                ActiveFloppyDiskDrive.SectorIndex = 0;
                            }
                            else
                            {
                                ActiveFloppyDiskDrive.SectorIndex++;
                            }

                            _statusRegisters0.UnSetBits(StatusRegisters0.IC_D6 | StatusRegisters0.IC_D7);

                            CommitResultCHRN();
                            CommitResultStatus();
                            ActivePhase = ControllerCommandPhase.Execution;
                        }

                        if (ActivePhase == ControllerCommandPhase.Execution)
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
                case ControllerCommandPhase.Execution:

                    var index = ExecutionLength - ExecutionBufferCounter;

                    LastSectorDataReadByte = ExecutionBuffer[index];

                    OverrunCounter--;
                    ExecutionBufferCounter--;

                    break;

                //----------------------------------------
                //  Result bytes being sent to CPU
                //----------------------------------------
                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
