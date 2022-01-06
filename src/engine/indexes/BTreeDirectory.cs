using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Indexes;

public class BTreeDirectory
{
    private readonly ITransaction _transaction;
    private readonly ILayout _layout;
    private BTreePage _contents;
    private readonly string _fileName;
    public BTreeDirectory(ITransaction transaction, IBlockId block, ILayout layout)
    {
        _transaction = transaction;
        _layout = layout;
        _contents = new BTreePage(_transaction, block, _layout);
        _fileName = block.FileName;
    }

    public void Close()
    {
        _contents.Close();
    }
    public int Search(IConstant key)
    {
        var child = FindChildBlock(key);
        while (0 < _contents.GetMode())
        {
            _contents.Close();
            _contents = new BTreePage(_transaction, child, _layout);
            child = FindChildBlock(key);
        }
        return child.Number;
    }
    public void MakeNewRoot(BTreeDirectoryEntry dir)
    {
        var first = _contents.GetValue(0);
        int level = _contents.GetMode();
        var block = _contents.Split(0, level);
        var old = new BTreeDirectoryEntry(first, block.Number);
        InsertEntry(old);
        InsertEntry(dir);
        _contents.SetMode(level + 1);
    }
    public BTreeDirectoryEntry? Insert(BTreeDirectoryEntry dir)
    {
        if (_contents.GetMode() == 0)
        {
            return InsertEntry(dir);
        }
        var block = FindChildBlock(dir.Value);
        var child = new BTreeDirectory(_transaction, block, _layout);
        var entry = child.Insert(dir);
        return (entry != null) ? InsertEntry(entry) : null;
    }
    private BTreeDirectoryEntry? InsertEntry(BTreeDirectoryEntry dir)
    {
        int newSlot = 1 + _contents.FindSlotBefore(dir.Value);
        _contents.InsertDir(newSlot, dir.Value, dir.BlockNumber);
        if (!_contents.IsFull)
        {
            return null;
        }
        int level = _contents.GetMode();
        int splitPosition = _contents.NumberOfRecords / 2;
        var splitValue = _contents.GetValue(splitPosition);
        var block = _contents.Split(splitPosition, level);
        return new BTreeDirectoryEntry(splitValue, block.Number);

    }

    private IBlockId FindChildBlock(IConstant key)
    {
        int slot = _contents.FindSlotBefore(key);
        if (_contents.GetValue(slot + 1).Equals(key))
        {
            slot++;
        }
        int block = _contents.ChildNumber(slot);
        return new BlockId(_fileName, block);
    }
}
