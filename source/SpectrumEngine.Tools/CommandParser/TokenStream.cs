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
  // --- Already fetched tokens
  private List<Token> _ahead = new();

  // --- Prefetched character (from the next token)
  private string? _prefetched;

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

  public InputStream Input { get; init; }
  
  // /**
      //  * Gets the specified part of the source code
      //  * @param start Start position
      //  * @param end End position
      //  */
      // getSourceSpan(start: number, end: number): string {
      //   return this.input.getSourceSpan(start, end);
      // }
      //
      // /**
      //  * Fethches the next token without advancing to its position
      //  * @param ws If true, retrieve whitespaces too
      //  */
      // peek(ws = false): Token {
      //   return this.ahead(0, ws);
      // }
      //
      // /**
      //  *
      //  * @param n Number of token positions to read ahead
      //  * @param ws If true, retrieve whitespaces too
      //  */
      // ahead(n = 1, ws = false): Token {
      //   if (n > 16) {
      //     throw new Error("Cannot look ahead more than 16 tokens");
      //   }
      //
      //   // --- Prefetch missing tokens
      //   while (this._ahead.length <= n) {
      //     const token = this.fetch();
      //     if (isEof(token)) {
      //       return token;
      //     }
      //     if (ws || (!ws && !isWs(token))) {
      //       this._ahead.push(token);
      //     }
      //   }
      //   return this._ahead[n];
      // }
      //
      // /**
      //  * Fethces the nex token and advances the stream position
      //  * @param ws If true, retrieve whitespaces too
      //  */
      // get(ws = false): Token {
      //   if (this._ahead.length > 0) {
      //     const token = this._ahead.shift();
      //     if (!token) {
      //       throw new Error("Token expected");
      //     }
      //     return token;
      //   }
      //   while (true) {
      //     const token = this.fetch();
      //     if (isEof(token) || ws || (!ws && !isWs(token))) {
      //       return token;
      //     }
      //   }
      // }
      //
      // /**
      //  * Fetches the next token from the input stream
      //  */
      // private fetch(): Token {
      //   const lexer = this;
      //   const input = this.input;
      //   const startPos = this._prefetchedPos || input.position;
      //   const line = input.line;
      //   const startColumn = this._prefetchedColumn || input.column;
      //   let text = "";
      //   let tokenType = TokenType.Eof;
      //   let lastEndPos = input.position;
      //   let lastEndColumn = input.column;
      //   let ch: string | null = null;
      //
      //   let phase: LexerPhase = LexerPhase.Start;
      //   while (true) {
      //     // --- Get the next character
      //     ch = fetchNextChar();
      //
      //     // --- In case of EOF, return the current token data
      //     if (ch === null) {
      //       return makeToken();
      //     }
      //
      //     // --- Set the intial token type to unknown for the other characters
      //     if (tokenType === TokenType.Eof) {
      //       tokenType = TokenType.Unknown;
      //     }
      //
      //     // --- Follow the lexer state machine
      //     switch (phase) {
      //       // ====================================================================
      //       // Process the first character
      //       case LexerPhase.Start:
      //         switch (ch) {
      //           // --- Go on with whitespaces
      //           case " ":
      //           case "\t":
      //             phase = LexerPhase.InWhiteSpace;
      //             tokenType = TokenType.Ws;
      //             break;
      //
      //           // --- New line
      //           case "\n":
      //             return completeToken(TokenType.NewLine);
      //
      //           // --- Potential new line
      //           case "\r":
      //             phase = LexerPhase.PotentialNewLine;
      //             tokenType = TokenType.NewLine;
      //             break;
      //
      //           case '"':
      //             phase = LexerPhase.String;
      //             break;
      //
      //           case "-":
      //             phase = LexerPhase.OptionOrNumber;
      //             break;
      //
      //           case "$":
      //             phase = LexerPhase.VariableOrHexaDecimal;
      //             break;
      //
      //           case "%":
      //             phase = LexerPhase.Binary;
      //             break;
      //
      //           default:
      //             if (isIdStart(ch)) {
      //               phase = LexerPhase.IdTail;
      //               tokenType = TokenType.Identifier;
      //             } else if (isDecimalDigit(ch)) {
      //               phase = LexerPhase.Decimal;
      //               tokenType = TokenType.DecimalLiteral;
      //             } else if (isPathStart(ch)) {
      //               phase = LexerPhase.PathTail;
      //               tokenType = TokenType.Path;
      //             }
      //             break;
      //         }
      //         break;
      //
      //       // ====================================================================
      //       // Process whitespaces, comments, and new line
      //
      //       // --- Looking for the end of whitespace
      //       case LexerPhase.InWhiteSpace:
      //         if (ch !== " " && ch !== "\t") {
      //           return makeToken();
      //         }
      //         break;
      //
      //       // --- We already received a "\r", so this is a new line
      //       case LexerPhase.PotentialNewLine:
      //         if (ch === "\n") {
      //           return completeToken(TokenType.NewLine);
      //         }
      //         return makeToken();
      //
      //       // ====================================================================
      //       // Identifier and keyword like tokens
      //
      //       // --- Wait for the completion of an identifier
      //       case LexerPhase.IdTail:
      //         if (isIdContinuation(ch)) {
      //           break;
      //         }
      //         if (isTokenSeparator(ch)) {
      //           return makeToken();
      //         }
      //         if (isPathContinuation(ch)) {
      //           phase = LexerPhase.PathTail;
      //           tokenType = TokenType.Path;
      //         } else {
      //           phase = LexerPhase.ArgumentTail;
      //           tokenType = TokenType.Argument;
      //         }
      //         break;
      //
      //       case LexerPhase.PathTail:
      //         if (isPathContinuation(ch)) {
      //           break;
      //         }
      //         if (isTokenSeparator(ch)) {
      //           return makeToken();
      //         }
      //         phase = LexerPhase.ArgumentTail;
      //         tokenType = TokenType.Argument;
      //         break;
      //
      //       // ====================================================================
      //       // Variables
      //
      //       case LexerPhase.VariableOrHexaDecimal:
      //         if (ch === "{") {
      //           phase = LexerPhase.Variable;
      //           break;
      //         } else if (isHexadecimalDigit(ch)) {
      //           phase = LexerPhase.HexaDecimalTail;
      //           tokenType = TokenType.HexadecimalLiteral;
      //         } else {
      //           phase = LexerPhase.ArgumentTail;
      //           tokenType = TokenType.Argument;
      //         }
      //         break;
      //
      //       // We already parsed "${"
      //       case LexerPhase.Variable:
      //         if (isIdStart(ch)) {
      //           phase = LexerPhase.VariableTail;
      //         } else {
      //           return completeToken();
      //         }
      //         break;
      //
      //       // We identified the start character of a variable, and wait for continuation
      //       case LexerPhase.VariableTail:
      //         if (isIdContinuation(ch)) {
      //           break;
      //         }
      //         return ch === "}"
      //           ? completeToken(TokenType.Variable)
      //           : completeToken(TokenType.Unknown);
      //
      //       // ====================================================================
      //       // --- Options
      //
      //       case LexerPhase.OptionOrNumber:
      //         if (ch === "$") {
      //           phase = LexerPhase.Hexadecimal;
      //         } else if (ch === "%") {
      //           phase = LexerPhase.Binary;
      //         } else if (isDecimalDigit(ch)) {
      //           phase = LexerPhase.Decimal;
      //           tokenType = TokenType.DecimalLiteral;
      //         } else if (isIdStart(ch)) {
      //           phase = LexerPhase.OptionTail;
      //           tokenType = TokenType.Option;
      //         } else {
      //           tokenType = TokenType.Argument;
      //           phase = LexerPhase.ArgumentTail;
      //         }
      //         break;
      //
      //       case LexerPhase.OptionTail:
      //         if (isIdContinuation(ch)) {
      //           break;
      //         }
      //         if (isTokenSeparator(ch)) {
      //           return makeToken();
      //         }
      //         tokenType = TokenType.Argument;
      //         phase = LexerPhase.ArgumentTail;
      //         break;
      //
      //       case LexerPhase.ArgumentTail:
      //         if (isTokenSeparator(ch)) {
      //           return makeToken();
      //         }
      //         break;
      //
      //       // ====================================================================
      //       // --- Literals
      //
      //       // String data
      //       case LexerPhase.String:
      //         if (ch === '"') {
      //           return completeToken(TokenType.String);
      //         } else if (isRestrictedInString(ch)) {
      //           return completeToken(TokenType.Unknown);
      //         } else if (ch === "\\") {
      //           phase = LexerPhase.StringBackSlash;
      //           tokenType = TokenType.Unknown;
      //         }
      //         break;
      //
      //       // Start of string character escape
      //       case LexerPhase.StringBackSlash:
      //         switch (ch) {
      //           case "b":
      //           case "f":
      //           case "n":
      //           case "r":
      //           case "t":
      //           case "v":
      //           case "0":
      //           case "'":
      //           case '"':
      //           case "\\":
      //             phase = LexerPhase.String;
      //             break;
      //           default:
      //             if (ch === "x") {
      //               phase = LexerPhase.StringHexa1;
      //             } else {
      //               phase = LexerPhase.String;
      //             }
      //         }
      //         break;
      //
      //       // First hexadecimal digit of string character escape
      //       case LexerPhase.StringHexa1:
      //         if (isHexadecimalDigit(ch)) {
      //           phase = LexerPhase.StringHexa2;
      //         } else {
      //           return completeToken(TokenType.Unknown);
      //         }
      //         break;
      //
      //       // Second hexadecimal digit of character escape
      //       case LexerPhase.StringHexa2:
      //         if (isHexadecimalDigit(ch)) {
      //           phase = LexerPhase.String;
      //         } else {
      //           return completeToken(TokenType.Unknown);
      //         }
      //         break;
      //
      //       // The first character after "$"
      //       case LexerPhase.Hexadecimal:
      //         if (isHexadecimalDigit(ch)) {
      //           phase = LexerPhase.HexaDecimalTail;
      //           tokenType = TokenType.HexadecimalLiteral;
      //         } else {
      //           tokenType = TokenType.Argument;
      //           phase = LexerPhase.ArgumentTail;
      //         }
      //         break;
      //
      //       case LexerPhase.HexaDecimalTail:
      //         if (isXHexadecimalDigit(ch)) {
      //           break;
      //         }
      //         if (isTokenSeparator(ch)) {
      //           return makeToken();
      //         }
      //         tokenType = TokenType.Argument;
      //         phase = LexerPhase.ArgumentTail;
      //         break;
      //
      //       // The first character after "%"
      //       case LexerPhase.Binary:
      //         if (isBinaryDigit(ch)) {
      //           phase = LexerPhase.BinaryTail;
      //           tokenType = TokenType.BinaryLiteral;
      //         } else {
      //           tokenType = TokenType.Argument;
      //           phase = LexerPhase.ArgumentTail;
      //         }
      //         break;
      //
      //       case LexerPhase.BinaryTail:
      //         if (isXBinaryDigit(ch)) {
      //           break;
      //         }
      //         if (isTokenSeparator(ch)) {
      //           return makeToken();
      //         }
      //         tokenType = TokenType.Argument;
      //         phase = LexerPhase.ArgumentTail;
      //         break;
      //
      //       // The first decimal lieterl character
      //       case LexerPhase.Decimal:
      //         if (isDecimalDigit(ch)) {
      //           phase = LexerPhase.DecimalTail;
      //           tokenType = TokenType.DecimalLiteral;
      //         } else if (isTokenSeparator(ch)) {
      //           return makeToken();
      //         } else {
      //           tokenType = TokenType.Argument;
      //           phase = LexerPhase.ArgumentTail;
      //         }
      //         break;
      //
      //       case LexerPhase.DecimalTail:
      //         if (isXDecimalDigit(ch)) {
      //           break;
      //         }
      //         if (isTokenSeparator(ch)) {
      //           return makeToken();
      //         }
      //         tokenType = TokenType.Argument;
      //         phase = LexerPhase.ArgumentTail;
      //         break;
      //
      //       // ====================================================================
      //       // --- We cannot continue
      //       default:
      //         return makeToken();
      //     }
      //
      //     // --- Append the char to the current text
      //     appendTokenChar();
      //
      //     // --- Go on with parsing the next character
      //   }
      //
      //   /**
      //    * Appends the last character to the token, and manages positions
      //    */
      //   function appendTokenChar(): void {
      //     text += ch;
      //     lexer._prefetched = null;
      //     lexer._prefetchedPos = null;
      //     lexer._prefetchedColumn = null;
      //     lastEndPos = input.position;
      //     lastEndColumn = input.position;
      //   }
      //
      //   /**
      //    * Fetches the next character from the input stream
      //    */
      //   function fetchNextChar(): string | null {
      //     let ch: string;
      //     if (!lexer._prefetched) {
      //       lexer._prefetchedPos = input.position;
      //       lexer._prefetchedColumn = input.column;
      //       lexer._prefetched = input.get();
      //     }
      //     return lexer._prefetched;
      //   }
      //
      //   /**
      //    * Packs the specified type of token to send back
      //    * @param type
      //    */
      //   function makeToken(): Token {
      //     return {
      //       text,
      //       type: tokenType,
      //       location: {
      //         startPos,
      //         endPos: lastEndPos,
      //         line,
      //         startColumn,
      //         endColumn: lastEndColumn,
      //       },
      //     };
      //   }
      //
      //   /**
      //    * Add the last character to the token and return it
      //    */
      //   function completeToken(suggestedType?: TokenType): Token {
      //     appendTokenChar();
      //
      //     // --- Send back the token
      //     if (suggestedType !== undefined) {
      //       tokenType = suggestedType;
      //   }
      //   return makeToken();
      // }
  // }

    
    
    
    
    

    /// <summary>
    /// Tests if a token is EOF
    /// </summary>
    private static bool IsEof(Token t) => t.Type == TokenType.Eof;

    /// <summary>
    /// Tests if a token is whitespace
    /// </summary>
    private static bool IsWs(Token t) => t.Type <= TokenType.Ws;

    /// <summary>
    /// Tests if a character is a letter
    /// </summary>
    private static bool IsLetterOrDigit(char ch) 
        => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9';

    /// <summary>
    /// Tests if a character can be the start of an identifier
    /// </summary>
    private static bool IsIdStart(char ch) => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';

    /// <summary>
    /// Tests if a character can be the continuation of an identifier
    /// </summary>
    private static bool IsIdContinuation(char ch) =>
    (
        IsIdStart(ch) ||
        IsDecimalDigit(ch) ||
        ch is '-' or '$' or '.' or '!' or ':' or '#'
    );

    /// <summary>
    /// Tests if a character can be the start of a path
    /// </summary>
    private static bool IsPathStart(char ch) 
        => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '/' or '\\' or '.' or '*' or '?';

    /// <summary>
    /// Tests if a character can be the continuation of a path
    /// </summary>
    private static bool IsPathContinuation(char ch) => IsPathStart(ch) || ch == ':';

    /// <summary>
    /// Tests if a character is a decimal digit
    /// </summary>
    private static bool IsDecimalDigit(char ch) => ch is >= '0' and <= '9';

    /// <summary>
    /// Tests if a character is an extended decimal digit
    /// </summary>
    private static bool IsXDecimalDigit(char ch) => IsDecimalDigit(ch) || ch is '_' or '\'';

    /// <summary>
    /// Tests if a character is a hexadecimal digit
    /// </summary>
    private static bool IsHexadecimalDigit(char ch)
        => ch is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';

    /// <summary>
    /// Tests if a character is a hexadecimal digit
    /// </summary>
    private static bool IsXHexadecimalDigit(char ch) => IsHexadecimalDigit(ch) || ch is '_' or '\'';

    /// <summary>
    /// Tests if a character is a binary digit
    /// </summary>
    private static bool IsBinaryDigit(char ch) => ch is '0' or '1';

    /// <summary>
    /// Tests if a character is an extended binary digit
    /// </summary>
    private static bool IsXBinaryDigit(char ch) => IsBinaryDigit(ch) || ch == '_' || ch == '\'';

    /// <summary>
    /// Tests if a character is a token separator
    /// </summary>
    private static bool IsTokenSeparator(char ch) => ch is ' ' or '\t' or '\r' or '\n';

    /// <summary>
    /// Tests if a character is restricted in a string
    /// </summary>
    private static bool IsRestrictedInString(char ch) => ch is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029';
}

/**
 * This enum indicates the current lexer phase
 */
internal enum LexerPhase {
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
