using System.Collections.ObjectModel;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum screen device.
/// </summary>
public sealed class ScreenDevice : IScreenDevice
{
    // --- These are the default memory contention values we use
    private static byte[] DefaultContentionValues = new byte[] { 6, 5, 4, 3, 2, 1, 0, 0 };

    // --- We use this reference for the default contention values so that in the future, we can configure it (for
    // --- example, when implementing ZX Spectrum +2/+3)
    private byte[] _contentionValues = DefaultContentionValues;

    // --- The current configuration
    private ScreenConfiguration _configuration;

    // --- The current value of the flash flag
    private bool _flashFlag;

    // --- Access the memory device directly
    private IMemoryDevice? _memoryDevice;

    // --- Paper color indexes what flash is off
    private readonly byte[] _paperColorFlashOff = new byte[0x100];

    // --- Paper color indexes what flash is on
    private readonly byte[] _paperColorFlashOn = new byte[0x100];

    // --- Ink color indexes what flash is off
    private readonly byte[] _inkColorFlashOff = new byte[0x100];

    // --- Ink color indexes what flash is on
    private readonly byte[] _inkColorFlashOn = new byte[0x100];

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
    /// Get the index of the first display line.
    /// </summary>
    private int FirstDisplayLine { get; set; }

    /// <summary>
    /// Initialize the helper tables that accelerate ink and paper color handling.
    /// </summary>
    private void InitializeInkAndPaperTables()
    {
        // --- Iterate through all the 256 combinations of attribute values
        for (var attr = 0; attr < 0x100; attr++)
        {
            var ink = (byte)((attr & 0x07) | ((attr & 0x40) >> 3));
            var paper = (byte)(((attr & 0x38) >> 3) | ((attr & 0x40) >> 3));
            // --- Use normal paper and ink colors when flash is off and normal flash phase is active
            _paperColorFlashOff[attr] = paper;
            _inkColorFlashOff[attr] = ink;

            // --- Use normal paper and ink colors when flash is on and normal flash phase is active
            // --- Exchange paper and ink colors when flash is on and inverted flash phase is active
            _paperColorFlashOn[attr] = (attr & 0x80) != 0 ? ink : paper; ;
            _inkColorFlashOn[attr] = (attr & 0x80) != 0 ? paper : ink;
        }
    }

