using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers
{
    public partial class NecUpd765
    {
        /// <summary>
        /// Specify
        /// COMMAND:    2 parameter bytes
        /// EXECUTION:  NO execution phase
        /// RESULT:     NO result phase
        /// 
        /// Looks like specify command returns status 0x80 throughout its lifecycle
        /// so CB is NOT set
        /// </summary>
        private void SpecifyCommandHandler()
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

                    // store the parameter in the command buffer
                    CommandBuffer[CommandBufferCounter] = LastByteReceived;

                    // process parameter byte
                    byte currByte = CommandBuffer[CommandBufferCounter];
                    BitArray bi = new BitArray(new byte[] { currByte });

                    switch (CommandBufferCounter)
                    {
                        // SRT & HUT
                        case 0:
                            SRT = 16 - (currByte >> 4) & 0x0f;
                            HUT = (currByte & 0x0f) << 4;
                            if (HUT == 0)
                            {
                                HUT = 255;
                            }
                            break;
                        // HLT & ND
                        case 1:
                            if (bi[0])
                                ND = true;
                            else
                                ND = false;

                            HLT = currByte & 0xfe;
                            if (HLT == 0)
                            {
                                HLT = 255;
                            }
                            break;
                    }

                    // increment command parameter counter
                    CommandBufferCounter++;

                    // was that the last parameter byte?
                    if (CommandBufferCounter == ActiveCommand.ParameterBytesCount)
                    {
                        // all parameter bytes received
                        ActivePhase = Phase.Idle;
                    }

                    break;

                //----------------------------------------
                //  FDC in execution phase reading/writing bytes
                //----------------------------------------
                case Phase.Execution:
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
