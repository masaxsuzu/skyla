using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Concurrency;

public class LockTable : ILockTable
{
    private static readonly int MaximumWaitTimeMilliSecond = 10;
    private readonly Dictionary<IBlockId, int> _locks;
    public LockTable()
    {
        _locks = new Dictionary<IBlockId, int>();
    }
    public void TryExclusiveLock(IBlockId block)
    {
        lock (this)
        {
            try
            {
                long timeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
                while (HasAnySharedLocks(block) && !WaitingTooLong(timeStamp))
                {
                    Monitor.Wait(this, MaximumWaitTimeMilliSecond);
                }
                if (HasAnySharedLocks(block))
                {
                    throw new EngineException($"cannot lock block {block} with an exclusive lock, due to another shared lock");
                }
                _locks.Put(block, -1);
            }
            catch (ThreadInterruptedException ex1)
            {
                throw new EngineException($"cannot lock block {block} with an exclusive lock, due to another shared lock", ex1);
            }
            catch (SynchronizationLockException ex2)
            {
                throw new EngineException($"cannot lock block {block} with an exclusive lock, due to another shared lock", ex2);
            }
        }
    }

    public void TrySharedLock(IBlockId block)
    {
        lock (this)
        {
            try
            {
                long timeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
                while (HasAnExclusiveLock(block) && !WaitingTooLong(timeStamp))
                {
                    Monitor.Wait(this, MaximumWaitTimeMilliSecond);
                }
                if (HasAnExclusiveLock(block))
                {
                    throw new EngineException($"cannot lock block {block} with a shared lock, due to another exclusive lock");
                }
                int value = LockedValue(block);
                _locks.Put(block, value + 1);

            }
            catch (ThreadInterruptedException ex1)
            {
                throw new EngineException($"cannot lock block {block} with a shared lock, due to another exclusive lock", ex1);
            }
            catch (SynchronizationLockException ex2)
            {
                throw new EngineException($"cannot lock block {block} with a shared lock, due to another exclusive lock", ex2);
            }
        }
    }

    public void Unlock(IBlockId block)
    {
        lock (this)
        {
            int lockedValue = LockedValue(block);
            if (lockedValue > 1)
            {
                _locks.Put(block, lockedValue - 1);
            }
            else
            {
                _locks.Remove(block);
                Monitor.PulseAll(this);
            }
        }
    }

    private bool HasAnExclusiveLock(IBlockId block)
    {
        return LockedValue(block) < 0;
    }

    private bool HasAnySharedLocks(IBlockId block)
    {
        return LockedValue(block) > 1;
    }

    private int LockedValue(IBlockId block)
    {
        return (_locks.ContainsKey(block)) ? _locks[block] : 0;
    }

    private bool WaitingTooLong(long from)
    {
        return from + MaximumWaitTimeMilliSecond < ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
    }
}
