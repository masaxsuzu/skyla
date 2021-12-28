namespace Skyla.Engine.Interfaces;

public interface IConcurrencyManager
{
    void GetSharedLock(IBlockId block);
    void GetExclusiveLock(IBlockId block);
    void Release();
}
