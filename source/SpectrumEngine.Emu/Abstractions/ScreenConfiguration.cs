namespace SpectrumEngine.Emu;

/// <summary>
/// This interface defines the data that can be used to render 
/// a Spectrum model's screen
/// </summary>
public sealed class ScreenConfiguration
{
    /// <summary>
    /// Number of lines used for vertical synch
    /// </summary>
    public int VerticalSyncLines { get; set; }

    /// <summary>
    /// The number of top border lines that are not visible
    /// when rendering the screen
    /// </summary>
    public int NonVisibleBorderTopLines { get; set; }

    /// <summary>
    /// The number of border lines before the display
    /// </summary>
    public int BorderTopLines { get; set; }

    /// <summary>
    /// Number of display lines
    /// </summary>
    public int DisplayLines { get; set; }

    /// <summary>
    /// The number of border lines after the display
    /// </summary>
    public int BorderBottomLines { get; set; }

    /// <summary>
    /// The number of bottom border lines that are not visible
    /// when rendering the screen
    /// </summary>
    public int NonVisibleBorderBottomLines { get; set; }

    /// <summary>
    /// Horizontal blanking time (HSync+blanking).
    /// Given in Z80 clock cycles.
    /// </summary>
    public int HorizontalBlankingTime { get; set; }

    /// <summary>
    /// The time of displaying left part of the border.
    /// Given in Z80 clock cycles.
    /// </summary>
    public int BorderLeftTime { get; set; }

    /// <summary>
    /// The time of displaying a pixel row.
    /// Given in Z80 clock cycles.
    /// </summary>
    public int DisplayLineTime { get; set; }

    /// <summary>
    /// The time of displaying right part of the border.
    /// Given in Z80 clock cycles.
    /// </summary>
    public int BorderRightTime { get; set; }

    /// <summary>
    /// The time used to render the nonvisible right part of the border.
    /// Given in Z80 clock cycles.
    /// </summary>
    public int NonVisibleBorderRightTime { get; set; }

    /// <summary>
    /// The time the data of a particular pixel should be prefetched
    /// before displaying it.
    /// Given in Z80 clock cycles.
    /// </summary>
    public int PixelDataPrefetchTime { get; set; }

    /// <summary>
    /// The time the data of a particular pixel attribute should be prefetched
    /// before displaying it.
    /// Given in Z80 clock cycles.
    /// </summary>
    public int AttributeDataPrefetchTime { get; set; }

    /// <summary>
    /// Sets the contention values to be used with the device
    /// </summary>
    public byte[] ContentionValues { get; init; } = Array.Empty<byte>();
}
