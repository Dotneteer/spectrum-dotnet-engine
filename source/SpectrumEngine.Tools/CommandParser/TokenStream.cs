namespace SpectrumEngine.Tools.CommandParser;

/**
 * This class implements the tokenizer (lexer) of the Klive command parser.
 * It recognizes these tokens:
 *
 * Identifier
 *   : idStart idContinuation*
 *   ;
 *
 * idStart
 *   : 'a'..'z' | 'A'..'Z' | '_'
 *   ;
 *
 * idContinuation
 *   : idStart | '0'..'9' | '-' | '$' | '.' | '!' | ':' | '#'
 *   ;
 *
 *  Variable
 *   : '${' Identifier '}'
 *   ;
 *
 * Option
 *   : '-' idStart idContinuation*
 *   ;
 *
 * Path
 *   : pathStart pathContinuation*
 *   ;
 *
 * pathStart
 *   : 'a'..'z' | 'A'..'Z' | '_' | '/' | '\' | '.' | '*' | '?'
 *   ;
 *
 * pathContinuation
 *   : pathStart | ':'
 *   ;
 *
 * String
 *   : '"' (stringChar | escapeChar) '"'
 *   ;
 *
 * stringChar
 *   : [any characted except NL or CR]
 *   ;
 *
 * escapeChar
 *   : '\b' | '\f' | '\n' | '\r' | '\t' | '\v' | '\0' | '\'' | '\"' | '\\'
 *   | '\x' hexadecimalDigit hexadecimalDigit
 *   ;
 *
 * Number
 *   : '-'? (decimalNumber | hexadecimalNumber | binaryNumber)
 *   ;
 *
 * decimalNumber
 *   : decimalDigit xDecimalDigit*
 *   ;
 *
 * hexadecimalNumber
 *   : '$' hexadecimalDigit xHexadecimalDigit*
 *   ;
 *
 * binaryNumber
 *   : '%' binarydigit xBinaryDigit*
 *   ;
 *
 * decimalDigit
 *   : '0'..'9'
 *   ;
 *
 * xDecimalDigit
 *   : decimalDigit | '_' | ''''
 *   ;
 *
 * hexadecimalDigit
 *   : '0'..'9' | 'a'..'f' | 'A'..'F'
 *   ;
 *
 * xHexadecimalDigit
 *   : hexadecimalDigit | '_' | ''''
 *   ;
 *
 * binaryDigit
 *   : '0' | '1'
 *   ;
 *
 * xBinaryDigit
 *   : binaryDigit | '_' | ''''
 *   ;
 *
 * Argument
 *   : [any characted except NL, CR, or other whitespace]+
 *   ;
 */
public class TokenStream
{
  // --- Prefetched character (from the next token)
  private char? _prefetched;

  // --- Prefetched character position (from the next token)
  private int? _prefetchedPos;

  // --- Prefetched character column (from the next token)
  private int? _prefetchedColumn;

  /**
   * Initializes the tokenizer with the input stream
   * @param input Input source code stream
   */
  public TokenStream(InputStream input)
  {
    Input = input;
  }

  /// <summary>
  /// The input stream of the command
  /// </summary>
  public InputStream Input { get; init; }
  
  /// <summary>
  /// Parses the specified command into a token list
  /// </summary>
  /// <param name="command">Command to parse</param>
  /// <returns>Parsed command</returns>
  public static List<Token> ParseCommand(string command)
  {
    var tokens = new List<Token>();
    var input = new InputStream(command);
    var tokenStream = new TokenStream(input);
    bool eof;
    do {
      var nextToken = tokenStream.Get();
      eof = nextToken.Type == TokenType.Eof;
      if (!eof) {
        tokens.Add(nextToken);
      }
    } while (!eof);
    return tokens;
  }

  /// <summary>
  /// Fethces the nex token and advances the stream position 
  /// </summary>
  /// <param name="ws">If true, retrieve whitespaces too</param>
  private Token Get(bool ws = false)
  {
    while (true)
    {
      var token = Fetch();
      if (IsEof(token) || ws || (!ws && !IsWs(token)))
      {
        return token;
      }
    }
  }

