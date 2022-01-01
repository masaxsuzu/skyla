using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record QueryStatement(string[] ColumnNames, string[] TableNames, IPredicate? Predicate) : IQueryStatement
{
}
