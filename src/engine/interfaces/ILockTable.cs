namespace Skyla.Engine.Interfaces;

public interface ILockTable
{
    void TrySharedLock(IBlockId block);
    void TryExclusiveLock(IBlockId block);
    void Unlock(IBlockId block);
}
