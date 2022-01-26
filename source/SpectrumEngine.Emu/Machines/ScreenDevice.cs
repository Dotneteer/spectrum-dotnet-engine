using System.Collections.ObjectModel;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum screen device.
/// </summary>
public sealed class ScreenDevice : IScreenDevice
{
    // --- The current configuration
    private ScreenConfiguration _configuration;

    // --- The current value of the flash flag
    private bool _flashFlag;

    // --- Access the memory device directly
    private IMemoryDevice? _memoryDevice;

    /// <summary>
    /// Define the screen configuration attributes of ZX Spectrum 48K (PAL)
    /// </summary>
    public static readonly ScreenConfiguration ZxSpectrum48ScreenConfiguration = new()
    {
        VerticalSyncLines = 8,
        NonVisibleBorderTopLines = 8,
        BorderTopLines = 48,
        BorderBottomLines = 48,
        NonVisibleBorderBottomLines = 8,
        DisplayLines = 192,
        BorderLeftTime = 24,
        BorderRightTime = 24,
        DisplayLineTime = 128,
        HorizontalBlankingTime = 40,
        NonVisibleBorderRightTime = 8,
        PixelDataPrefetchTime = 2,
        AttributeDataPrefetchTime = 1
    };

    /// <summary>
    /// Define the screen configuration attributes of ZX Spectrum 48K (NTSC)
    /// </summary>
    public static readonly ScreenConfiguration ZxSpectrum48NtscScreenConfiguration = new()
    {
        VerticalSyncLines = 8,
        NonVisibleBorderTopLines = 16,
        BorderTopLines = 24,
        BorderBottomLines = 24,
        NonVisibleBorderBottomLines = 0,
        DisplayLines = 192,
        BorderLeftTime = 24,
        BorderRightTime = 24,
        DisplayLineTime = 128,
        HorizontalBlankingTime = 40,
        NonVisibleBorderRightTime = 8,
        PixelDataPrefetchTime = 2,
        AttributeDataPrefetchTime = 1
    };

    /// <summary>
    /// This table defines the ARGB colors for the 16 available colors on the ZX Spectrum 48K model.
    /// </summary>
    public static readonly ReadOnlyCollection<uint> SpectrumColors = new(new List<uint>
    {
        0xFF000000, // Black
        0xFF0000AA, // Blue
        0xFFAA0000, // Red
        0xFFAA00AA, // Magenta
        0xFF00AA00, // Green
        0xFF00AAAA, // Cyan
        0xFFAAAA00, // Yellow
        0xFFAAAAAA, // White
        0xFF000000, // Bright Black
        0xFF0000FF, // Bright Blue
        0xFFFF0000, // Bright Red
        0xFFFF00FF, // Bright Magenta
        0xFF00FF00, // Bright Green
        0xFF00FFFF, // Bright Cyan
        0xFFFFFF00, // Bright Yellow
        0xFFFFFFFF, // Bright White
    });

    /// <summary>
    /// Get or set the current border color.
    /// </summary>
    public int BorderColor { get; set; }

    /// <summary>
    /// This table defines the rendering information associated with the tacts of the ULA screen rendering frame.
    /// </summary>
    public RenderingTact[] RenderingTactTable { get; private set; } = Array.Empty<RenderingTact>();

    /// <summary>
    /// This value shows the refresh rate calculated from the base clock frequency of the CPU and the screen
    /// configuration (total #of screen rendering tacts per frame).
    /// </summary>
    public decimal RefreshRate { get; private set; }

    /// <summary>
    /// This value shows the number of frames after which the ULA toggles the flash flag. In the hardware machine,
    /// the flash flag toggles twice in a second.
    /// </summary>
    public int FlashToggleFrames { get; private set; }

    /// <summary>
    /// This flag indicates whether the flash is in the standard (false) or inverted (true) phase.
    /// </summary>
    public bool FlashFlag => _flashFlag;

    /// <summary>
    /// Initialize the screen device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ScreenDevice(IZxSpectrum48Machine machine)
    {
        Machine = machine;
        _configuration = ZxSpectrum48ScreenConfiguration;
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrum48Machine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        // --- Attach the screen device to the memory device
        _memoryDevice = Machine.MemoryDevice;

        // --- Set default color values
        BorderColor = 7;
        _flashFlag = false;

        // --- Calculate helper values from screen dimensions
        
        // --- Create helper tables for screen rendering
        InitializeInkAndPaperTables();
        InitializeRenderingTactTable();
    }

    /// <summary>
    /// Get or set the configuration of this device.
    /// </summary>
    public ScreenConfiguration Configuration
    {
        get => _configuration;
        set
        {
            _configuration = value;
            Reset();
        }
    }

    /// <summary>
    /// Initialize the helper tables that accelerate ink and paper color handling.
    /// </summary>
    private void InitializeInkAndPaperTables()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Initialize the helper tables that accelerate screen rendering by precalculating rendering tact information.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void InitializeRenderingTactTable()
    {
        throw new NotImplementedException();
    }
}