  /// <summary>
  /// Fetches the next token from the input stream
  /// </summary>
  /// <returns>The next token</returns>
  private Token Fetch()
  {
    var startPos = _prefetchedPos ?? Input.Position;
    var line = Input.Line;
    var startColumn = _prefetchedColumn ?? Input.Column;
    var text = "";
    var tokenType = TokenType.Eof;
    var lastEndPos = Input.Position;
    var lastEndColumn = Input.Column;
    char? ch;

    var phase = LexerPhase.Start;
    while (true)
    {
      // --- Get the next character
      ch = FetchNextChar();

      // --- In case of EOF, return the current token data
      if (ch == null)
      {
        return MakeToken();
      }

      // --- Set the intial token type to unknown for the other characters
      if (tokenType == TokenType.Eof)
      {
        tokenType = TokenType.Unknown;
      }

      // --- Follow the lexer state machine
      switch (phase)
      {
        // ====================================================================
        // Process the first character
        case LexerPhase.Start:
          switch (ch)
          {
            // --- Go on with whitespaces
            case ' ':
            case '\t':
              phase = LexerPhase.InWhiteSpace;
              tokenType = TokenType.Ws;
              break;

            // --- New line
            case '\n':
              return CompleteToken(TokenType.NewLine);

            // --- Potential new line
            case '\r':
              phase = LexerPhase.PotentialNewLine;
              tokenType = TokenType.NewLine;
              break;

            case '"':
              phase = LexerPhase.String;
              break;

            case '-':
              phase = LexerPhase.OptionOrNumber;
              break;

            case '$':
              phase = LexerPhase.VariableOrHexaDecimal;
              break;

            case '%':
              phase = LexerPhase.Binary;
              break;

            default:
              if (IsIdStart(ch))
              {
                phase = LexerPhase.IdTail;
                tokenType = TokenType.Identifier;
              }
              else if (IsDecimalDigit(ch))
              {
                phase = LexerPhase.Decimal;
                tokenType = TokenType.DecimalLiteral;
              }
              else if (IsPathStart(ch))
              {
                phase = LexerPhase.PathTail;
                tokenType = TokenType.Path;
              }

              break;
          }

          break;

        // ====================================================================
        // Process whitespaces, comments, and new line

        // --- Looking for the end of whitespace
        case LexerPhase.InWhiteSpace:
          if (ch != ' ' && ch != '\t')
          {
            return MakeToken();
          }

          break;

        // --- We already received a "\r", so this is a new line
        case LexerPhase.PotentialNewLine:
          return ch == '\n' ? CompleteToken(TokenType.NewLine) : MakeToken();

        // ====================================================================
        // Identifier and keyword like tokens

        // --- Wait for the completion of an identifier
        case LexerPhase.IdTail:
          if (IsIdContinuation(ch))
          {
            break;
          }

          if (IsTokenSeparator(ch))
          {
            return MakeToken();
          }

          if (IsPathContinuation(ch))
          {
            phase = LexerPhase.PathTail;
            tokenType = TokenType.Path;
          }
          else
          {
            phase = LexerPhase.ArgumentTail;
            tokenType = TokenType.Argument;
          }

          break;

        case LexerPhase.PathTail:
          if (IsPathContinuation(ch))
          {
            break;
          }

          if (IsTokenSeparator(ch))
          {
            return MakeToken();
          }

          phase = LexerPhase.ArgumentTail;
          tokenType = TokenType.Argument;
          break;

        // ====================================================================
        // Variables

        case LexerPhase.VariableOrHexaDecimal:
          if (ch == '{')
          {
            phase = LexerPhase.Variable;
          }
          else if (IsHexadecimalDigit(ch))
          {
            phase = LexerPhase.HexaDecimalTail;
            tokenType = TokenType.HexadecimalLiteral;
          }
          else
          {
            phase = LexerPhase.ArgumentTail;
            tokenType = TokenType.Argument;
          }

          break;

        // We already parsed "${"
        case LexerPhase.Variable:
          if (IsIdStart(ch))
          {
            phase = LexerPhase.VariableTail;
          }
          else
          {
            return CompleteToken();
          }

          break;

        // We identified the start character of a variable, and wait for continuation
        case LexerPhase.VariableTail:
          if (IsIdContinuation(ch))
          {
            break;
          }

          return ch == '}'
            ? CompleteToken(TokenType.Variable)
            : CompleteToken(TokenType.Unknown);

        // ====================================================================
        // --- Options

        case LexerPhase.OptionOrNumber:
          switch (ch)
          {
            case '$':
              phase = LexerPhase.Hexadecimal;
              break;
            case '%':
              phase = LexerPhase.Binary;
              break;
            default:
            {
              if (IsDecimalDigit(ch))
              {
                phase = LexerPhase.Decimal;
                tokenType = TokenType.DecimalLiteral;
              }
              else if (IsIdStart(ch))
              {
                phase = LexerPhase.OptionTail;
                tokenType = TokenType.Option;
              }
              else
              {
                tokenType = TokenType.Argument;
                phase = LexerPhase.ArgumentTail;
              }

              break;
            }
          }

          break;

        case LexerPhase.OptionTail:
          if (IsIdContinuation(ch))
          {
            break;
          }

          if (IsTokenSeparator(ch))
          {
            return MakeToken();
          }

          tokenType = TokenType.Argument;
          phase = LexerPhase.ArgumentTail;
          break;

        case LexerPhase.ArgumentTail:
          if (IsTokenSeparator(ch))
          {
            return MakeToken();
          }

          break;

        // ====================================================================
        // --- Literals

        // String data
        case LexerPhase.String:
          if (ch == '"')
          {
            return CompleteToken(TokenType.String);
          }

          if (IsRestrictedInString(ch))
          {
            return CompleteToken(TokenType.Unknown);
          }

          if (ch == '\\')
          {
            phase = LexerPhase.StringBackSlash;
            tokenType = TokenType.Unknown;
          }

          break;

        // Start of string character escape
        case LexerPhase.StringBackSlash:
          switch (ch)
          {
            case 'b':
            case 'f':
            case 'n':
            case 'r':
            case 't':
            case 'v':
            case '0':
            case '\'':
            case '"':
            case '\\':
              phase = LexerPhase.String;
              break;
            default:
              phase = ch == 'x' ? LexerPhase.StringHexa1 : LexerPhase.String;
              break;
          }

          break;

        // First hexadecimal digit of string character escape
        case LexerPhase.StringHexa1:
          if (IsHexadecimalDigit(ch))
          {
            phase = LexerPhase.StringHexa2;
          }
          else
          {
            return CompleteToken(TokenType.Unknown);
          }

          break;

        // Second hexadecimal digit of character escape
        case LexerPhase.StringHexa2:
          if (IsHexadecimalDigit(ch))
          {
            phase = LexerPhase.String;
          }
          else
          {
            return CompleteToken(TokenType.Unknown);
          }

          break;

        // The first character after "$"
        case LexerPhase.Hexadecimal:
          if (IsHexadecimalDigit(ch))
          {
            phase = LexerPhase.HexaDecimalTail;
            tokenType = TokenType.HexadecimalLiteral;
          }
          else
          {
            tokenType = TokenType.Argument;
            phase = LexerPhase.ArgumentTail;
          }

          break;

        case LexerPhase.HexaDecimalTail:
          if (IsXHexadecimalDigit(ch))
          {
            break;
          }

          if (IsTokenSeparator(ch))
          {
            return MakeToken();
          }

          tokenType = TokenType.Argument;
          phase = LexerPhase.ArgumentTail;
          break;

        // The first character after "%"
        case LexerPhase.Binary:
          if (IsBinaryDigit(ch))
          {
            phase = LexerPhase.BinaryTail;
            tokenType = TokenType.BinaryLiteral;
          }
          else
          {
            tokenType = TokenType.Argument;
            phase = LexerPhase.ArgumentTail;
          }

          break;

        case LexerPhase.BinaryTail:
          if (IsXBinaryDigit(ch))
          {
            break;
          }

          if (IsTokenSeparator(ch))
          {
            return MakeToken();
          }

          tokenType = TokenType.Argument;
          phase = LexerPhase.ArgumentTail;
          break;

        // The first decimal lieterl character
        case LexerPhase.Decimal:
          if (IsDecimalDigit(ch))
          {
            phase = LexerPhase.DecimalTail;
            tokenType = TokenType.DecimalLiteral;
          }
          else if (IsTokenSeparator(ch))
          {
            return MakeToken();
          }
          else
          {
            tokenType = TokenType.Argument;
            phase = LexerPhase.ArgumentTail;
          }

          break;

        case LexerPhase.DecimalTail:
          if (IsXDecimalDigit(ch))
          {
            break;
          }

          if (IsTokenSeparator(ch))
          {
            return MakeToken();
          }

          tokenType = TokenType.Argument;
          phase = LexerPhase.ArgumentTail;
          break;

        // ====================================================================
        // --- We cannot continue
        default:
          return MakeToken();
      }

      // --- Append the char to the current text
      AppendTokenChar();

      // --- Go on with parsing the next character
    }

    // --- Appends the last character to the token, and manages positions
    void AppendTokenChar()
    {
      text += ch;
      _prefetched = null;
      _prefetchedPos = null;
      _prefetchedColumn = null;
      lastEndPos = Input.Position;
      lastEndColumn = Input.Position;
    }

    // --- Fetches the next character from the input stream 
    char? FetchNextChar()
    {
      if (_prefetched == null)
      {
        _prefetchedPos = Input.Position;
        _prefetchedColumn = Input.Column;
        _prefetched = Input.Get();
      }

      return _prefetched;
    }

    // --- Packs the specified type of token to send back 
    Token MakeToken()
    {
      return new Token(
        text,
        tokenType,
        new TokenLocation
        {
          StartPos = startPos,
          EndPos = lastEndPos,
          Line = line,
          StartColumn = startColumn,
          EndColumn = lastEndColumn,
        }
      );
    }

    // --- Add the last character to the token and return it 
    Token CompleteToken(TokenType? suggestedType = null)
    {
      AppendTokenChar();

      // --- Send back the token
      if (suggestedType.HasValue)
      {
        tokenType = suggestedType.Value;
      }

      return MakeToken();
    }
  }

