﻿namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the properties and operations of the ZX Spectrum's screen device.
/// </summary>
public interface IScreenDevice: IGenericDevice<IZxSpectrumMachine>
{
    /// <summary>
    /// Get or set the configuration of this device.
    /// </summary>
    ScreenConfiguration Configuration { get; set; }

    /// <summary>
    /// Get or set the current border color.
    /// </summary>
    int BorderColor { get; set; }

    /// <summary>
    /// This table defines the rendering information associated with the tacts of the ULA screen rendering frame.
    /// </summary>
    RenderingTact[] RenderingTactTable { get; }

    /// <summary>
    /// This value shows the refresh rate calculated from the base clock frequency of the CPU and the screen
    /// configuration (total #of screen rendering tacts per frame).
    /// </summary>
    decimal RefreshRate { get; }

    /// <summary>
    /// This value shows the number of frames after which the ULA toggles the flash flag. In the hardware machine,
    /// the flash flag toggles twice in a second.
    /// </summary>
    int FlashToggleFrames { get; }

    /// <summary>
    /// This flag indicates whether the flash is in the standard (false) or inverted (true) phase.
    /// </summary>
    bool FlashFlag { get; }

    /// <summary>
    /// Get the number of raster lines (height of the rendered screen).
    /// </summary>
    int RasterLines { get; }

    /// <summary>
    /// Get the width of the rendered screen.
    /// </summary>
    int ScreenWidth { get; }

    /// <summary>
    /// Get the number of visible screen lines.
    /// </summary>
    int ScreenLines { get; }

    /// <summary>
    /// Render the pixel pair belonging to the specified frame tact.
    /// </summary>
    /// <param name="tact">Frame tact to render</param>
    void RenderTact(int tact);

    /// <summary>
    /// Gets the buffer that stores the rendered pixels
    /// </summary>
    uint[] GetPixelBuffer();

    /// <summary>
    /// This method signs that a new screen frame has been started
    /// </summary>
    void OnNewFrame();
}

