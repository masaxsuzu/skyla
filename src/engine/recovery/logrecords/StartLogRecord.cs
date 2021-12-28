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
        _transactionNumber = page.GetInt(pos);
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

    public static int WriteToLog(ILogManager logManager, int transactionNumber)
    {
        var record = new byte[2 * 4];
        var p = new Page(record);
        p.SetInt(0, 1);
        p.SetInt(4, transactionNumber);
        return logManager.Append(record);
    }
}
