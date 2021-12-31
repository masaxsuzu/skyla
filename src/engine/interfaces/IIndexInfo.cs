namespace Skyla.Engine.Interfaces;

public interface IIndexInfo
{
    IIndexable Open();
    int AccessedBlocks { get; }
    int Records { get; }
    int DistinctValues(string fieldName);
}
