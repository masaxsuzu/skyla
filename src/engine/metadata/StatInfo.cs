using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Metadata;

public class StatInfo : IStatInfo
{
    public StatInfo(int blocks, int records)
    {
        AccessedBlocks = blocks;
        Records = records;
    }
    public int AccessedBlocks { get; }

    public int Records { get; }

    public int DistinctValues(string fieldName)
    {
        return 1 + (Records / 3); // This is wildly inaccurate;
    }
}
