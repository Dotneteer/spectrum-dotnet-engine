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
        /// Invalid
        /// COMMAND:    NO parameter bytes
        /// EXECUTION:  NO execution phase
        /// RESULT:     1 result byte
        /// </summary>
        private void InvalidCommandHandler()
        {
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
                    break;

                //----------------------------------------
                //  FDC in execution phase reading/writing bytes
                //----------------------------------------
                case Phase.Execution:
                    // no execution phase
                    ActivePhase = Phase.Result;
                    InvalidCommandHandler();
                    break;

                //----------------------------------------
                //  Result bytes being sent to CPU
                //----------------------------------------
                case Phase.Result:
                    // ResultBuffer[0] = 0x80;
                    SetResultBuffer(0, 0x80);
                    break;
            }
        }

    }
}
