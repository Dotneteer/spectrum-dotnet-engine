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
        /// Read ID
        /// COMMAND:    1 parameter byte
        /// EXECUTION:  The first correct ID information on the cylinder is stored in the data register
        /// RESULT:     7 result bytes
        /// </summary>
        private void ReadIdCommandHandler()
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

                    PushCommandByteInBuffer();

                    // was that the last parameter byte?
                    if (_commandParameterIndex == _activeCommand.ParameterBytesCount)
                    {
                        DriveLight = true;

                        // all parameter bytes received
                        ClearResultBuffer();
                        _statusRegisters0 = 0;
                        _statusRegisters1 = 0;
                        _statusRegisters2 = 0;
                        _statusRegisters3 = 0;

                        // set unit select
                        //SetUnitSelect(ActiveFloppyDiskDrive.ID, ref Status0);

                        // HD should always be 0
                        _statusRegisters0.UnSetBits(StatusRegisters0.HD);

                        if (!ActiveFloppyDiskDrive.IsReady)
                        {
                            // no disk, no tracks or motor is not on
                            // it is at this point the +3 detects whether a disk is present
                            // if not (and after another readid and SIS) it will eventually proceed to loading from tape
                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6 | StatusRegisters0.NR);

                            // setup the result buffer
                            _resultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
                            for (int i = 1; i < 7; i++)
                            {
                                _resultBuffer[i] = 0;
                            }

                            // move to result phase
                            _activePhase = ControllerCommandPhase.Result;
                            break;
                        }

                        var track = ActiveFloppyDiskDrive.Disk.DiskTracks.FirstOrDefault(a => a.TrackNumber == ActiveFloppyDiskDrive.CurrentTrackId);

                        if (track != null && track.NumberOfSectors > 0 && track.TrackNumber != 0xff)
                        {
                            // formatted track

                            // is the index out of bounds?
                            if (ActiveFloppyDiskDrive.SectorIndex >= track.NumberOfSectors)
                            {
                                // reset the index
                                ActiveFloppyDiskDrive.SectorIndex = 0;
                            }

                            if (ActiveFloppyDiskDrive.SectorIndex == 0 && ActiveFloppyDiskDrive.Disk.DiskTracks[ActiveFloppyDiskDrive.CurrentTrackId].Sectors.Length > 1)
                            {
                                // looks like readid always skips the first sector on a track
                                ActiveFloppyDiskDrive.SectorIndex++;
                            }

                            // read the sector data
                            var data = track.Sectors[ActiveFloppyDiskDrive.SectorIndex]; //.GetCHRN();
                            _resultBuffer[(int)CommandResultParameter.C] = data.TrackNumber;
                            _resultBuffer[(int)CommandResultParameter.H] = data.SideNumber;
                            _resultBuffer[(int)CommandResultParameter.R] = data.SectorID;
                            _resultBuffer[(int)CommandResultParameter.N] = data.SectorSize;

                            _resultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;

                            // check for DAM & CRC
                            //if (data.Status2.Bit(SR2_CM))
                            //SetBit(SR2_CM, ref ResBuffer[RS_ST2]);


                            // increment the current sector
                            ActiveFloppyDiskDrive.SectorIndex++;

                            // is the index out of bounds?
                            if (ActiveFloppyDiskDrive.SectorIndex >= track.NumberOfSectors)
                            {
                                // reset the index
                                ActiveFloppyDiskDrive.SectorIndex = 0;
                            }
                        }
                        else
                        {
                            // unformatted track?
                            CommitResultCHRN();

                            _statusRegisters0.SetBits(StatusRegisters0.IC_D6);
                            _resultBuffer[(int)CommandResultParameter.ST0] = (byte)_statusRegisters0;
                            _resultBuffer[(int)CommandResultParameter.ST1] = 0x01;
                        }

                        _activePhase = ControllerCommandPhase.Result;
                    }

                    break;

                //----------------------------------------
                //  FDC in execution phase reading/writing bytes
                //----------------------------------------
                case ControllerCommandPhase.Execution:
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
