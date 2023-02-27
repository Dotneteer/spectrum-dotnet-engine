﻿using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
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
                case ControllerCommandPhase.Idle:
                case ControllerCommandPhase.Command:
                case ControllerCommandPhase.Execution:
                    // no execution phase
                    ActivePhase = ControllerCommandPhase.Result;
                    VersionCommandHandler();
                    break;
                case ControllerCommandPhase.Result:
                    // 90H indicates 7658, 80H indicates 765A/A-2 
                    ResultBuffer[0] = 0x80;
                    break;
            }
        }
    }
}
