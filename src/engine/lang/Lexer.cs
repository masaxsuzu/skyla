using System.Globalization;
namespace Skyla.Engine.Language;

public class Lexer
{
    private readonly string[] _src;
    private readonly string[] _keywords = {
        "select",
        "from",
        "where",
        "and",
        "insert",
        "into",
        "values",
        "delete",
        "update",
        "set",
        "create",
        "table",
        "int",
        "varchar",
        "view",
        "as",
        "index",
        "on",
    };
    private Token[] _token = new Token[] { };
    private int _currentPosition;
    private int _nextPosition;
    private string _text;
    public Lexer(string src)
    {
        _src = Texts(System.Globalization.StringInfo.GetTextElementEnumerator(src)).ToArray();

        _currentPosition = 0;
        _nextPosition = 0;
        _text = "";
        Advance();
    }

    public Token[] Tokenize()
    {
        if (_token.Length > 0)
        {
            return _token;
        }

        var tokens = new List<Token>();

        while (true)
        {
            var token = Next();
            tokens.Add(token);
            if (token.Type == TokenType.eos)
            {
                break;
            }
        }

        return tokens.ToArray();
    }

    private Token Next()
    {
        SkipWhitespaces();

        var token = new Token(TokenType.illegal, _text, _currentPosition);
        var pos = _currentPosition;

        switch (_text)
        {
            case "":
                token = new Token(TokenType.eos, "", pos);
                break;
            case "=":
            case ",":
            case "(":
            case ")":
            case ";":
                token = new Token(TokenType.symbol, _text, pos);
                break;
            case "'":
                var strPos = pos + 1;
                token = new Token(TokenType.stringConstant, ReadStringValue(), strPos);
                break;
            default:
                if (IsLetter(_text))
                {
                    var letterPos = pos;
                    var (type, literal) = ReadkeywordOrIdentifier();
                    token = new Token(type, literal, letterPos);
                    return token;
                }
                else if (IsDigit(_text))
                {
                    var numPos = pos;
                    token = new Token(TokenType.integerConstant, ReadNumber(), numPos);
                    return token;
                }
                throw new Engine.Exceptions.EngineException($"illegal token {_text}");
        }

        Advance();
        return token;
    }

    private string ReadStringValue()
    {
        var from = _currentPosition + 1;
        while (true)
        {
            Advance();
            if (_text == "'")
            {
                break;
            }
            else if (_text == "")
            {
                throw new Engine.Exceptions.EngineException("string must be closed");
            }
        }
        return string.Join("", _src[from.._currentPosition]);
    }

    private string ReadNumber()
    {
        var from = _currentPosition;
        while (IsDigit(_text))
        {
            Advance();
        }
        return string.Join("", _src[from.._currentPosition]);
    }

    private (TokenType, string) ReadkeywordOrIdentifier()
    {
        var from = _currentPosition;
        while (IsLetter(_text) || IsDigit(_text))
        {
            Advance();
        }
        var literal = string.Join("", _src[from.._currentPosition]);
        var type = _keywords.Contains(literal.ToLower()) ? TokenType.keyword : TokenType.identifier;
        return (type, literal);
    }

    private bool IsLetter(string text)
    {
        var x = text.ToCharArray();
        if (x.Length != 1) return false;
        var ch = x[0];
        return
            ('a' <= ch && ch <= 'z') ||
            ('A' <= ch && ch <= 'Z') ||
             ch == '_';
    }

    private bool IsDigit(string text)
    {
        var x = text.ToCharArray();
        if (x.Length != 1) return false;
        var ch = x[0];
        return '0' <= ch && ch <= '9';
    }

    private void SkipWhitespaces()
    {
        while (_text == " " || _text == "\t" || _text == "\n" || _text == "\r")
        {
            Advance();
        }
    }

    private void Advance()
    {
        _text = _src.Length <= _nextPosition ? "" : _src[_nextPosition];
        _currentPosition = _nextPosition;
        _nextPosition++;
    }

    static IEnumerable<string> Texts(TextElementEnumerator src)
    {
        while (src.MoveNext())
        {
            string? c = src.Current as string;
            if (c != null)
            {
                yield return c.ToString();
            }
        }
    }

}
