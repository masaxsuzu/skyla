using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Storage;

public class BlockId : IBlockId
{
    public BlockId(string fileName, int number)
    {
        FileName = fileName;
        Number = number;
    }
    public string FileName { get; }
    public int Number { get; }

    public override string ToString()
    {
        return $"{{ \"\": \"{FileName}\", \"\": {Number} }}";
    }
}