  /// <summary>
  /// Tests if a token is EOF
  /// </summary>
  private static bool IsEof(Token t) => t.Type == TokenType.Eof;

  /// <summary>
  /// Tests if a token is whitespace
  /// </summary>
  private static bool IsWs(Token t) => t.Type <= TokenType.Ws;

  /// <summary>
  /// Tests if a character can be the start of an identifier
  /// </summary>
  private static bool IsIdStart(char? ch) => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';

  /// <summary>
  /// Tests if a character can be the continuation of an identifier
  /// </summary>
  private static bool IsIdContinuation(char? ch) =>
  (
    IsIdStart(ch) ||
    IsDecimalDigit(ch) ||
    ch is '-' or '$' or '.' or '!' or ':' or '#'
  );

  /// <summary>
  /// Tests if a character can be the start of a path
  /// </summary>
  private static bool IsPathStart(char? ch)
    => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '/' or '\\' or '.' or '*' or '?';

  /// <summary>
  /// Tests if a character can be the continuation of a path
  /// </summary>
  private static bool IsPathContinuation(char? ch) => IsPathStart(ch) || ch == ':';

  /// <summary>
  /// Tests if a character is a decimal digit
  /// </summary>
  private static bool IsDecimalDigit(char? ch) => ch is >= '0' and <= '9';

