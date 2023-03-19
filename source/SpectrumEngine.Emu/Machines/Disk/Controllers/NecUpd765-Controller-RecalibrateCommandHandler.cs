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
                    if (_commandParameterIndex == _activeCommandConfiguration.ParameterBytesCount)
                    {
                        // all parameter bytes received
                        DriveLight = true;
                        _activePhase = ControllerCommandPhase.Execution;
                        _activeCommandConfiguration.CommandHandler();
                    }
                    break;

                case ControllerCommandPhase.Execution:

                    // immediate recalibration
                    ActiveFloppyDiskDrive.TrackIndex = 0;
                    ActiveFloppyDiskDrive.SectorIndex = 0;

                    // set seek flag
                    ActiveFloppyDiskDrive.SeekStatus = (int)DriveSeekState.Recalibrate;

                    // skip execution mode and go directly to idle result is determined by SIS command
                    _activePhase = ControllerCommandPhase.Idle;
                    break;

                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
