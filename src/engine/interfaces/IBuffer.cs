namespace Skyla.Engine.Interfaces;

public interface IBuffer
{
    IPage Contents { get; }
    IBlockId? Block { get; }
    bool IsPinned { get; }
    int ModifyingTransaction { get; }
    void SetModified(int transactionNumber, int lsNumber);
    void Flush();
    void Pin();
    void Unpin();
}
