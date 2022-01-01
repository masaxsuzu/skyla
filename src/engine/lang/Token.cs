namespace Skyla.Engine.Language;

public enum TokenType
{
    eos,
    symbol,
    keyword,
    integerConstant,
    stringConstant,
    identifier,
    illegal,
}

public record struct Token(TokenType Type, string Literal, int Position)
{
    public override string ToString()
    {
        return $"{{\"Type\":\"{Type}\", \"Literal\":\"{Literal}\", \"Position\":\"{Position}\"}}";
    }
}
