using Skyla.Engine.Language;
using Xunit;
namespace Skyla.Tests;

public class TokenizeTest
{
    [Fact]
    public void LexerText()
    {
        //            012345678901234567890123456789012345678901234 56789012345678901234567890
        string src = "select a,b1,b2 from x where a = 1 and b1 = '𩸽' and b2 = 'skyla on .NET'";
        Token[] expected = new Token[] {
            new Token(TokenType.keyword, "select", 0),
            new Token(TokenType.identifier, "a", 7),
            new Token(TokenType.symbol, ",", 8),
            new Token(TokenType.identifier, "b1", 9),
            new Token(TokenType.symbol, ",", 11),
            new Token(TokenType.identifier, "b2", 12),
            new Token(TokenType.keyword, "from", 15),
            new Token(TokenType.identifier, "x", 20),
            new Token(TokenType.keyword, "where", 22),
            new Token(TokenType.identifier, "a", 28),
            new Token(TokenType.symbol, "=", 30),
            new Token(TokenType.integerConstant, "1", 32),
            new Token(TokenType.keyword, "and", 34),
            new Token(TokenType.identifier, "b1", 38),
            new Token(TokenType.symbol, "=", 41),
            new Token(TokenType.stringConstant, "𩸽", 44),
            new Token(TokenType.keyword, "and", 47),
            new Token(TokenType.identifier, "b2", 51),
            new Token(TokenType.symbol, "=", 54),
            new Token(TokenType.stringConstant, "skyla on .NET", 57),
            new Token(TokenType.eos, "", 71),
        };

        var lexer = new Lexer(src);
        var got = lexer.Tokenize();

        Assert.Equal(expected.Length, got.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], got[i]);
        }
    }
}
