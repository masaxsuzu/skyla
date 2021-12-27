namespace Skyla.Engine.Interfaces;

public interface IBufferManager
{
    IBuffer Pin(IBlockId block);
    void Unpin(IBuffer buffer);
    int Available { get; }
    void FlushAll(int transactionNumber);
}
