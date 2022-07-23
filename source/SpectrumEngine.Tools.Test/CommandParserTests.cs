using Shouldly;
using SpectrumEngine.Tools.CommandParser;
using Xunit;

namespace SpectrumEngine.Tools.Test;

public class CommandParserTests
{
    [Fact]
    public void StringLiteral1()
    {
        TestToken("\"\"", TokenType.String);
        TestToken("\"a\"", TokenType.String);
        TestToken("\"abcd\"", TokenType.String);
        TestToken("\"\\b\"", TokenType.String);
        TestToken("\"\\f\"", TokenType.String);
        TestToken("\"\\n\"", TokenType.String);
        TestToken("\"\\r\"", TokenType.String);
        TestToken("\"\\t\"", TokenType.String);
        TestToken("\"\\v\"", TokenType.String);
        TestToken("\"\\0\"", TokenType.String);
        TestToken("\"\\'\"", TokenType.String);
        TestToken("\"\\\"\"", TokenType.String);
        TestToken("\"\\\\\"", TokenType.String);

        TestToken("\"\\x01\"", TokenType.String);
        TestToken("\"\\xa1\"", TokenType.String);
        TestToken("\"\\xBC\"", TokenType.String);
    }

    [Fact]
    public void StringLiteral2()
    {
        TestToken("\"\\\"", TokenType.Unknown);
        TestToken("\"\\x0\"", TokenType.Unknown);
        TestToken("\"a'", TokenType.Unknown);
    }
    
    [Fact]
    public void Variable1()
    {
        TestToken("${a}", TokenType.Variable);
        TestToken("${z}", TokenType.Variable);
        TestToken("${A}", TokenType.Variable);
        TestToken("${Z}", TokenType.Variable);
        TestToken("${_}", TokenType.Variable);
        TestToken("${a0}", TokenType.Variable);
        TestToken("${a9}", TokenType.Variable);
        TestToken("${a-}", TokenType.Variable);
        TestToken("${a$}", TokenType.Variable);
        TestToken("${a.}", TokenType.Variable);
        TestToken("${a!}", TokenType.Variable);
        TestToken("${a:}", TokenType.Variable);
        TestToken("${a#}", TokenType.Variable);
        TestToken("${project-root}", TokenType.Variable);
    }

    [Fact]
    public void Variable2()
    {
        TestToken("${0}", TokenType.Unknown, "${0");
        TestToken("${$}", TokenType.Unknown, "${$");
        TestToken("${a*}", TokenType.Unknown, "${a*");
        TestToken("${project-root", TokenType.Unknown, null);
    }
    
    [Fact]
    public void HexaDecimalLiteral1()
    {
        TestToken("$abcd", TokenType.HexadecimalLiteral);
        TestToken("$12ac", TokenType.HexadecimalLiteral);
        TestToken("$12acd", TokenType.HexadecimalLiteral);
        TestToken("$0", TokenType.HexadecimalLiteral);
        TestToken("$a", TokenType.HexadecimalLiteral);
        TestToken("$0a", TokenType.HexadecimalLiteral);
        TestToken("$a0", TokenType.HexadecimalLiteral);
        TestToken("$0a1", TokenType.HexadecimalLiteral);
        TestToken("$a0b", TokenType.HexadecimalLiteral);
        TestToken("$a_bcd", TokenType.HexadecimalLiteral);
        TestToken("$ab_cd", TokenType.HexadecimalLiteral);
        TestToken("$ab'cd'", TokenType.HexadecimalLiteral);
    }
    
    [Fact]
    public void HexaDecimalLiteral2()
    {
        TestToken("-$abcd", TokenType.HexadecimalLiteral);
        TestToken("-$12ac", TokenType.HexadecimalLiteral);
        TestToken("-$12acd", TokenType.HexadecimalLiteral);
        TestToken("-$0", TokenType.HexadecimalLiteral);
        TestToken("-$a", TokenType.HexadecimalLiteral);
        TestToken("-$0a", TokenType.HexadecimalLiteral);
        TestToken("-$a0", TokenType.HexadecimalLiteral);
        TestToken("-$0a1", TokenType.HexadecimalLiteral);
        TestToken("-$a0b", TokenType.HexadecimalLiteral);
        TestToken("-$a_bcd", TokenType.HexadecimalLiteral);
        TestToken("-$ab_cd", TokenType.HexadecimalLiteral);
        TestToken("-$ab'cd'", TokenType.HexadecimalLiteral);
    }
    
