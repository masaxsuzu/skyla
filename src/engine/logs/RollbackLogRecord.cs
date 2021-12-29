using Skyla.Engine.Buffers;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Logs;

public class RollbackLogRecord : ILogRecord
{
    private readonly int _transactionNumber;
    public RollbackLogRecord(IPage page)
    {
        int pos = 4;
        _transactionNumber = page.Get(pos, new IntegerType());
    }
    public RollbackLogRecord(int transactionNumber)
    {
        _transactionNumber = transactionNumber;
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

    public int WriteToLog(ILogManager logManager)
    {
        var record = new byte[2 * 4];
        var p = new Page(record);
        p.Set(0, new IntegerType(), 3);
        p.Set(4, new IntegerType(), _transactionNumber);
        return logManager.Append(record);
    }
}
