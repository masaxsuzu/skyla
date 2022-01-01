using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record InsertStatement(string TableName, string[] ColumnNames, IConstant[] Values) : IInsertStatement
{
}
