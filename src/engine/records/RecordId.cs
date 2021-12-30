using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Records;

public record RecordId(int BlockNumber, int Slot) : IRecordId
{
    public override string ToString()
    {
        return $"{{ \"BlockNumber\": \"{BlockNumber}\", \"Slot\": {Slot} }}";
    }
}
