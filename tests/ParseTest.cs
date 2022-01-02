using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
using Skyla.Engine.Language.Ast;
using Skyla.Engine.Scans;

using Xunit;
namespace Skyla.Tests;

public class ParseTest
{
    [Fact]
    public void PredicateTest()
    {
        string src = "x = 1 and y = 'y' and z = w";

        IPredicate expected = new Predicate(new Term[]{
            new Term(
                new FieldExpression(new Field("x")),
                new ConstantExpression(new IntegerConstant(1))
            ),
            new Term(
                new FieldExpression(new Field("y")),
                new ConstantExpression(new StringConstant("y"))
            ),
            new Term(
                new FieldExpression(new Field("z")),
                new FieldExpression(new Field("w"))
            ),
        });
        var parser = new Parser();
        var got = parser.ParsePredicate(src);
        Assert.Equal(expected, got);
    }

    [Fact]
    public void QueryTest1()
    {
        string src = "select x,y,z from a,b where x = 1 and y = 'y' and z = w";

        IPredicate p = new Predicate(new Term[]{
            new Term(
                new FieldExpression(new Field("x")),
                new ConstantExpression(new IntegerConstant(1))
            ),
            new Term(
                new FieldExpression(new Field("y")),
                new ConstantExpression(new StringConstant("y"))
            ),
            new Term(
                new FieldExpression(new Field("z")),
                new FieldExpression(new Field("w"))
            ),
        });
        var expected = new QueryStatement(new string[] { "x", "y", "z" }, new string[] { "a", "b" }, p);
        var parser = new Parser();
        var got = parser.ParseQuery(src);
        Assert.Equal(expected.ColumnNames, got.ColumnNames);
        Assert.Equal(expected.TableNames, got.TableNames);
        Assert.Equal(expected.Predicate, got.Predicate);
    }
    [Fact]
    public void QueryTest2()
    {
        string src = "select x from a";

        var expected = new QueryStatement(new string[] { "x" }, new string[] { "a" }, new Predicate());
        var parser = new Parser();
        var got = parser.ParseQuery(src);
        Assert.Equal(expected.ColumnNames, got.ColumnNames);
        Assert.Equal(expected.TableNames, got.TableNames);
        Assert.Equal(expected.Predicate, got.Predicate);
    }

    [Fact]
    public void CreateTableTest1()
    {
        string src = "create table a (x int, y varchar(2), z varchar(10))";

        var expected = new CreateTableStatement(
            "a",
            new TypeDefinition[]{
                new TypeDefinition("x", DefineType.integer, 1),
                new TypeDefinition("y", DefineType.varchar, 2),
                new TypeDefinition("z", DefineType.varchar, 10),
            }
        );
        var parser = new Parser();
        var got = parser.ParseCreateTable(src);

        Assert.Equal(expected.TableName, got.TableName);
        Assert.Equal(expected.Types.Length, got.Types.Length);
        for (int i = 0; i < expected.Types.Length; i++)
        {
            Assert.Equal(expected.Types[i], got.Types[i]);
        }
    }

    [Fact]
    public void CreateViewTest1()
    {
        string src = "create view v1 as select x,y,z from a,b where x = 1 and y = 'y' and z = w";

        IPredicate p = new Predicate(new Term[]{
            new Term(
                new FieldExpression(new Field("x")),
                new ConstantExpression(new IntegerConstant(1))
            ),
            new Term(
                new FieldExpression(new Field("y")),
                new ConstantExpression(new StringConstant("y"))
            ),
            new Term(
                new FieldExpression(new Field("z")),
                new FieldExpression(new Field("w"))
            ),
        });

        var expected = new CreateViewStatement(
            "v1",
            "select x,y,z from a,b where x = 1 and y = 'y' and z = w",
            new string[] { "x", "y", "z" },
            new string[] { "a", "b" },
            p
        );
        var parser = new Parser();
        var got = parser.ParseCreateView(src);

        Assert.Equal(expected.ViewName, got.ViewName);
        Assert.Equal(expected.ViewDefinition, got.ViewDefinition);
        Assert.Equal(expected.ColumnNames, got.ColumnNames);
        Assert.Equal(expected.TableNames, got.TableNames);
        Assert.Equal(expected.Predicate, got.Predicate);
    }
    [Fact]
    public void CreateIndexTest1()
    {
        string src = "create index i1 on a (x)";


        var expected = new CreateIndexStatement("i1", "a", "x");
        var parser = new Parser();
        var got = parser.ParseCreateIndex(src);

        Assert.Equal(expected.IndexName, got.IndexName);
        Assert.Equal(expected.TableName, got.TableName);
        Assert.Equal(expected.ColumnName, got.ColumnName);
    }

