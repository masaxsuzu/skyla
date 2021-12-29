using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Recovery.LogRecords;

public class RollbackLogRecord : ILogRecord
{
    private readonly int _transactionNumber;
    public RollbackLogRecord(IPage page)
    {
        int pos = 4;
        _transactionNumber = page.Get(pos, new IntegerType());
    }
    public int Operation => 3;

    public int TransactionNumber => _transactionNumber;

    public void Undo(ITransaction transaction)
    {
    }

    public override string ToString()
    {
        return $"<Rollback {_transactionNumber}>";
    }

    public static int WriteToLog(ILogManager logManager, int transactionNumber)
    {
        var record = new byte[2 * 4];
        var p = new Page(record);
        p.Set(0, new IntegerType(), 3);
        p.Set(4, new IntegerType(), transactionNumber);
        return logManager.Append(record);
    }
}
