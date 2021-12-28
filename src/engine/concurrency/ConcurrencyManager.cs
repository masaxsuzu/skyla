using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Concurrency;

public class ConcurrencyManager : IConcurrencyManager
{
    private static LockTable _lockTable = new LockTable();
    private Dictionary<IBlockId, string> _locks;
    public ConcurrencyManager()
    {
        _locks = new Dictionary<IBlockId, string>();
    }

    public void GetSharedLock(IBlockId block)
    {
        if (!_locks.ContainsKey(block))
        {
            _lockTable.TrySharedLock(block);
            _locks.Put(block, "S");
        }
    }
    public void GetExclusiveLock(IBlockId block)
    {
        if (!HasAnExclusiveLock(block))
        {
            GetSharedLock(block);
            _lockTable.TryExclusiveLock(block);
            _locks.Put(block, "X");
        }
    }

    private bool HasAnExclusiveLock(IBlockId block)
    {
        if (_locks.TryGetValue(block, out string? type))
        {
            return type == "X";
        }
        return false;
    }

    public void Release()
    {
        foreach (var lockedBlock in _locks.Keys)
        {
            _lockTable.Unlock(lockedBlock);
        }
        _locks.Clear();
    }
}
