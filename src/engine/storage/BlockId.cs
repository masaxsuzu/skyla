using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Storage;

public record BlockId(string FileName, int Number) : IBlockId
{
    public override string ToString()
    {
        return $"{{ \"FileName\": \"{FileName}\", \"Number\": {Number} }}";
    }
}
