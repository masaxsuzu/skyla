using Skyla.Engine.Interfaces;
using Skyla.Engine.Language.Ast;
namespace Skyla.Engine.Language;

public class Parser
{
    public Predicate ParsePredicate(string sql)
    {
        var lexer = new Lexer(sql);
        var tokens = new TokenList(lexer.Tokenize(), 0);

        var p = Predicate(tokens, out TokenList rest);

        if (!rest.IsEndOfStream())
        {
            throw new Exceptions.EngineException($"{rest.Peek(0).Literal} is not end of sql");
        }
        return p;
    }

    public QueryStatement ParseQuery(string sql)
    {
        var lexer = new Lexer(sql);
        var tokens = new TokenList(lexer.Tokenize(), 0);

        var q = Query(tokens, out TokenList rest);

        if (!rest.IsEndOfStream())
        {
            throw new Exceptions.EngineException($"{rest.Peek(0).Literal} is not enf of sql");
        }
        return q;
    }

    public InsertCommand ParseInsert(string sql)
    {
        var lexer = new Lexer(sql);
        var tokens = new TokenList(lexer.Tokenize(), 0);

        var i = Insert(tokens, out TokenList rest);

        if (!rest.IsEndOfStream())
        {
            throw new Exceptions.EngineException($"{rest.Peek(0).Literal} is not enf of sql");
        }
        return i;
    }

    public DeleteCommand ParseDelete(string sql)
    {
        var lexer = new Lexer(sql);
        var tokens = new TokenList(lexer.Tokenize(), 0);

        var d = Delete(tokens, out TokenList rest);

        if (!rest.IsEndOfStream())
        {
            throw new Exceptions.EngineException($"{rest.Peek(0).Literal} is not enf of sql");
        }
        return d;
    }

    public ModifyCommand ParseModify(string sql)
    {
        var lexer = new Lexer(sql);
        var tokens = new TokenList(lexer.Tokenize(), 0);

        var m = Modify(tokens, out TokenList rest);

        if (!rest.IsEndOfStream())
        {
            throw new Exceptions.EngineException($"{rest.Peek(0).Literal} is not enf of sql");
        }
        return m;
    }

    public CreateTableStatement ParseCreateTable(string sql)
    {
        var lexer = new Lexer(sql);
        var tokens = new TokenList(lexer.Tokenize(), 0);

        var t = CreateTable(tokens, out TokenList rest);

        if (!rest.IsEndOfStream())
        {
            throw new Exceptions.EngineException($"{rest.Peek(0).Literal} is not enf of sql");
        }
        return t;
    }

    public CreateViewStatement ParseCreateView(string sql)
    {
        var lexer = new Lexer(sql);
        var tokens = new TokenList(lexer.Tokenize(), 0);

        var t = CreateView(tokens, out TokenList rest);

        if (!rest.IsEndOfStream())
        {
            throw new Exceptions.EngineException($"{rest.Peek(0).Literal} is not enf of sql");
        }
        return t;
    }

    public CreateIndexStatement ParseCreateIndex(string sql)
    {
        var lexer = new Lexer(sql);
        var tokens = new TokenList(lexer.Tokenize(), 0);

        var i = CreateIndex(tokens, out TokenList rest);

        if (!rest.IsEndOfStream())
        {
            throw new Exceptions.EngineException($"{rest.Peek(0).Literal} is not enf of sql");
        }
        return i;
    }

    private CreateTableStatement CreateTable(TokenList token, out TokenList rest)
    {
        token = token.Expect("create");
        token = token.Expect("table");

        var tableName = Field(token, out token).Identifier;
        token = token.Expect("(");

        var defs = new List<TypeDefinition>();
        defs.Add(TypeDefinition(token, out token));
        while (token.Peek(0).IsReserved(","))
        {
            token = token.Consume(1);
            defs.Add(TypeDefinition(token, out token));
        }
        token = token.Expect(")");

        rest = token;
        return new CreateTableStatement(tableName, defs.ToArray());
    }

    private TypeDefinition TypeDefinition(TokenList token, out TokenList rest)
    {
        var id = Field(token, out token);
        if (token.Peek(0).IsReserved("int"))
        {
            token = token.Consume(1);
            rest = token;
            return new TypeDefinition(id.Identifier, DefineType.integer, 1);
        }

        token = token.Expect("varchar");
        token = token.Expect("(");

        token.Must(TokenType.integerConstant);
        var n = int.Parse(token.Peek(0).Literal);
        token = token.Consume(1);
        token = token.Expect(")");
        rest = token;
        return new TypeDefinition(id.Identifier, DefineType.varchar, n);
    }

