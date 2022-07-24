namespace SpectrumEngine.Tools.Commands;

/// <summary>
/// Represents an output buffer we can write
/// </summary>
public sealed class OutputBuffer
{
    private OutputColors _currentColor = OutputColors.White;
    
    /// <summary>
    /// Clears the contents of the output buffer 
    /// </summary>
    public void Clear()
    {
        Contents = new List<List<OutputSection>>();
    }

    /// <summary>
    /// Gets the contents of the buffer 
    /// </summary>
    /// <returns></returns>
    public List<List<OutputSection>> Contents { get; private set; } = new();

    /// <summary>
    /// 
    /// Sets the default color
    /// </summary>
    public void ResetColor()
    {
        _currentColor = OutputColors.White;
    }

    /// <summary>
    /// Sets the output to the specified color
    /// </summary>
    /// <param name="color">Color to use</param>
    public void Color(OutputColors color)
    {
        // TODO: Close the current segment
        _currentColor = color;
    }

    /// <summary>
    /// Indicates if the font is to be used in bold
    /// </summary>
    /// <param name="use">True to use bold; otherwise, false</param>
    public void Bold(bool use)
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// Indicates if the font is to be used in italic
    /// </summary>
    /// <param name="use">True to use italic; otherwise, false</param>
    public void Italic(bool use)
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// Indicates if the font is to be used with underline
    /// </summary>
    /// <param name="use">True to use underline; otherwise, false</param>
    public void Underline(bool use)
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// Indicates if the font is to be used with strikethrough
    /// </summary>
    /// <param name="use">True to use strikethrough; otherwise, false</param>
    public void StrikeThrough(bool use)
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// Writes a new entry to the output
    /// </summary>
    /// <param name="message">Message to write</param>
    /// <param name="action">Optional action</param>
    public void Write(string message, Action<object?>? action = null)
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// Writes a new entry to the output and adds a new line
    /// </summary>
    /// <param name="message">Message to write</param>
    /// <param name="action">Optional action</param>
    public void WriteLine(string message, Action<object?>? action = null)
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// This event fires when the contents of the buffer changes.
    /// </summary>
    event EventHandler ContentsChanged;
}


/// <summary>
/// Available output colors
/// </summary>
public enum OutputColors
{
    Black,
    Red,
    Green,
    Yellow,
    Blue,
    Magenta,
    Cyan,
    White,
    BrightRed,
    BrightGreen,
    BrightYellow,
    BrightBlue,
    BrightMagenta,
    BrightCyan,
    BrightWhite,
}

/// <summary>
/// Represent an output section within a section line
/// </summary>
/// <param name="Text">Text to display</param>
public record OutputSection(string Text)
{
    public OutputColors Color { get; set; }
    public Action<object?>? Action { get; set; }
    public bool Bold { get; set; }
    public bool Italoc { get; set; }
    public bool Underline { get; set; }
    public bool StrikeThrough { get; set; }
}
