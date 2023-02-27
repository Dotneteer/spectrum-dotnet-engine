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
        /// Write Data
        /// COMMAND:    8 parameter bytes
        /// EXECUTION:  Data transfer between FDC and FDD
        /// RESULT:     7 result bytes
        /// </summary>
        private void WriteDataCommandHandler()
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

                        // check write protect tab
                        if (ActiveFloppyDiskDrive.IsWriteProtect)
                        {
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                            _statusRegisters1.SetBits(StatusRegisters1.NW);

                            CommitResultCHRN();
                            CommitResultStatus();
                            //ResBuffer[RS_ST0] = Status0;

                            // move to result phase
                            ActivePhase = ControllerCommandPhase.Result;
                            break;
                        }
                        else
                        {

                            // calculate the number of bytes to write
                            int byteCounter = 0;
                            byte startSecID = ActiveCommandData.Sector;
                            byte endSecID = ActiveCommandData.EOT;
                            bool lastSec = false;

                            // get the first sector
                            var track = ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveCommandData.Cylinder];
                            //int secIndex = 0;
                            for (int s = 0; s < track.Sectors.Length; s++)
                            {
                                if (track.Sectors[s].SectorID == endSecID)
                                    lastSec = true;

                                for (int i = 0; i < 0x80 << ActiveCommandData.SectorSize; i++)
                                {
                                    byteCounter++;

                                    if (i == (0x80 << ActiveCommandData.SectorSize) - 1 && lastSec)
                                    {
                                        break;
                                    }
                                }

                                if (lastSec)
                                    break;
                            }

                            ExecutionBufferCounter = byteCounter;
                            ExecutionLength = byteCounter;
                            ActivePhase = ControllerCommandPhase.Execution;
                            DriveLight = true;
                            break;
                        }
                    }

                    break;

                //----------------------------------------
                //  FDC in execution phase reading/writing bytes
                //----------------------------------------
                case ControllerCommandPhase.Execution:

                    var index = ExecutionLength - ExecutionBufferCounter;

                    ExecutionBuffer[index] = LastSectorDataWriteByte;

                    OverrunCounter--;
                    ExecutionBufferCounter--;

                    if (ExecutionBufferCounter <= 0)
                    {
                        int cnt = 0;

                        // all data received
                        byte startSecID = ActiveCommandData.Sector;
                        byte endSecID = ActiveCommandData.EOT;
                        bool lastSec = false;
                        var track = ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveCommandData.Cylinder];
                        //int secIndex = 0;

                        for (int s = 0; s < track.Sectors.Length; s++)
                        {
                            if (cnt == ExecutionLength)
                                break;

                            ActiveCommandData.Sector = track.Sectors[s].SectorID;

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
                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
