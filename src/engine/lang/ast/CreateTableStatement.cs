using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public record CreateTableStatement(string TableName, TypeDefinition[] Types)
{
}

public record TypeDefinition(string Identifier, DefineType Type, int Length)
{

}
