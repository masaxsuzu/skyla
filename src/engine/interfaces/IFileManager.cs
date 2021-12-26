namespace Skyla.Engine.Interfaces;

public interface IFileManager
{
    void Read(IBlockId block, IPage page);
    void Write(IBlockId block, IPage page);
    IBlockId Append(string fileName);
    bool IsNew { get; }
    int Length(string fileName);
    int BlockSize { get; }
}
