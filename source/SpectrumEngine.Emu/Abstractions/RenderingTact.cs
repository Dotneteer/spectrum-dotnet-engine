namespace SpectrumEngine.Emu;

/// <summary>
/// This structure defines information related to a particular tact of the ULA screen rendering frame.
/// </summary>
public struct RenderingTact
{
    /// <summary>
    /// Describe the rendering phase associated with the current tact.
    /// </summary>
    public RenderingPhase Phase { get; set; }

    /// <summary>
    /// Display memory address used in the particular tact
    /// </summary>
    public ushort PixelAddress { get; set; }

    /// <summary>
    /// Display memory address used in the particular tact
    /// </summary>
    public ushort AttributeAddress { get; set; }
}

/// <summary>
/// This enumeration defines the different phases of ULA rendering.
/// </summary>
public enum RenderingPhase : byte
{
    /// <summary>
    /// The ULA does not do any rendering.
    /// </summary>
    None = 0,

    /// <summary>
    /// The ULA sets the border color to display the current pixel.
    /// </summary>
    Border,

    /// <summary>
    /// The ULA sets the border color to display the current pixel. It prepares to display the first pixel in the row
    /// by pre-fetching the corresponding byte from the display memory.
    /// </summary>
    BorderFetchPixel,

    /// <summary>
    /// The ULA sets the border color to display the current pixel. It has already fetched the byte of eight pixels to
    /// display, and it prepares to display the first pixel in the row by pre-fetching the corresponding attribute
    /// byte from the display memory.
    /// </summary>
    BorderFetchPixelAttr,

    /// <summary>
    /// The ULA displays the subsequent two pixels of Byte1 sequentially during a single Z80 clock cycle.
    /// </summary>
    DisplayB1,

    /// <summary>
    /// The ULA displays the subsequent two pixels of Byte2 sequentially during a single Z80 clock cycle.
    /// </summary>
    DisplayB2,

    /// <summary>
    /// The ULA displays the subsequent two pixels of Byte1 sequentially during a single Z80 clock cycle. It prepares
    /// to display the pixels of the next byte in the row by pre-fetching the corresponding byte from the display
    /// memory.
    /// </summary>
    DisplayB1FetchB2,

    /// <summary>
    /// The ULA displays the subsequent two pixels of Byte1 sequentially during a single Z80 clock cycle. It prepares
    /// to display the pixels of the next byte in the row by pre-fetching the corresponding attribute from the display
    /// memory.
    /// </summary>
    DisplayB1FetchA2,

    /// <summary>
    /// The ULA displays the subsequent two pixels of Byte2 sequentially during a single Z80 clock cycle. It prepares
    /// to display the pixels of the next byte in the row by pre-fetching the corresponding byte from the display
    /// memory.
    /// </summary>
    DisplayB2FetchB1,

    /// <summary>
    /// The ULA displays the subsequent two pixels of Byte2 sequentially during a single Z80 clock cycle. It prepares
    /// to display the pixels of the next byte in the row by pre-fetching the corresponding attribute from the display
    /// memory.
    /// </summary>
    DisplayB2FetchA1
}

