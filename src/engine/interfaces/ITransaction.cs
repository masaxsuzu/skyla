namespace Skyla.Engine.Interfaces;

public interface ITransaction
{
    void Commit();
    void Rollback();
    void Recover();
    void Pin(IBlockId block);
    void UnPin(IBlockId block);
    int GetInt(IBlockId block, int offset);
    void SetInt(IBlockId block, int offset, int value, bool toLog);
    string GetString(IBlockId block, int offset);
    void SetString(IBlockId block, int offset, string value, bool toLog);
    int AvailableBuffers { get; }
    int Size(string fileName);
    IBlockId Append(string fileName);
    int BlockSize { get; }
}
