namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents the state of the machine controller.
/// </summary>
public enum MachineControllerState
{
    /// <summary>
    /// The machine controller has just been initialized.
    /// </summary>
    None = 0,

    /// <summary>
    /// The machine has started and is running.
    /// </summary>
    Running,

    /// <summary>
    /// The controller is about to pause the machine.
    /// </summary>
    Pausing,

    /// <summary>
    /// The controller has paused the machine.
    /// </summary>
    Paused,

    /// <summary>
    /// The controller is about to stop the machine.
    /// </summary>
    Stopping,

    /// <summary>
    /// The controller has stopped the machine.
    /// </summary>
    Stopped
}