    [Fact]
    public void HexaDecimalLiteral3()
    {
        TestToken("$abq", TokenType.Argument);
        TestToken("-$abq", TokenType.Argument);
    }
    
    [Fact]
    public void BinaryLiteral1()
    {
        TestToken("%0", TokenType.BinaryLiteral);
        TestToken("%1", TokenType.BinaryLiteral);
        TestToken("%01110001", TokenType.BinaryLiteral);
        TestToken("%0111_0001", TokenType.BinaryLiteral);
        TestToken("%1111_0001", TokenType.BinaryLiteral);
        TestToken("%01_11_0001", TokenType.BinaryLiteral);
        TestToken("%11_11_0001_", TokenType.BinaryLiteral);
        TestToken("%0111'0001", TokenType.BinaryLiteral);
        TestToken("%1111'0001", TokenType.BinaryLiteral);
        TestToken("%01'11'0001", TokenType.BinaryLiteral);
        TestToken("%11'11_0001'", TokenType.BinaryLiteral);
        TestToken("-%0", TokenType.BinaryLiteral);
        TestToken("-%1", TokenType.BinaryLiteral);
        TestToken("-%01110001", TokenType.BinaryLiteral);
        TestToken("-%0111_0001", TokenType.BinaryLiteral);
        TestToken("-%1111_0001", TokenType.BinaryLiteral);
        TestToken("-%01_11_0001", TokenType.BinaryLiteral);
        TestToken("-%11_11_0001_", TokenType.BinaryLiteral);
        TestToken("-%0111'0001", TokenType.BinaryLiteral);
        TestToken("-%1111'0001", TokenType.BinaryLiteral);
        TestToken("-%01'11'0001", TokenType.BinaryLiteral);
        TestToken("-%11'11_0001'", TokenType.BinaryLiteral);
    }
    
    [Fact]
    public void BinaryLiteral2()
    {
        TestToken("%_", TokenType.Argument);
        TestToken("%_111_0001", TokenType.Argument);
        TestToken("%1112", TokenType.Argument);
        TestToken("%11q111", TokenType.Argument);
    }

    [Fact]
    public void DecimalLiteral1()
    {
        TestToken("1", TokenType.DecimalLiteral);
        TestToken("0", TokenType.DecimalLiteral);
        TestToken("9", TokenType.DecimalLiteral);
        TestToken("8765432", TokenType.DecimalLiteral);
        TestToken("765432", TokenType.DecimalLiteral);
        TestToken("65432", TokenType.DecimalLiteral);
        TestToken("5432", TokenType.DecimalLiteral);
        TestToken("432", TokenType.DecimalLiteral);
        TestToken("32", TokenType.DecimalLiteral);
        TestToken("765_432", TokenType.DecimalLiteral);
        TestToken("65_432", TokenType.DecimalLiteral);
        TestToken("54_32_", TokenType.DecimalLiteral);
        TestToken("765'432", TokenType.DecimalLiteral);
        TestToken("65'432", TokenType.DecimalLiteral);
        TestToken("54'32'", TokenType.DecimalLiteral);
        TestToken("54_32'", TokenType.DecimalLiteral);
        TestToken("54'32_", TokenType.DecimalLiteral);
        TestToken("-1", TokenType.DecimalLiteral);
        TestToken("-0", TokenType.DecimalLiteral);
        TestToken("-9", TokenType.DecimalLiteral);
        TestToken("-8765432", TokenType.DecimalLiteral);
        TestToken("-765432", TokenType.DecimalLiteral);
        TestToken("-65432", TokenType.DecimalLiteral);
        TestToken("-5432", TokenType.DecimalLiteral);
        TestToken("-432", TokenType.DecimalLiteral);
        TestToken("-32", TokenType.DecimalLiteral);
        TestToken("-765_432", TokenType.DecimalLiteral);
        TestToken("-65_432", TokenType.DecimalLiteral);
        TestToken("-54_32_", TokenType.DecimalLiteral);
        TestToken("-765'432", TokenType.DecimalLiteral);
        TestToken("-65'432", TokenType.DecimalLiteral);
        TestToken("-54'32'", TokenType.DecimalLiteral);
        TestToken("-54_32'", TokenType.DecimalLiteral);
        TestToken("-54'32_", TokenType.DecimalLiteral);
    }

