﻿namespace SpectrumEngine.Emu.Machines.Disk.Controllers
{
    public partial class NecUpd765
    {
        /// <summary>
        /// Scan Equal
        /// COMMAND:    8 parameter bytes
        /// EXECUTION:  Data compared between the FDD and FDC
        /// RESULT:     7 result bytes
        /// </summary>
        private void ScanEqualCommandHandler()
        {
            switch (_activePhase)
            {
                case ControllerCommandPhase.Idle:
                    break;
                case ControllerCommandPhase.Command:
                    break;
                case ControllerCommandPhase.Execution:
                    break;
                case ControllerCommandPhase.Result:
                    break;
            }

            throw new NotImplementedException();
        }
    }
}