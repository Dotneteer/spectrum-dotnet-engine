namespace SpectrumEngine.Emu;

/// <summary>
/// This class is intended to be a reusable base class for emulators using the Z80 CPU.
/// </summary>
public abstract class Z80MachineBase : IZ80Machine
{
    /// <summary>
    /// The folder where the ROM files are stored
    /// </summary>
    public const string ROM_RESOURCE_FOLDER = "Roms";

    /// <summary>
    /// This property stores the execution context where the emulated machine runs its execution loop.
    /// </summary>
    public ExecutionContext ExecutionContext { get; } = new();

    /// <summary>
    /// Get the Z80 instance associated with the machine.
    /// </summary>
    public IZ80Cpu Cpu { get; } = new Z80Cpu();

    /// <summary>
    /// Get the base clock frequency of the CPU. We use this value to calculate the machine frame rate.
    /// </summary>
    public int BaseClockFrequency { get; set; }

    /// <summary>
    /// This property gets or sets the value of the current clock multiplier.
    /// </summary>
    /// <remarks>
    /// By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock
    /// frequency multiplier to emulate a faster CPU.
    /// </remarks>
    public int ClockMultiplier { get; set; }

    /// <summary>
    /// This method provides a way to configure (or reconfigure) the emulated machine after changing the properties
    /// of its components.
    /// </summary>
    public virtual void Configure()
    {
        // --- Implement this method in derived classes
    }

    /// <summary>
    /// Executes the machine loop using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    public abstract LoopTerminationMode ExecuteMachineLoop();

    /// <summary>
    /// Emulates turning on a machine (after it has been turned off).
    /// </summary>
    public virtual void HardReset()
    {
        Cpu.HardReset();
    }

    /// <summary>
    /// This method emulates resetting a machine with a hardware reset button.
    /// </summary>
    public virtual void Reset()
    {
        Cpu.Reset();
    }

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    /// <remarks>
    /// Override in derived classes to implement hardware components running parallel with the CPU.
    /// </remarks>
    protected abstract void OnTactIncremented();

    /// <summary>
    /// Get the name of the default ROM's resource file within this assembly.
    /// </summary>
    protected abstract string DefaultRomResource { get; }

    /// <summary>
    /// Load the specified ROM from the current assembly resource.
    /// </summary>
    /// <param name="romName">Name of the ROM file to load</param>
    /// <param name="page">Optional ROM page for multi-rom machines</param>
    /// <returns>The byte array that represents the ROM contents</returns>
    /// <exception cref="InvalidOperationException">
    /// The ROM cannot be loaded from the named resource.
    /// </exception>
    public static byte[] LoadRomFromResource(string romName, int page = -1)
    {
        var resourceName = page == -1 ? romName : $"{romName}-{page}";
        var currentAsm = typeof(Z80MachineBase).Assembly;
        resourceName = $"{currentAsm.GetName().Name}.{ROM_RESOURCE_FOLDER}.{romName}.{resourceName}.rom";
        var resMan = currentAsm.GetManifestResourceStream(resourceName);
        if (resMan == null)
        {
            throw new InvalidOperationException($"Input stream for the '{romName}' .rom file not found.");
        }
        using var stream = new StreamReader(resMan).BaseStream;
        stream.Seek(0, SeekOrigin.Begin);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        return bytes;
    }
}
