using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record InsertCommand(string TableName, string[] ColumnNames, IConstant[] Values)
{
}
