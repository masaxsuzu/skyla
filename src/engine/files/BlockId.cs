using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Files;

public record BlockId(string FileName, int Number) : IBlockId
{
    public override string ToString()
    {
        return $"{{ \"FileName\": \"{FileName}\", \"Number\": {Number} }}";
    }
}