    private CreateViewStatement CreateView(TokenList token, out TokenList rest)
    {
        token = token.Expect("create");
        token = token.Expect("view");

        var viewName = Field(token, out token).Identifier;
        token = token.Expect("as");

        var q = Query(token, out token);

        rest = token;

        return new CreateViewStatement(viewName, q.ColumnNames, q.TableNames, q.Predicate);
    }
    private CreateIndexStatement CreateIndex(TokenList token, out TokenList rest)
    {
        token = token.Expect("create");
        token = token.Expect("index");

        var indexName = Field(token, out token).Identifier;
        token = token.Expect("on");
        var tableName = Field(token, out token).Identifier;
        token = token.Expect("(");
        var fieldName = Field(token, out token).Identifier;
        token = token.Expect(")");

        rest = token;
        return new CreateIndexStatement(indexName, tableName, fieldName);
    }

    private InsertCommand Insert(TokenList token, out TokenList rest)
    {
        token = token.Expect("insert");
        token = token.Expect("into");

        var tableName = Field(token, out token).Identifier;
        token = token.Expect("(");
        var ids = FieldList(token, out token);
        token = token.Expect(")");

        token = token.Expect("values");

        token = token.Expect("(");
        var values = ConstantList(token, out token);
        token = token.Expect(")");

        rest = token;
        return new InsertCommand(tableName, ids, values);
    }

    private DeleteCommand Delete(TokenList token, out TokenList rest)
    {
        token = token.Expect("delete");
        token = token.Expect("from");

        var tableName = Field(token, out token).Identifier;
        if (token.Peek(0).IsReserved("where"))
        {
            token = token.Consume(1);
            var p = Predicate(token, out token);
            rest = token;
            return new DeleteCommand(tableName, p);
        }
        else
        {
            rest = token;
            return new DeleteCommand(tableName, null);
        }
    }

    private ModifyCommand Modify(TokenList token, out TokenList rest)
    {
        token = token.Expect("update");

        var tableName = Field(token, out token).Identifier;

        token = token.Expect("set");

        var field = Field(token, out token).Identifier;

        token = token.Expect("=");

        var expr = Expression(token, out token);

        if (token.Peek(0).IsReserved("where"))
        {
            token = token.Consume(1);
            var p = Predicate(token, out token);
            rest = token;
            return new ModifyCommand(tableName, field, expr, p);
        }
        else
        {
            rest = token;
            return new ModifyCommand(tableName, field, expr, null);
        }
    }
    private IConstant[] ConstantList(TokenList token, out TokenList rest)
    {
        var values = new List<IConstant>();
        values.Add(Constant(token, out token));
        while (token.Peek(0).IsReserved(","))
        {
            token = token.Consume(1);
            values.Add(Constant(token, out token));
        }
        rest = token;
        return values.ToArray();
    }
    private QueryStatement Query(TokenList token, out TokenList rest)
    {
        token = token.Expect("select");

        var selects = FieldList(token, out token);

        token = token.Expect("from");

        var tables = FieldList(token, out token);

        if (token.Peek(0).IsReserved("where"))
        {
            token = token.Consume(1);
            var p = Predicate(token, out token);
            rest = token;
            return new QueryStatement(selects, tables, p);
        }
        else
        {
            rest = token;
            return new QueryStatement(selects, tables, null);
        }
    }

    private string[] FieldList(TokenList token, out TokenList rest)
    {
        var fields = new List<Field>();
        fields.Add(Field(token, out token));
        while (token.Peek(0).IsReserved(","))
        {
            token = token.Consume(1);
            fields.Add(Field(token, out token));
        }
        rest = token;
        return fields.Select(f => f.Identifier).ToArray();
    }

    private Predicate Predicate(TokenList token, out TokenList rest)
    {
        var term = Term(token, out token);
        var p = new Predicate(new Term[] { term });

        if (token.Peek(0).IsReserved("and"))
        {
            var predicate = Predicate(token.Consume(1), out token);
            p = p.With(predicate);
        }
        rest = token;
        return p;
    }

    private Term Term(TokenList token, out TokenList rest)
    {
        var left = Expression(token, out token);
        token = token.Expect("=");
        var right = Expression(token, out token);

        rest = token;
        return new Term(left, right);
    }

    private IExpression Expression(TokenList token, out TokenList rest)
    {
        if (token.Peek(0).Type == TokenType.identifier)
        {
            var f = Field(token, out rest);
            return new FieldExpression(f);
        }
        var c = Constant(token, out rest);
        return new ConstantExpression(c);
    }

    private Field Field(TokenList token, out TokenList rest)
    {
        token.Must(TokenType.identifier);
        var id = token.Peek(0);
        rest = token.Consume(1);
        return new Field(id.Literal);
    }

    private IConstant Constant(TokenList token, out TokenList rest)
    {
        var c = token.Peek(0);
        if (c.Type == TokenType.integerConstant)
        {
            rest = token.Consume(1);
            return new IntegerConstant(int.Parse(c.Literal));
        }
        if (c.Type == TokenType.stringConstant)
        {
            rest = token.Consume(1);
            return new StringConstant(c.Literal);
        }
        throw new Engine.Exceptions.EngineException($"{c.Literal} is not constant");
    }
}
