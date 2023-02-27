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

            switch (_activePhase)
            {
                case ControllerCommandPhase.Idle:
                    break;

                case ControllerCommandPhase.Command:

                    PushCommandByteInBuffer();

                    // was that the last parameter byte?
                    if (_commandParameterIndex == _activeCommand.ParameterBytesCount)
                    {
                        // all parameter bytes received
                        DriveLight = true;
                        _activePhase = ControllerCommandPhase.Execution;
                        _activeCommand.CommandHandler();
                    }
                    break;

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
                    _activePhase = ControllerCommandPhase.Idle;
                    break;

                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
