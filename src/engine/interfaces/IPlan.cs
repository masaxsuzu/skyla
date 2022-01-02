namespace Skyla.Engine.Interfaces;

public interface IPlan
{
    IScan Open();
    int AccessedBlocks { get; }
    int Records { get; }
    int DistinctValues(string fieldName);
    ISchema Schema { get; }
}

public interface IWritablePlan : IPlan
{
    IUpdateScan Update();
}
