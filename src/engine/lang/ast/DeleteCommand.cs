using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record DeleteCommand(string TableName, IPredicate? Predicate)
{
}
