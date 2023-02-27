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
                        // all parameter bytes received
                        DriveLight = true;
                        ActivePhase = ControllerCommandPhase.Execution;
                        ActiveCommand.CommandHandler();
                    }
                    break;

                //----------------------------------------
                //  FDC in execution phase reading/writing bytes
                //----------------------------------------
                case ControllerCommandPhase.Execution:

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
                    ActivePhase = ControllerCommandPhase.Idle;
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
