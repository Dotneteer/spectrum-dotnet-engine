namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the behavior of a virtual machine that integrates the emulator from separate hardware
/// components, including the Z80 CPU, the memory, screen, keyboard, and many other devices.
/// </summary>
public interface IZ80Machine : IZ80Cpu
{
    /// <summary>
    /// The unique identifier of the machine type
    /// </summary>
    string MachineId { get; }
    
    /// <summary>
    /// The name of the machine type to display
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// This property stores the execution context where the emulated machine runs its execution loop.
    /// </summary>
    ExecutionContext ExecutionContext { get; }

    /// <summary>
    /// Gets the value of the machine property with the specified key
    /// </summary>
    /// <param name="key">Machine property key</param>
    /// <returns>Value of the property, if found; otherwise, null</returns>
    object? GetMachineProperty(string key);

    /// <summary>
    /// Sets the value of the specified machine property
    /// </summary>
    /// <param name="key">Machine property key</param>
    /// <param name="value">Machine property value</param>
    void SetMachineProperty(string key, object? value);
    
    /// <summary>
    /// This event fires when the state of a machine property changes.
    /// </summary>
    event EventHandler<(string propertyName, object? newValue)>? MachinePropertyChanged;

    /// <summary>
    /// Get the duration of a machine frame in milliseconds.
    /// </summary>
    double FrameTimeInMs { get; }

    /// <summary>
    /// This property gets or sets the value of the target clock multiplier to set when the next machine frame starts.
    /// </summary>
    /// <remarks>
    /// By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock
    /// frequency multiplier to emulate a faster CPU.
    /// </remarks>
    int TargetClockMultiplier { get; set; }

    /// <summary>
    /// This method provides a way to configure (or reconfigure) the emulated machine after changing the properties
    /// of its components.
    /// </summary>
    void Configure();

    /// <summary>
    /// Executes the machine frame using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    FrameTerminationMode ExecuteMachineFrame();

    /// <summary>
    /// Width of the screen in native machine screen pixels
    /// </summary>
    int ScreenWidthInPixels { get; }

    /// <summary>
    /// Height of the screen in native machine screen pixels
    /// </summary>
    int ScreenHeightInPixels { get; }

    /// <summary>
    /// Gets the buffer that stores the rendered pixels
    /// </summary>
    uint[] GetPixelBuffer();
    
    /// <summary>
    /// Set the status of the specified ZX Spectrum key.
    /// </summary>
    /// <param name="key">Key code</param>
    /// <param name="isDown">Indicates if the key is pressed down.</param>
    void SetKeyStatus(SpectrumKeyCode key, bool isDown);

    /// <summary>
    /// Emulates queued keystrokes as if those were pressed by the user
    /// </summary>
    void EmulateKeystroke();

    /// <summary>
    /// Adds an emulated keypress to the queue of the provider.
    /// </summary>
    /// <param name="startFrame">Frame count to start the emulation</param>
    /// <param name="frames">Number of frames to hold the emulation</param>
    /// <param name="primary">Primary key code</param>
    /// <param name="secondary">Optional secondary key code</param>
    /// <remarks>The provider can play back emulated key strokes</remarks>
    void QueueKeystroke(int startFrame, int frames, SpectrumKeyCode primary, SpectrumKeyCode? secondary);
}