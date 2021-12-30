namespace Skyla.Engine.Interfaces;

public interface IRecordId
{
    int BlockNumber { get; }
    int Slot { get; }
}
