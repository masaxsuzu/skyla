using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Recovery.LogRecords;

public class StartLogRecord : ILogRecord
{
    private readonly int _transactionNumber;
    public StartLogRecord(IPage page)
    {
        int pos = 4;
        _transactionNumber = page.Get(pos, new IntegerType());
    }
    public StartLogRecord(int transactionNumber)
    {
        _transactionNumber = transactionNumber;
    }
    public int Operation => 1;

    public int TransactionNumber => _transactionNumber;

    public void Undo(ITransaction transaction)
    {
    }

    public override string ToString()
    {
        return $"<START {_transactionNumber}>";
    }

    public int WriteToLog(ILogManager logManager)
    {
        var record = new byte[2 * 4];
        var p = new Page(record);
        p.Set(0, new IntegerType(), 1);
        p.Set(4, new IntegerType(), _transactionNumber);
        return logManager.Append(record);
    }
}
