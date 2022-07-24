namespace SpectrumEngine.Tools.Commands;

/// <summary>
/// Represents a token
/// </summary>
public record Token(
    // The raw text of the token
    string Text,
    // The type of the token
    TokenType Type,
    //The location of the token
    TokenLocation Location);

/// <summary>
/// This enumeration defines the available command token types
/// </summary>
public enum TokenType {
    Eof = -1,
    Ws = -2,
    InlineComment = -3,
    EolComment = -4,
    Unknown = 0,

    NewLine,
    Argument,
    Variable,
    Option,
    Path,
    Identifier,
    String,
    DecimalLiteral,
    HexadecimalLiteral,
    BinaryLiteral,
}

/// <summary>
/// Represents the location of a token
/// </summary>
public class TokenLocation {
    /// <summary>
    /// Start position in the source stream
    /// </summary>
    public int StartPos { get; set; }

    /// <summary>
    /// End position in the source stream
    /// </summary>
    public int EndPos { get; set; }

    /// <summary>
    /// Source code line of the token
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// The token's start column within the line
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// The tokens end column within the line
    /// </summary>
    public int EndColumn { get; set; }
}
