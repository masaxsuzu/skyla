using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record ModifyCommand(string TableName, string ColumnName, IExpression Expression, IPredicate? Predicate)
{
}
