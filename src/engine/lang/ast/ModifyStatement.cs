using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record ModifyStatement(string TableName, string ColumnName, IExpression Expression, IPredicate? Predicate) : IModifyStatement
{
}