    [Fact]
    public void InsertCommandTest1()
    {
        string src = "insert into a (x,y,z) values (1, 'a', 'b')";

        var expected = new InsertStatement(
            "a",
            new string[] { "x", "y", "z" },
            new IConstant[] {
                new IntegerConstant(1),
                new StringConstant("a"),
                new StringConstant("b")
            }
        );
        var parser = new Parser();
        var got = parser.ParseInsert(src);

        Assert.Equal(expected.TableName, got.TableName);
        Assert.Equal(expected.ColumnNames, got.ColumnNames);
        Assert.Equal(expected.Values.Length, got.Values.Length);
        for (int i = 0; i < expected.Values.Length; i++)
        {
            Assert.Equal(expected.Values[0], got.Values[0]);
        }
    }

    [Fact]
    public void DeleteCommandTest1()
    {
        string src = "delete from a where x = 1 and y = 'y'";

        IPredicate p = new Predicate(new Term[]{
            new Term(
                new FieldExpression(new Field("x")),
                new ConstantExpression(new IntegerConstant(1))
            ),
            new Term(
                new FieldExpression(new Field("y")),
                new ConstantExpression(new StringConstant("y"))
            ),
        });

        var expected = new DeleteStatement(
            "a",
            p
        );
        var parser = new Parser();
        var got = parser.ParseDelete(src);

        Assert.Equal(expected.TableName, got.TableName);
        Assert.Equal(expected.Predicate, got.Predicate);
    }

    [Fact]
    public void ModifyCommandTest1()
    {
        string src = "update a set x = 1 where y = 'y'";

        IPredicate p = new Predicate(new Term[]{
            new Term(
                new FieldExpression(new Field("y")),
                new ConstantExpression(new StringConstant("y"))
            ),
        });

        var expected = new ModifyStatement(
            "a",
            "x",
            new ConstantExpression(new IntegerConstant(1)),
            p
        );
        var parser = new Parser();
        var got = parser.ParseModify(src);

        Assert.Equal(expected.TableName, got.TableName);
        Assert.Equal(expected.ColumnName, got.ColumnName);
        Assert.Equal(expected.Expression, got.Expression);
        Assert.Equal(expected.Predicate, got.Predicate);
    }
    [Theory]
    [InlineData(StatementType.table, "create table a (x int, y varchar(2), z varchar(10))")]
    [InlineData(StatementType.view, "create view v1 as select x,y,z from a,b where x = 1 and y = 'y' and z = w")]
    [InlineData(StatementType.index, "create index i1 on a (x)")]
    [InlineData(StatementType.query, "select x from y where a = 1")]
    [InlineData(StatementType.insert, "insert into a (x,y,z) values (1, 'a', 'b')")]
    [InlineData(StatementType.update, "update a set x = 1 where y = 'y'")]
    [InlineData(StatementType.delete, "delete from a where x = 1 and y = 'y'")]
    public void ParseTest1(StatementType type, string src)
    {
        var parser = new Parser();
        var (got, s) = parser.Parse(src);

        Assert.Equal(type, got);
        Assert.NotNull(s);
    }
}

