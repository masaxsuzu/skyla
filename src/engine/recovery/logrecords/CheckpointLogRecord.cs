using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Recovery.LogRecords;

public class CheckpointLogRecord : ILogRecord
{
    public int Operation => 0;

    public int TransactionNumber => -1;

    public void Undo(ITransaction transaction)
    {
    }

    public override string ToString()
    {
        return "<CHECKPOINT>";
    }

    public int WriteToLog(ILogManager logManager)
    {
        var record = new byte[4];
        var p = new Page(record);
        p.Set(0, new IntegerType(), 0);
        return logManager.Append(record);
    }
}
