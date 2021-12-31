namespace Skyla.Engine.Interfaces;

public interface IStatInfo
{
    int AccessedBlocks { get; }
    int Records { get; }
    int DistinctValues(string fieldName);
}
