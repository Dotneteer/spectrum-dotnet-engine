namespace SpectrumEngine.Emu.Machines.Disk.Controllers
{
    public partial class NecUpd765
    {
        /// <summary>
        /// Invalid
        /// COMMAND:    NO parameter bytes
        /// EXECUTION:  NO execution phase
        /// RESULT:     1 result byte
        /// </summary>
        private void InvalidCommandHandler()
        {
            switch (ActivePhase)
            {
                case ControllerCommandPhase.Idle:
                    break;

                case ControllerCommandPhase.Command:
                    break;

                case ControllerCommandPhase.Execution:
                    // no execution phase
                    ActivePhase = ControllerCommandPhase.Result;
                    InvalidCommandHandler();
                    break;

                case ControllerCommandPhase.Result:
                    // ST0 = 80H
                    ResultBuffer[0] = 0x80;
                    break;
            }
        }

    }
}
