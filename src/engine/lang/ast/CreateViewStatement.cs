using System.Text;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record CreateViewStatement(string ViewName, string ViewDefinition, string[] ColumnNames, string[] TableNames, IPredicate Predicate) : ICreateViewStatement
{
}
