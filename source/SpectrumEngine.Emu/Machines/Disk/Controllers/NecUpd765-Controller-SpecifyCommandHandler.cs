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
            switch (_activePhase)
            {
                case ControllerCommandPhase.Idle:
                    break;

                case ControllerCommandPhase.Command:

                    // store the parameter in the command buffer
                    _commandParameters[_commandParameterIndex] = _lastByteReceived;

                    // increment command parameter counter
                    _commandParameterIndex++;

                    // was that the last parameter byte?
                    if (_commandParameterIndex == _activeCommandConfiguration.ParameterBytesCount)
                    {
                        // all parameter bytes received

                        // nothing useful to do in specify, this just configures some
                        // timing and DMA-mode params that are not relevant for this emulation
                        _activePhase = ControllerCommandPhase.Idle;
                    }

                    break;

                case ControllerCommandPhase.Execution:
                    break;

                case ControllerCommandPhase.Result:
                    break;
            }
        }

    }
}
