using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record CreateViewStatement(string ViewName, string[] ColumnNames, string[] TableNames, IPredicate? Predicate)
{
}
