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

            switch (_activePhase)
            {
                case ControllerCommandPhase.Idle:
                    break;

                case ControllerCommandPhase.Command:
                    // store the parameter in the command buffer
                    _commandParameters[_commandParameterIndex] = _lastByteReceived;

                    // process parameter byte
                    byte currByte = _commandParameters[_commandParameterIndex];
                    switch (_commandParameterIndex)
                    {
                        case 0:
                            ParseParameterByte((CommandParameter)_commandParameterIndex);
                            break;
                        case 1:
                            ActiveFloppyDiskDrive.SeekingTrack = currByte;
                            break;
                    }

                    // increment command parameter counter
                    _commandParameterIndex++;

                    // was that the last parameter byte?
                    if (_commandParameterIndex == _activeCommand.ParameterBytesCount)
                    {
                        // all parameter bytes received
                        DriveLight = true;
                        _activePhase = ControllerCommandPhase.Execution;
                        _activeCommand.CommandHandler();
                    }
                    break;

                //----------------------------------------
                //  FDC in execution phase reading/writing bytes
                //----------------------------------------
                case ControllerCommandPhase.Execution:
                    // set seek flag
                    ActiveFloppyDiskDrive.SeekStatus = (int)DriveSeekState.Seek;

                    if (ActiveFloppyDiskDrive.CurrentTrackId == _commandParameters[(int)CommandParameter.C])
                    {
                        // we are already on the correct track
                        ActiveFloppyDiskDrive.SectorIndex = 0;
                    }
                    else
                    {
                        // immediate seek
                        ActiveFloppyDiskDrive.CurrentTrackId = _commandParameters[(int)CommandParameter.C];

                        ActiveFloppyDiskDrive.SectorIndex = 0;

                        if (ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveFloppyDiskDrive.CurrentTrackId].Sectors.Length > 1)
                        {
                            // always read the first sector
                            //ActiveFloppyDiskDrive.SectorIndex++;
                        }
                    }

                    // skip execution mode and go directly to idle
                    // result is determined by SIS command
                    _activePhase = ControllerCommandPhase.Idle;
                    break;

                //----------------------------------------
                //  Result bytes being sent to CPU
                //----------------------------------------
                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
