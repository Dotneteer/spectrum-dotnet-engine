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
        /// Write ID (format write)
        /// COMMAND:    5 parameter bytes
        /// EXECUTION:  Entire track is formatted
        /// RESULT:     7 result bytes
        /// </summary>
        private void WriteIdCommandHandler()
        {
            if (ActiveFloppyDiskDrive == null)
            {
                return;
            }

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
                    //CommandBuffer[CommandBufferCounter] = LastByteReceived;
                    SetCommandBuffer(CommandBufferCounter, LastByteReceived);

                    // process parameter byte
                    ParseParamByteStandard((CommandParameter)CommandBufferCounter);

                    // increment command parameter counter
                    CommandBufferCounter++;

                    // was that the last parameter byte?
                    if (CommandBufferCounter == ActiveCommand.ParameterBytesCount)
                    {
                        // all parameter bytes received - setup for execution phase
                        DriveLight = true;

                        // clear exec buffer and status registers
                        ClearExecBuffer();
                        _statusRegisters0 = 0;
                        _statusRegisters1 = 0;
                        _statusRegisters2 = 0;
                        _statusRegisters3 = 0;

                        // temp sector index
                        byte secIdx = ActiveCommandState.Sector;

                        // do we have a valid disk inserted?
                        if (!ActiveFloppyDiskDrive.IsReady)
                        {
                            // no disk, no tracks or motor is not on
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                            CommitResultCHRN();
                            CommitResultStatus();
                            //ResBuffer[RS_ST0] = Status0;

                            // move to result phase
                            ActivePhase = Phase.Result;
                            break;
                        }

                        // check write protect tab
                        if (ActiveFloppyDiskDrive.IsWriteProtect)
                        {
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                            _statusRegisters1.SetBits(StatusRegisters1.NW);

                            CommitResultCHRN();
                            CommitResultStatus();
                            //ResBuffer[RS_ST0] = Status0;

                            // move to result phase
                            ActivePhase = Phase.Result;
                            break;
                        }
                        else
                        {
                            // not implemented yet
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                            _statusRegisters1.SetBits(StatusRegisters1.NW);

                            CommitResultCHRN();
                            CommitResultStatus();
                            //ResBuffer[RS_ST0] = Status0;

                            // move to result phase
                            ActivePhase = Phase.Result;
                            break;
                        }
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
