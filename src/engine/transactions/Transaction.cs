using Skyla.Engine.Concurrency;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Transactions;

public class Transaction : ITransaction
{
    private static object _lockForNextTransactionNumber = new object();
    private static int _nextTransactionNumber = 0;
    private static readonly int _endOfFile = -1;
    private readonly IRecoveryManager _recoveryManager;
    private readonly IConcurrencyManager _concurrencyManager;
    private readonly IBufferManager _bufferManager;
    private readonly IFileManager _fileManager;
    private readonly int _transactionNumber;
    private readonly BufferList _buffers;
    public Transaction(IFileManager fileManager, ILogManager logManager, IBufferManager bufferManager)
    {
        _fileManager = fileManager;
        _bufferManager = bufferManager;
        _transactionNumber = IncrementNextTransactionNumber();
        _recoveryManager = new RecoveryManager(logManager, bufferManager, this, _transactionNumber);
        _concurrencyManager = new ConcurrencyManager();
        _buffers = new BufferList(bufferManager);
    }
    public int AvailableBuffers => _bufferManager.Available;

    public int BlockSize => _fileManager.BlockSize;

    public void Commit()
    {
        _recoveryManager.Commit();
        _concurrencyManager.Release();
        _buffers.UnpinAll();
    }
    public void Pin(IBlockId block)
    {
        _buffers.Pin(block);
    }

    public void UnPin(IBlockId block)
    {
        _buffers.Unpin(block);
    }

    public void Recover()
    {
        _bufferManager.FlushAll(_transactionNumber);
        _recoveryManager.Recover();
    }

    public void Rollback()
    {
        _recoveryManager.Rollback();
        _concurrencyManager.Release();
        _buffers.UnpinAll();
    }
    public int GetInt(IBlockId block, int offset)
    {
        _concurrencyManager.GetSharedLock(block);
        var buffer = _buffers.Get(block);
        return buffer.Contents.Get(offset, new IntegerType());
    }

    public string GetString(IBlockId block, int offset)
    {
        _concurrencyManager.GetSharedLock(block);
        var buffer = _buffers.Get(block);
        return buffer.Contents.Get(offset, new StringType());
    }


    public void SetInt(IBlockId block, int offset, int value, bool toLog)
    {
        _concurrencyManager.GetExclusiveLock(block);
        var buffer = _buffers.Get(block);
        int lsNumber = -1;
        if (toLog)
        {
            lsNumber = _recoveryManager.SetInt(buffer, offset, value);
        }
        var page = buffer.Contents;
        page.Set(offset, new IntegerType(), value);
        buffer.SetModified(_transactionNumber, lsNumber);
    }

    public void SetString(IBlockId block, int offset, string value, bool toLog)
    {
        _concurrencyManager.GetExclusiveLock(block);
        var buffer = _buffers.Get(block);
        int lsNumber = -1;
        if (toLog)
        {
            lsNumber = _recoveryManager.SetString(buffer, offset, value);
        }
        var page = buffer.Contents;
        page.Set(offset, new StringType(), value);
        buffer.SetModified(_transactionNumber, lsNumber);
    }

    public IBlockId Append(string fileName)
    {
        var dummyBlock = new BlockId(fileName, _endOfFile);
        _concurrencyManager.GetExclusiveLock(dummyBlock);
        return _fileManager.Append(fileName);
    }

    public int Size(string fileName)
    {
        var dummyBlock = new BlockId(fileName, _endOfFile);
        _concurrencyManager.GetSharedLock(dummyBlock);
        return _fileManager.Length(fileName);
    }

    public static int IncrementNextTransactionNumber()
    {
        lock (_lockForNextTransactionNumber)
        {
            _nextTransactionNumber++;
            return _nextTransactionNumber;
        }
    }
}