    /// <summary>
    /// Initialize the helper tables that accelerate screen rendering by precalculating rendering tact information.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void InitializeRenderingTactTable()
    {
        // --- Shortcut to the memory device
        var memDevice = Machine.MemoryDevice;

        // --- Calculate helper screen dimensions
        FirstDisplayLine =
            _configuration.VerticalSyncLines + 
            _configuration.NonVisibleBorderTopLines + 
            _configuration.BorderTopLines;
        var lastDisplayLine = 
            FirstDisplayLine + 
            _configuration.DisplayLines
            - 1;

        var rasterLines =
            FirstDisplayLine + 
            _configuration.DisplayLines + 
            _configuration.BorderBottomLines + 
            _configuration.NonVisibleBorderBottomLines;
        var screenLineTime = 
            _configuration.BorderLeftTime + 
            _configuration.DisplayLineTime + 
            _configuration.BorderRightTime + 
            _configuration.NonVisibleBorderRightTime +
            _configuration.HorizontalBlankingTime;

        // --- Determine the number of tacts in a machine frame
        var tactsInFrame = rasterLines * screenLineTime;

        // --- Notify the CPU about it
        Machine.Cpu.SetTactsInFrame(tactsInFrame);

        // --- Notify the memory device to allocate the contention array
        Machine.MemoryDevice.AllocateContentionValues(tactsInFrame);

        // --- Calculate the first and last visible lines
        var firstVisibleLine = _configuration.VerticalSyncLines + _configuration.NonVisibleBorderTopLines;
        var lastVisibleLine = rasterLines - _configuration.NonVisibleBorderBottomLines;
        var firstVisibleBorderTact = screenLineTime - _configuration.BorderLeftTime;

        // --- Calculate the last visible line tact
        var lastVisibleLineTact = _configuration.DisplayLineTime + _configuration.BorderRightTime;

        // --- Calculate border pixel and attribute fetch tacts
        var borderPixelFetchTact = screenLineTime - _configuration.PixelDataPrefetchTime;
        var borderAttrFetchTact = screenLineTime - _configuration.AttributeDataPrefetchTime;

        // --- Iterate through all tacts to create the rendering table
        RenderingTactTable = new RenderingTact[tactsInFrame];
        for (var tact = 0; tact < tactsInFrame; tact++)
        {
            // --- Init the current tact
            var currentTact = new RenderingTact
            {
                Phase = RenderingPhase.None,
                PixelAddress = 0,
                AttributeAddress = 0,
                PixelBufferIndex = 0,
                RenderingAction = () => { }
            };
            memDevice.SetContentionValue(tact, 0);

            // --- Calculate line index and the tact index within line
            var line = tact / screenLineTime;
            var tactInLine = tact % screenLineTime;

            // Test, if the current tact is visible
            if (
              (line >= firstVisibleLine) &&
              (line <= lastVisibleLine) &&
              (tactInLine < lastVisibleLineTact || tactInLine >= firstVisibleBorderTact)
            )
            {
                // --- Yes, the tact is visible.
                // --- Is it the first pixel/attr prefetch?
                var calculated = false;
                if (line == FirstDisplayLine - 1)
                {
                    if (tactInLine == borderPixelFetchTact - 1)
                    {
                        currentTact.Phase = RenderingPhase.Border;
                        memDevice.SetContentionValue(tact, _contentionValues[6]);
                        // TODO: Set action reference
                        calculated = true;
                    }
                    else if (tactInLine == borderPixelFetchTact)
                    {
                        // --- Yes, prefetch pixel data
                        currentTact.Phase = RenderingPhase.BorderFetchPixel;
                        currentTact.PixelAddress = CalcPixelAddress(line + 1, 0);
                        memDevice.SetContentionValue(tact, _contentionValues[7]);
                        // TODO: Set action reference
                        calculated = true;
                    }
                    else if (tactInLine == borderAttrFetchTact)
                    {
                        currentTact.Phase = RenderingPhase.BorderFetchAttr;
                        currentTact.PixelAddress = CalcAttrAddress(line + 1, 0);
                        memDevice.SetContentionValue(tact, _contentionValues[0]);
                        // TODO: Set action reference
                        calculated = true;
                    }
                }

                // --- Test, if it is in the display area
                if (!calculated)
                {
                    if (
                      (line >= FirstDisplayLine) &
                      (line <= lastDisplayLine) &
                      (tactInLine < _configuration.DisplayLineTime)
                    )
                    {
                        // --- Yes, it is the display area
                        // --- Carry out actions according to pixel tact
                        var pixelTact = tactInLine & 0x07;
                        switch (pixelTact)
                        {
                            case 0:
                                currentTact.Phase = RenderingPhase.DisplayB1FetchB2;
                                currentTact.PixelAddress = CalcPixelAddress(line, tactInLine + 4);
                                memDevice.SetContentionValue(tact, _contentionValues[1]);
                                // TODO: Set action reference
                                break;
                            case 1:
                                currentTact.Phase = RenderingPhase.DisplayB1FetchA2;
                                currentTact.PixelAddress = CalcAttrAddress(line, tactInLine + 3);
                                memDevice.SetContentionValue(tact, _contentionValues[2]);
                                // TODO: Set action reference
                                break;
                            case 2:
                                currentTact.Phase = RenderingPhase.DisplayB1;
                                memDevice.SetContentionValue(tact, _contentionValues[3]);
                                // TODO: Set action reference
                                break;
                            case 3:
                                currentTact.Phase = RenderingPhase.DisplayB1;
                                memDevice.SetContentionValue(tact, _contentionValues[4]);
                                // TODO: Set action reference
                                break;
                            case 4:
                                currentTact.Phase = RenderingPhase.DisplayB2;
                                memDevice.SetContentionValue(tact, _contentionValues[5]);
                                // TODO: Set action reference
                                break;
                            case 5:
                                currentTact.Phase = RenderingPhase.DisplayB2;
                                memDevice.SetContentionValue(tact, _contentionValues[6]);
                                // TODO: Set action reference
                                break;
                            case 6:
                                // --- Test, if there are more pixels to display in this line
                                if (tactInLine < (_configuration.DisplayLineTime - _configuration.PixelDataPrefetchTime))
                                {
                                    // --- Yes, there are still more bytes
                                    currentTact.Phase = RenderingPhase.DisplayB2FetchB1;
                                    currentTact.PixelAddress = CalcPixelAddress(line, tactInLine + _configuration.PixelDataPrefetchTime);
                                    memDevice.SetContentionValue(tact, _contentionValues[7]);
                                    // TODO: Set action reference
                                }
                                else
                                {
                                    // --- Last byte in this line
                                    currentTact.Phase = RenderingPhase.DisplayB2;
                                    // TODO: Set action reference
                                }
                                break;
                            case 7:
                                // --- Test, if there are more pixels to display in this line
                                if (tactInLine < (_configuration.DisplayLineTime - _configuration.AttributeDataPrefetchTime))
                                {
                                    // --- Yes, there are still more bytes
                                    currentTact.Phase = RenderingPhase.DisplayB2FetchA1;
                                    currentTact.PixelAddress = CalcAttrAddress(line, tactInLine + _configuration.AttributeDataPrefetchTime);
                                    memDevice.SetContentionValue(tact, _contentionValues[0]);
                                    // TODO: Set action reference
                                }
                                else
                                {
                                    // --- Last byte in this line
                                    currentTact.Phase = RenderingPhase.DisplayB2;
                                    // TODO: Set action reference
                                }
                                break;

                        }
                    }
                    else
                    {
                        // --- It is the border area
                        currentTact.Phase = RenderingPhase.Border;

                        // --- Left or right border?
                        if (line >= FirstDisplayLine)
                        {
                            if (line < lastDisplayLine)
                            {
                                // -- Yes, it is left or right border
                                // --- Is it pixel data prefetch time?
                                if (tactInLine == borderPixelFetchTact)
                                {
                                    // --- Yes, prefetch pixel data
                                    currentTact.Phase = RenderingPhase.BorderFetchPixel;
                                    currentTact.PixelAddress = CalcPixelAddress(line + 1, 0);
                                    memDevice.SetContentionValue(tact, _contentionValues[7]);
                                    // TODO: Set action reference
                                }
                                else if (tactInLine == borderAttrFetchTact)
                                {
                                    currentTact.Phase = RenderingPhase.BorderFetchAttr;
                                    currentTact.PixelAddress = CalcAttrAddress(line + 1, 0);
                                    memDevice.SetContentionValue(tact, _contentionValues[0]);
                                    // TODO: Set action reference
                                }
                            }
                        }
                    }
                }
            }

            // --- Store the current rendering item
            RenderingTactTable[tact] = currentTact;
        }
    }

    /// <summary>
    /// Calculate the pixel address of the specified tact.
    /// </summary>
    /// <param name="line">Line index</param>
    /// <param name="tactInLine">Tact within the line</param>
    /// <returns>The calculated pixel address</returns>
    private ushort CalcPixelAddress(int line, int tactInLine)
    {
        int row = line - FirstDisplayLine;
        return (ushort)
          (((row & 0xc0) << 5) +
          ((row & 0x07) << 8) +
          ((row & 0x38) << 2) +
          (tactInLine >> 2));
    }

    /// <summary>
    /// Calculate the attribute address of the specified tact.
    /// </summary>
    /// <param name="line">Line index</param>
    /// <param name="tactInLine">Tact within the line</param>
    /// <returns>The calculated attribute address</returns>
    private ushort CalcAttrAddress(int line, int tactInLine)
    {
        return (ushort)
          ((tactInLine >> 2) +
          (((line - FirstDisplayLine) >> 3) << 5) +
          0x1800);
    }
}

