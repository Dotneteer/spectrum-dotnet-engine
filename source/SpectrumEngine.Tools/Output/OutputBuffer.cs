using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SpectrumEngine.Tools.Output;

/// <summary>
/// Represents an output buffer we can write
/// </summary>
public sealed class OutputBuffer
{
    private OutputColors _color = OutputColors.White;
    private OutputColors _background = OutputColors.Transparent;
    private bool _isBold;
    private bool _isItalic;
    private bool _isUnderline;
    private bool _isStrikethrough;
    
    /// <summary>
    /// Clears the contents of the output buffer 
    /// </summary>
    public void Clear()
    {
        Contents = new List<List<OutputSection>>();
        ResetFormat();
        RaiseContentsChanged();
    }

    /// <summary>
    /// Gets the contents of the buffer 
    /// </summary>
    /// <returns></returns>
    public List<List<OutputSection>> Contents { get; private set; } = new();

    /// <summary>
    /// Gets or sets the current text color
    /// </summary>
    public OutputColors Color
    {
        get => _color;
        set {
            if (_color == value ) return;
            
            var lastSection = GetLastSection();
            if (lastSection != null && string.IsNullOrEmpty(lastSection.Text))
            {
                lastSection.Color = value;
                return;
            }
            
            OpenSection(new OutputSection
            {
                Color = value
            });
            _color = value;
        }
    }

    /// <summary>
    /// Gets or sets the current text color
    /// </summary>
    public OutputColors Background
    {
        get => _background;
        set {
            if (_background == value) return;
            
            var lastSection = GetLastSection();
            if (lastSection != null && string.IsNullOrEmpty(lastSection.Text))
            {
                lastSection.Background = value;
                return;
            }
            
            OpenSection(new OutputSection
            {
                Background = value
            });
            _background = value;
        }
    }

    /// <summary>
    /// Indicates if the font is to be used in bold
    /// </summary>
    public bool Bold
    {
        get => _isBold;
        set {
            if (_isBold == value) return;

            var lastSection = GetLastSection();
            if (lastSection != null && string.IsNullOrEmpty(lastSection.Text))
            {
                lastSection.Bold = value;
                return;
            }
            
            OpenSection(new OutputSection
            {
                Bold = value
            });
            _isBold = value;
        }
    }

    /// <summary>
    /// Indicates if the font is to be used in italic
    /// </summary>
    public bool Italic
    {
        get => _isItalic;
        set {
            if (_isItalic == value) return;
            
            var lastSection = GetLastSection();
            if (lastSection != null && string.IsNullOrEmpty(lastSection.Text))
            {
                lastSection.Italic = value;
                return;
            }

            OpenSection(new OutputSection
            {
                Italic = value
            });
            _isItalic = value;
        }
    }

    /// <summary>
    /// Indicates if the font is to be used with underline
    /// </summary>
    public bool Underline
    {
        get => _isUnderline;
        set {
            if (_isUnderline == value) return;

            var lastSection = GetLastSection();
            if (lastSection != null && string.IsNullOrEmpty(lastSection.Text))
            {
                lastSection.Underline = value;
                return;
            }
            
            OpenSection(new OutputSection
            {
                Underline = value
            });
            _isUnderline = value;
        }
    }

    /// <summary>
    /// Indicates if the font is to be used with strikethrough 
    /// </summary>
    public bool Strikethrough
    {
        get => _isStrikethrough;
        set {
            if (_isItalic == value) return;
            
            var lastSection = GetLastSection();
            if (lastSection != null && string.IsNullOrEmpty(lastSection.Text))
            {
                lastSection.Strikethrough = value;
                return;
            }
            
            OpenSection(new OutputSection
            {
                Strikethrough = value
            });
            _isStrikethrough = value;
        }
    }

    /// <summary>
    /// Sets the default color
    /// </summary>
    public void ResetFormat()
    {
        Color = OutputColors.White;
        Background = OutputColors.Transparent;
        Bold = false;
        Italic = false;
        Underline = false;
        Strikethrough = false;
    }

