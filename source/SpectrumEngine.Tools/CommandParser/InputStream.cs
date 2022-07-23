namespace SpectrumEngine.Tools.CommandParser;

/// <summary>
/// This class represents the input stream of the command parser
/// </summary>
public class InputStream
{
    // --- Current stream position

    // --- Current line number

    // --- Current column number
    private int _column;

    /// <summary>
    /// Creates a stream that uses the specified source code 
    /// </summary>
    /// <param name="source">Source code string</param>
    public InputStream(string source)
    {
        Source = source;
    }

    /// <summary>
    /// The source code string to use
    /// </summary>
    public string Source { get; init; }

    /// <summary>
    /// Gets the specified part of the source code 
    /// </summary>
    public string GetSourceSpan(int start, int end) => Source.Substring(start, end - start + 1);

    /// <summary>
    /// Gets the current position in the stream. Starts from 0. 
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Gets the current line number. Starts from 1. 
    /// </summary>
    public int Line { get; private set; } = 1;

    /// <summary>
    /// Gets the current column number. Starts from0. 
    /// </summary>
    public int Column => _column;

    /// <summary>
    /// Peeks the next character in the stream 
    /// </summary>
    /// <returns>
    /// Null, if EOF; otherwise the current source code character
    /// </returns>
    public char? Peek() => Ahead(0);

    /// <summary>
    /// Looks ahead with `n` characters in the stream. 
    /// </summary>
    /// <param name="n">Number of positions to look ahead. Default: 1</param>
    /// <returns>Null, if EOF; otherwise the look-ahead character</returns>
    public char? Ahead(int n = 1)
    {
        return Position + n > Source.Length - 1
            ? null
            : Source[Position + n];
    }

    /// <summary>
    /// Gets the next character from the stream 
    /// </summary>
    public char? Get() {
        // --- Check for EOF
        if (Position >= Source.Length) {
            return null;
        }

        // --- Get the char, and keep track of position
        var ch = Source[Position++];
        if (ch == '\n') {
            Line++;
            _column = 0;
        } else {
            _column++;
        }
        return ch;
    }
}