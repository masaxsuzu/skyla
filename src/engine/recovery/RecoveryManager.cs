using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Recovery;

public class RecoveryManager : IRecoveryManager
{
    private readonly ILogManager _logManager;
    private readonly IBufferManager _bufferManager;
    private readonly ITransaction _transaction;
    private readonly int _transactionNumber;
    public RecoveryManager(
        ILogManager logManager,
        IBufferManager bufferManager,
        ITransaction transaction,
        int transactionNumber)
    {
        _logManager = logManager;
        _bufferManager = bufferManager;
        _transaction = transaction;
        _transactionNumber = transactionNumber;
        LogRecords.StartLogRecord.WriteToLog(logManager, transactionNumber);
    }
    public void Commit()
    {
        _bufferManager.FlushAll(_transactionNumber);
        int lsNumber = LogRecords.CommitLogRecord.WriteToLog(_logManager, _transactionNumber);
        _logManager.Flush(lsNumber);
    }

    public void Recover()
    {
        DoRecover();
        _bufferManager.FlushAll(_transactionNumber);
        int lsNumber = LogRecords.CheckpointLogRecord.WriteToLog(_logManager);
        _logManager.Flush(lsNumber);
    }

    public void Rollback()
    {
        DoRollback();
        _bufferManager.FlushAll(_transactionNumber);
        int lsNumber = LogRecords.RollbackLogRecord.WriteToLog(_logManager, _transactionNumber);
        _logManager.Flush(lsNumber);
    }

    public int SetInt(IBuffer buffer, int offset, int value)
    {
        int old = buffer.Contents.GetInt(offset);
        var block = buffer.Block;
        if (block == null)
        {
            throw new EngineException("unreachable");
        }
        return LogRecords.SetIntLogRecord.WriteToLog(_logManager, _transactionNumber, block, offset, old);
    }

    public int SetString(IBuffer buffer, int offset, string value)
    {
        string old = buffer.Contents.GetString(offset);
        var block = buffer.Block;
        if (block == null)
        {
            throw new EngineException("unreachable");
        }
        return LogRecords.SetStringLogRecord.WriteToLog(_logManager, _transactionNumber, block, offset, old);
    }

    private void DoRollback()
    {
        foreach (byte[] log in _logManager.AsEnumerable())
        {
            var record = CreateFrom(log);
            if (record.TransactionNumber == _transactionNumber)
            {
                if (record.Operation == 1)
                {
                    return;
                }
                record.Undo(_transaction);
            }
        }
    }

    private void DoRecover()
    {
        var finishedTransactionNumbers = new List<int>();
        foreach (byte[] log in _logManager.AsEnumerable())
        {
            var record = CreateFrom(log);
            if (record.TransactionNumber == _transactionNumber)
            {
                if (record.Operation == 0)
                {
                    return;
                }
                if (record.Operation == 2 || record.Operation == 3)
                {
                    finishedTransactionNumbers.Add(record.TransactionNumber);
                }
                else if (!finishedTransactionNumbers.Contains(record.TransactionNumber))
                {
                    record.Undo(_transaction);
                }
            }
        }
    }

    private static ILogRecord CreateFrom(byte[] log)
    {
        var page = new Page(log);
        var type = page.GetInt(0);
        switch (type)
        {
            case 0: // checkpoint
                return new LogRecords.CheckpointLogRecord();
            case 1: // start
                return new LogRecords.StartLogRecord(page);
            case 2: // commit
                return new LogRecords.CommitLogRecord(page);
            case 3: // rollback
                return new LogRecords.RollbackLogRecord(page);
            case 4: // int
                return new LogRecords.SetIntLogRecord(page);
            case 5: // string
                return new LogRecords.SetStringLogRecord(page);
            default:
                throw new EngineException($"unsupported log record type {type}");
        }
    }
}
