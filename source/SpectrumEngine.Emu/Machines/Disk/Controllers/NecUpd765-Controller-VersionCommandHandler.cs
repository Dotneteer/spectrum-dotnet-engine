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
        /// Version
        /// COMMAND:    NO parameter bytes
        /// EXECUTION:  NO execution phase
        /// RESULT:     1 result byte
        /// </summary>
        private void VersionCommandHandler()
        {
            switch (ActivePhase)
            {
                case Phase.Idle:
                case Phase.Command:
                case Phase.Execution:
                case Phase.Result:
                    InvalidCommandHandler();
                    break;
            }
        }
    }
}
