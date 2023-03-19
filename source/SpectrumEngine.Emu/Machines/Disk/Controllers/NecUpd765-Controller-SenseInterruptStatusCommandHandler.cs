namespace SpectrumEngine.Emu.Machines.Disk.Controllers
{
    public partial class NecUpd765
    {

        /// <summary>
        /// Sense Interrupt Status
        /// COMMAND:    NO parameter bytes
        /// EXECUTION:  NO execution phase
        /// RESULT:     2 result bytes
        /// </summary>
        private void SenseInterruptStatusCommandHandler()
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
                    break;

                case ControllerCommandPhase.Execution:
                    // SIS should return 2 bytes if sucessfully sensed an interrupt
                    // 1 byte otherwise

                    // it seems like the +3 ROM makes 3 SIS calls for each seek/recalibrate call for some reason
                    // possibly one for each drive???
                    // 1 - the interrupt is acknowleged with ST0 = 32 and track number
                    // 2 - second sis returns 1 ST0 byte with 192
                    // 3 - third SIS call returns standard 1 byte 0x80 (unknown cmd or SIS with no interrupt occured)
                    // for now I will assume that the first call is aimed at DriveA, the second at DriveB (which we are NOT implementing)

                    // check active drive first
                    if (ActiveFloppyDiskDrive.SeekStatus == (int)DriveSeekState.Recalibrate ||
                        ActiveFloppyDiskDrive.SeekStatus == (int)DriveSeekState.Seek)
                    {
                        // interrupt has been raised for this drive
                        // acknowledge
                        ActiveFloppyDiskDrive.SeekStatus = (int)DriveSeekState.Idle;

                        // result length 2
                        _resultLength = 2;

                        // first byte ST0 0x20
                        _statusRegisters0 = (StatusRegisters0)0x20;
                        _resultBuffer[0] = (byte)_statusRegisters0;
                        // second byte is the current track id
                        _resultBuffer[1] = ActiveFloppyDiskDrive.CurrentTrackId;
                    }
                    else if (ActiveFloppyDiskDrive.SeekStatus == (int)DriveSeekState.Idle)
                    {
                        // SIS with no interrupt
                        _resultLength = 1;
                        _statusRegisters0 = (StatusRegisters0)0x80;
                        _resultBuffer[0] = (byte)_statusRegisters0;
                    }

                    _activePhase = ControllerCommandPhase.Result;

                    break;

                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
