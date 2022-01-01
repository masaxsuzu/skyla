namespace Skyla.Engine.Language;

public class TokenList
{
    private readonly Token[] _tokens;
    private readonly int _at;
    public TokenList(Token[] tokens, int at)
    {
        _tokens = tokens;
        _at = at;
    }

    public TokenList Consume(int next)
    {
        return new TokenList(_tokens, _at + next);
    }

    public void Must(TokenType type)
    {
        if (Peek(0).Type != type)
        {
            throw new Engine.Exceptions.EngineException($"not token type {type}");
        }
    }
    public TokenList Expect(string literal)
    {
        if (Peek(0).IsReserved(literal))
        {
            return Consume(1);
        }
        else
        {
            throw new Engine.Exceptions.EngineException($"not {literal}");
        }
    }

    public Token Peek(int next)
    {
        if (_tokens.Length <= _at + next)
        {
            return _tokens[_tokens.Length - 1];
        }
        return _tokens[_at + next];
    }

    public bool IsEndOfStream()
    {
        return Peek(0).IsReserved("");
    }
}
