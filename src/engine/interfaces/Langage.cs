namespace Skyla.Engine.Interfaces;

public enum DefineType
{
    integer,
    varchar,
}

public enum ConstantType
{
    integer,
    varchar,
}

public enum ExpressionType
{
    field,
    constant
}

public interface ICreateTableStatement : IStatement
{
    string TableName { get; }
    TypeDefinition[] Types { get; }
}

public record TypeDefinition(string Identifier, DefineType Type, int Length)
{

}

public interface ICreateViewStatement : IStatement
{
    string ViewName { get; }
    string[] ColumnNames { get; }
    string[] TableNames { get; }
    IPredicate? Predicate { get; }
}

public interface ICreateIndexStatement : IStatement
{
    string IndexName { get; }
    string TableName { get; }
    string ColumnName { get; }
}

public interface IQueryStatement : IStatement
{
    string[] ColumnNames { get; }
    string[] TableNames { get; }
    IPredicate? Predicate { get; }
}

public interface IInsertStatement : IStatement
{
    string TableName { get; }
    string[] ColumnNames { get; }
    IConstant[] Values { get; }
}

public interface IDeleteStatement : IStatement
{
    string TableName { get; }
    IPredicate? Predicate { get; }
}

public interface IModifyStatement : IStatement
{
    string TableName { get; }
    string ColumnName { get; }
    IExpression Expression { get; }
    IPredicate? Predicate { get; }
}
