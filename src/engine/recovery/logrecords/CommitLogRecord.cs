using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Recovery.LogRecords;

public class CommitLogRecord : ILogRecord
{
    private readonly int _transactionNumber;
    public CommitLogRecord(IPage page)
    {
        int pos = 4;
        _transactionNumber = page.Get(pos, new IntegerType());
    }
    public int Operation => 2;

    public int TransactionNumber => _transactionNumber;

    public void Undo(ITransaction transaction)
    {
    }

    public override string ToString()
    {
        return $"<COMMIT {_transactionNumber}>";
    }

    public static int WriteToLog(ILogManager logManager, int transactionNumber)
    {
        var record = new byte[2 * 4];
        var p = new Page(record);
        p.Set(0, new IntegerType(), 2);
        p.Set(4, new IntegerType(), transactionNumber);
        return logManager.Append(record);
    }
}
