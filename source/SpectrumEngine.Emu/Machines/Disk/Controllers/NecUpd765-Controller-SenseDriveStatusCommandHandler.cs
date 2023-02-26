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
        /// Sense Drive Status
        /// COMMAND:    1 parameter byte
        /// EXECUTION:  NO execution phase
        /// RESULT:     1 result byte
        /// 
        /// The ZX spectrum appears to only specify drive 1 as the parameter byte, NOT drive 0
        /// After the final param byte is received main status changes to 0xd0
        /// Data register (ST3) result is 0x51 if drive/disk not available
        /// 0x71 if disk is present in 2nd drive
        /// </summary>
        private void SenseDriveStatusCommandHandler()
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
                        // all parameter bytes received
                        ActivePhase = Phase.Execution;
                        SenseDriveStatusCommandHandler();
                    }
                    break;

                //----------------------------------------
                //  FDC in execution phase reading/writing bytes
                //----------------------------------------
                case Phase.Execution:
                    // one ST3 byte required

                    // TODO: (Falta verificar que se esta añadiendo) set US
                    _statusRegisters3 = (StatusRegisters3)ActiveFloppyDiskDrive.Id;

                    if (_statusRegisters3 != 0)
                    {
                        // we only support 1 drive
                        _statusRegisters3.SetBits(StatusRegisters3.FT);
                    }
                    else
                    {
                        // HD - only one side
                        _statusRegisters3.UnSetBits(StatusRegisters3.HD);

                        // write protect
                        if (ActiveFloppyDiskDrive.IsWriteProtect)
                        {
                            _statusRegisters3.SetBits(StatusRegisters3.WP);
                        }

                        // track 0
                        if (ActiveFloppyDiskDrive.TrackIndex == 0)
                        {
                            _statusRegisters3.SetBits(StatusRegisters3.T0);
                        }

                        // rdy
                        if (ActiveFloppyDiskDrive.Disk != null)
                        {
                            _statusRegisters3.SetBits(StatusRegisters3.RY);
                        }
                    }

                    //ResultBuffer[0] = (byte)_statusRegisters3;
                    SetResultBuffer(0, (byte)_statusRegisters3);
                    ActivePhase = Phase.Result;

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