  /// <summary>
  /// Tests if a character is an extended decimal digit
  /// </summary>
  private static bool IsXDecimalDigit(char? ch) => IsDecimalDigit(ch) || ch is '_' or '\'';

  /// <summary>
  /// Tests if a character is a hexadecimal digit
  /// </summary>
  private static bool IsHexadecimalDigit(char? ch)
    => ch is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';

  /// <summary>
  /// Tests if a character is a hexadecimal digit
  /// </summary>
  private static bool IsXHexadecimalDigit(char? ch) => IsHexadecimalDigit(ch) || ch is '_' or '\'';

  /// <summary>
  /// Tests if a character is a binary digit
  /// </summary>
  private static bool IsBinaryDigit(char? ch) => ch is '0' or '1';

  /// <summary>
  /// Tests if a character is an extended binary digit
  /// </summary>
  private static bool IsXBinaryDigit(char? ch) => IsBinaryDigit(ch) || ch == '_' || ch == '\'';

  /// <summary>
  /// Tests if a character is a token separator
  /// </summary>
  private static bool IsTokenSeparator(char? ch) => ch is ' ' or '\t' or '\r' or '\n';

  /// <summary>
  /// Tests if a character is restricted in a string
  /// </summary>
  private static bool IsRestrictedInString(char? ch) => ch is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029';
}

/**
 * This enum indicates the current lexer phase
 */
internal enum LexerPhase
{
  // Start getting a token
  Start = 0,

  // Collecting whitespace
  InWhiteSpace,

  // Waiting for "\n" after "\r"
  PotentialNewLine,

  // Waiting for Option or Number decision
  OptionOrNumber,

  // Waiting for a Vatiable or a hexadecimal number
  VariableOrHexaDecimal,

  // Waiting for an argument tail
  ArgumentTail,

  // Variable related phases
  Variable,
  VariableTail,

  // String-related parsing phases
  IdTail,
  String,
  StringBackSlash,
  StringHexa1,
  StringHexa2,
  StringTail,

  OptionTail,
  PathTail,

  Decimal,
  DecimalTail,
  Hexadecimal,
  HexaDecimalTail,
  Binary,
  BinaryTail,
}