    [Fact]
    public void DecimalLiteral2()
    {
        TestToken("765_4q32", TokenType.Argument);
        TestToken("-765_4q32", TokenType.Argument);
        TestToken("-_765_4*32", TokenType.Argument);
    }

    [Fact]
    public void Option1()
    {
        TestToken("-a", TokenType.Option);
        TestToken("-z", TokenType.Option);
        TestToken("-A", TokenType.Option);
        TestToken("-Z", TokenType.Option);
        TestToken("-Z", TokenType.Option);
        TestToken("-a0", TokenType.Option);
        TestToken("-a9-", TokenType.Option);
        TestToken("-a-", TokenType.Option);
        TestToken("-a$", TokenType.Option);
        TestToken("-a.", TokenType.Option);
        TestToken("-a!", TokenType.Option);
        TestToken("-a:", TokenType.Option);
        TestToken("-a#", TokenType.Option);
        TestToken("-project-root", TokenType.Option);
    }

    [Fact]
    public void Option2()
    {
        TestToken("-a*", TokenType.Argument);
        TestToken("-z\\", TokenType.Argument);
    }

    [Fact]
    public void Identifier1()
    {
        TestToken("a", TokenType.Identifier);
        TestToken("z", TokenType.Identifier);
        TestToken("A", TokenType.Identifier);
        TestToken("Z", TokenType.Identifier);
        TestToken("Z", TokenType.Identifier);
        TestToken("a0", TokenType.Identifier);
        TestToken("a9-", TokenType.Identifier);
        TestToken("a-", TokenType.Identifier);
        TestToken("a$", TokenType.Identifier);
        TestToken("a.", TokenType.Identifier);
        TestToken("a!", TokenType.Identifier);
        TestToken("a:", TokenType.Identifier);
        TestToken("a#", TokenType.Identifier);
        TestToken("project-root", TokenType.Identifier);
    }

    [Fact]
    public void Identifier2()
    {
        TestToken("a-{", TokenType.Argument);
        TestToken("a*", TokenType.Path);
        TestToken("z\\", TokenType.Path);
    }

    private void TestToken(string tokenStr, TokenType type, string? errorToken = null)
    {
        // --- Test for the single token
        var resultStr = errorToken ?? tokenStr;
        var ts = new TokenStream(new InputStream(tokenStr));

        var token = ts.Get();
        token.Type.ShouldBe(type);
        token.Text.ShouldBe(resultStr);
        token.Location.StartPos.ShouldBe(0);
        token.Location.EndPos.ShouldBe(resultStr.Length);
        token.Location.Line.ShouldBe(1);
        token.Location.StartColumn.ShouldBe(0);
        token.Location.EndColumn.ShouldBe(resultStr.Length);

        // --- Test for token with subsequent token
        if (errorToken != null)
        {
            ts = new TokenStream(new InputStream(tokenStr + " /"));
            token = ts.Get();
            token.Type.ShouldBe(type);
            token.Text.ShouldBe(resultStr);
            token.Location.StartPos.ShouldBe(0);
            token.Location.EndPos.ShouldBe(resultStr.Length);
            token.Location.Line.ShouldBe(1);
            token.Location.StartColumn.ShouldBe(0);
            token.Location.EndColumn.ShouldBe(resultStr.Length);
        }

        // --- Test for token with leading whitespace
        ts = new TokenStream(new InputStream("  " + tokenStr));
        token = ts.Get();
        token.Type.ShouldBe(type);
        token.Text.ShouldBe(resultStr);
        token.Location.StartPos.ShouldBe(2);
        token.Location.EndPos.ShouldBe(resultStr.Length + 2);
        token.Location.Line.ShouldBe(1);
        token.Location.StartColumn.ShouldBe(2);
        token.Location.EndColumn.ShouldBe(resultStr.Length + 2);
    }
}