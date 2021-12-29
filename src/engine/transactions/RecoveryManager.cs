using Skyla.Engine.Buffers;
using Skyla.Engine.Exceptions;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Logs;
namespace Skyla.Engine.Transactions;

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
        new StartLogRecord(transactionNumber).WriteToLog(logManager);
    }
    public void Commit()
    {
        _bufferManager.FlushAll(_transactionNumber);
        int lsNumber = new CommitLogRecord(_transactionNumber).WriteToLog(_logManager);
        _logManager.Flush(lsNumber);
    }

    public void Recover()
    {
        DoRecover();
        _bufferManager.FlushAll(_transactionNumber);
        int lsNumber = new CheckpointLogRecord().WriteToLog(_logManager);
        _logManager.Flush(lsNumber);
    }

    public void Rollback()
    {
        DoRollback();
        _bufferManager.FlushAll(_transactionNumber);
        int lsNumber = new RollbackLogRecord(_transactionNumber).WriteToLog(_logManager);
        _logManager.Flush(lsNumber);
    }

    public int SetInt(IBuffer buffer, int offset, int value)
    {
        int old = buffer.Contents.Get(offset, new IntegerType());
        var block = buffer.Block;
        if (block == null)
        {
            throw new EngineException("unreachable");
        }
        return new SetIntLogRecord(_transactionNumber, block, offset, old).WriteToLog(_logManager);
    }

    public int SetString(IBuffer buffer, int offset, string value)
    {
        string old = buffer.Contents.Get(offset, new StringType());
        var block = buffer.Block;
        if (block == null)
        {
            throw new EngineException("unreachable");
        }
        return new SetStringLogRecord(_transactionNumber, block, offset, old).WriteToLog(_logManager);
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
        var type = page.Get(0, new IntegerType());
        switch (type)
        {
            case 0: // checkpoint
                return new CheckpointLogRecord();
            case 1: // start
                return new StartLogRecord(page);
            case 2: // commit
                return new CommitLogRecord(page);
            case 3: // rollback
                return new RollbackLogRecord(page);
            case 4: // int
                return new SetIntLogRecord(page);
            case 5: // string
                return new SetStringLogRecord(page);
            default:
                throw new EngineException($"unsupported log record type {type}");
        }
    }
}
