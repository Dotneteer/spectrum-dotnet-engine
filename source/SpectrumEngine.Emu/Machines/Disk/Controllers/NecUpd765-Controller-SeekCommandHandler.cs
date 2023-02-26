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
        /// Seek
        /// COMMAND:    2 parameter bytes
        /// EXECUTION:  Head is positioned over proper cylinder on disk
        /// RESULT:     NO result phase
        /// </summary>
        private void SeekCommandHandler()
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
                    byte currByte = CommandBuffer[CommandBufferCounter];
                    switch (CommandBufferCounter)
                    {
                        case 0:
                            ParseParamByteStandard((CommandParameter)CommandBufferCounter);
                            break;
                        case 1:
                            ActiveFloppyDiskDrive.SeekingTrack = currByte;
                            break;
                    }

                    // increment command parameter counter
                    CommandBufferCounter++;

                    // was that the last parameter byte?
                    if (CommandBufferCounter == ActiveCommand.ParameterBytesCount)
                    {
                        // all parameter bytes received
                        DriveLight = true;
                        ActivePhase = Phase.Execution;
                        ActiveCommand.CommandHandler();
                    }
                    break;

                //----------------------------------------
                //  FDC in execution phase reading/writing bytes
                //----------------------------------------
                case Phase.Execution:
                    // set seek flag
                    ActiveFloppyDiskDrive.SeekStatus = (int)DriveSeekState.Seek;

                    if (ActiveFloppyDiskDrive.CurrentTrackId == CommandBuffer[(int)CommandParameter.C])
                    {
                        // we are already on the correct track
                        ActiveFloppyDiskDrive.SectorIndex = 0;
                    }
                    else
                    {
                        // immediate seek
                        ActiveFloppyDiskDrive.CurrentTrackId = CommandBuffer[(int)CommandParameter.C];

                        ActiveFloppyDiskDrive.SectorIndex = 0;

                        if (ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveFloppyDiskDrive.CurrentTrackId].Sectors.Length > 1)
                        {
                            // always read the first sector
                            //ActiveFloppyDiskDrive.SectorIndex++;
                        }
                    }

                    // skip execution mode and go directly to idle
                    // result is determined by SIS command
                    ActivePhase = Phase.Idle;
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
