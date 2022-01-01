using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record DeleteStatement(string TableName, IPredicate? Predicate) : IDeleteStatement
{
}
