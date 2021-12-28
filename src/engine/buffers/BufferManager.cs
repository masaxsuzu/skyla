using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Buffers;

public class BufferManager : IBufferManager
{
    private static readonly int MaximumWaitTimeMilliSecond = 10;
    private readonly Buffer[] _pool;
    private int _availablePoolNumber;
    public BufferManager(IFileManager file, ILogManager log, int poolSize)
    {
        _pool = new Buffer[poolSize];
        _availablePoolNumber = poolSize;
        for (int i = 0; i < poolSize; i++)
        {
            _pool[i] = new Buffer(file, log);
        }
    }

    public int Available => _availablePoolNumber;

    public void FlushAll(int transactionNumber)
    {
        lock (this)
        {
            foreach (var buffer in _pool)
            {
                if (buffer.ModifyingTransaction == transactionNumber)
                {
                    buffer.Flush();
                }
            }
        }
    }

    public IBuffer Pin(IBlockId block)
    {
        lock (this)
        {
            try
            {
                long timeStamp = System.DateTime.UtcNow.Millisecond;
                Buffer? buffer = TryToPin(block);
                while (buffer == null && !WaitingTooLong(timeStamp))
                {
                    Monitor.Wait(this, MaximumWaitTimeMilliSecond);
                    buffer = TryToPin(block);
                }
                if (buffer == null)
                {
                    throw new EngineException($"cannot pin block {block} due to unavailability of buffer pool");
                }
                return buffer;
            }
            catch (ThreadInterruptedException ex1)
            {
                HandleThreadingException(ex1, block);
            }
            catch (SynchronizationLockException ex2)
            {
                HandleThreadingException(ex2, block);
            }
        }
        throw new EngineException("unreachable");
    }

    private void HandleThreadingException(Exception ex, IBlockId block)
    {
        throw new EngineException($"cannot pin block {block} due to threading error", ex);
    }

    public void Unpin(IBuffer buffer)
    {
        lock (this)
        {
            buffer.Unpin();
            if (!buffer.IsPinned)
            {
                _availablePoolNumber++;
                Monitor.PulseAll(this);
            }
        }
    }

    private bool WaitingTooLong(long from)
    {
        return from + MaximumWaitTimeMilliSecond < System.DateTime.UtcNow.Millisecond;
    }

    private Buffer? TryToPin(IBlockId block)
    {
        Buffer? buffer = FindExistingBuffer(block);
        if (buffer == null)
        {
            buffer = ChooseUnpinnedBuffer();
            if (buffer == null)
            {
                return null;
            }
            buffer.AssignToBlock(block);
        }
        if (!buffer.IsPinned)
        {
            _availablePoolNumber--;
        }
        buffer.Pin();
        return buffer;
    }

    private Buffer? FindExistingBuffer(IBlockId block)
    {
        foreach (var buffer in _pool)
        {
            IBlockId? b = buffer.Block;
            if (b != null && b.Equals(block))
            {
                return buffer;
            }
        }
        return null;
    }

    private Buffer? ChooseUnpinnedBuffer()
    {
        foreach (var buffer in _pool)
        {
            if (!buffer.IsPinned)
            {
                return buffer;
            }
        }
        return null;
    }

    internal Buffer[] GetInternalBufferPool => _pool;
}