    /// <summary>
    /// Writes a new entry to the output
    /// </summary>
    /// <param name="message">Message to write</param>
    /// <param name="action">Optional action</param>
    public void Write(string message, Action<object?>? action = null)
    {
        // --- Start a new section with an action
        if (action != null)
        {
            NewSection();
            return;
        }

        var lastSection = GetLastSection();

        // --- Start a new section if no section exists yet
        if (lastSection == null)
        {
            NewSection();
            return;
        }

        // --- Start a new section if the old section has an action
        if (lastSection.Action != null)
        {
            NewSection();
            return;
        }

        // --- Extend the text of the last action
        lastSection.Text = (lastSection.Text ?? "") + message;

        void NewSection()
        {
            OpenSection(new OutputSection
            {
                Text = message,
                Action = action,
                Color = Color,
                Background = Background,
                Underline = Underline,
                Strikethrough = Strikethrough
            });
        }
    }

    /// <summary>
    /// Writes a new entry to the output and adds a new line
    /// </summary>
    /// <param name="message">Message to write</param>
    /// <param name="action">Optional action</param>
    public void WriteLine(string message, Action<object?>? action = null)
    {
        Write(message, action);
        Contents.Add(new List<OutputSection>());
    }

    /// <summary>
    /// This event fires when the contents of the buffer changes.
    /// </summary>
    event EventHandler? ContentsChanged;

    /// <summary>
    /// Gets the last output section
    /// </summary>
    public OutputSection? GetLastSection()
    {
        if (Contents.Count == 0) return null;
        var lastLine = Contents[^1];
        return lastLine.Count == 0 ? null : lastLine[^1];
    }

    /// <summary>
    /// Tests if the buffer has open text section
    /// </summary>
    private bool HasOpenText => !string.IsNullOrEmpty(GetLastSection()?.Text);
    
    /// <summary>
    /// Closes the last segment and opens a new one
    /// </summary>
    /// <param name="section">Section to open</param>
    private void OpenSection(OutputSection section)
    {
        if (Contents.Count == 0)
        {
            Contents.Add(new List<OutputSection> { section } );
        }
        else
        {
            Contents[^1].Add(section);
        }
        RaiseContentsChanged();
    }
    
    /// <summary>
    /// Fire the ContenstChanged event
    /// </summary>
    private void RaiseContentsChanged() => ContentsChanged?.Invoke(this, EventArgs.Empty);

    public static Brush GetBrushFor(OutputColors color)
    {
        if (color == OutputColors.Transparent) return new SolidColorBrush(Colors.Transparent);
        
        var colorResource = color switch
        {
            OutputColors.Black => "ConsoleBlack",
            OutputColors.Blue => "ConsoleBlue",
            OutputColors.Cyan => "ConsoleCyan",
            OutputColors.Green => "ConsoleGreen",
            OutputColors.Magenta => "ConsoleMagenta",
            OutputColors.Red => "ConsoleRed",
            OutputColors.White => "ConsoleWhite",
            OutputColors.Yellow => "ConsoleYellow",
            OutputColors.BrightBlack => "ConsoleBrightBlack",
            OutputColors.BrightBlue => "ConsoleBrightBlue",
            OutputColors.BrightCyan => "ConsoleBrightCyan",
            OutputColors.BrightGreen => "ConsoleBrightGreen",
            OutputColors.BrightMagenta => "ConsoleBrightMagenta",
            OutputColors.BrightRed => "ConsoleBrightRed",
            OutputColors.BrightWhite => "ConsoleBrightWhite",
            OutputColors.BrightYellow => "ConsoleBrightYellow",
            _ => ""
        };
        var value = Application.Current!.FindResource(colorResource);
        return new SolidColorBrush(value is Color colorValue ? colorValue : Colors.OrangeRed);
    }
}


/// <summary>
/// Available output colors
/// </summary>
public enum OutputColors
{
    Transparent,
    Black,
    Red,
    Green,
    Yellow,
    Blue,
    Magenta,
    Cyan,
    White,
    BrightBlack,
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
public record OutputSection
{
    public string? Text { get; set; }
    public OutputColors Color { get; set; } = OutputColors.White;
    public OutputColors Background { get; set; } = OutputColors.Transparent;
    public Action<object?>? Action { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strikethrough { get; set; }
}
