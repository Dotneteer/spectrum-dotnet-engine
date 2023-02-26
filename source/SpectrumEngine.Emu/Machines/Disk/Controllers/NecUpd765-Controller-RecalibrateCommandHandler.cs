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
        /// Recalibrate (seek track 0)
        /// COMMAND:    1 parameter byte
        /// EXECUTION:  Head retracted to track 0
        /// RESULT:     NO result phase
        /// </summary>
        private void RecalibrateCommandHandler()
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
                    CommandBuffer[CommandBufferCounter] = LastByteReceived;

                    // process parameter byte
                    ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                    // increment command parameter counter
                    CommandBufferCounter++;

                    // was that the last parameter byte?
                    if (CommandBufferCounter == ActiveCommand.ParameterBytesCount)
                    {
                        // all parameter bytes received
                        DriveLight = true;
                        ActivePhase = Phase.Execution;
                        ActiveCommand.CommandHandler();
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

    }
}
