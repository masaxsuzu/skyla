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
        return $"{{ \"FileName\": \"{FileName}\", \"Number\": {Number} }}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        else if (obj is BlockId other)
        {
            return FileName == other.FileName && Number == other.Number;
        }
        else
        {
            return false;
        }
    }
    public override int GetHashCode()
    {
        return FileName.GetHashCode() + Number.GetHashCode();
    }
}
