using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record CreateIndexStatement(string IndexName, string TableName, string ColumnName) : ICreateIndexStatement
{
}
