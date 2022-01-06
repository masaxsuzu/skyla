using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Indexes;

public class BTreeLeaf
{
    private readonly ITransaction _transaction;
    private readonly IBlockId _block;
    private readonly ILayout _layout;
    private readonly IConstant _key;
    private BTreePage _contents;
    private readonly string _fileName;
    private int _slot;
    public BTreeLeaf(ITransaction transaction, IBlockId block, ILayout layout, IConstant key)
    {
        _transaction = transaction;
        _block = block;
        _layout = layout;
        _key = key;
        _contents = new BTreePage(_transaction, block, layout);
        _slot = _contents.FindSlotBefore(key);
        _fileName = block.FileName;
    }

    public void Close()
    {
        _contents.Close();
    }

    public bool Next()
    {
        _slot++;
        if (_contents.NumberOfRecords <= _slot)
        {
            return TryOverflow();
        }
        else if (_contents.GetValue(_slot).Equals(_key))
        {
            return true;
        }
        else
        {
            return TryOverflow();
        }
    }
    public IRecordId Record => _contents.Record(_slot);
    public void Delete(IRecordId record)
    {
        while (Next())
        {
            if (Record == record)
            {
                _contents.Delete(_slot);
                return;
            }
        }
    }

    public BTreeDirectoryEntry? Insert(IRecordId record)
    {
        if (0 <= _contents.GetMode() && 0 < _contents.GetValue(0).CompareTo(_key))
        {
            var firstValue = _contents.GetValue(0);
            var block = _contents.Split(0, _contents.GetMode());
            _slot = 0;
            _contents.SetMode(-1);
            _contents.InsertLeaf(_slot, _key, record);
            return new BTreeDirectoryEntry(firstValue, block.Number);
        }
        _slot++;
        _contents.InsertLeaf(_slot, _key, record);
        if (!_contents.IsFull)
        {
            return null;
        }
        var firstKey = _contents.GetValue(0);
        var lastKey = _contents.GetValue(_slot);
        if (lastKey.Equals(firstKey))
        {
            var block = _contents.Split(1, _contents.GetMode());
            _contents.SetMode(block.Number);
            return null;
        }
        else
        {
            int splitPosition = _contents.NumberOfRecords;
            var splitKey = _contents.GetValue(splitPosition);
            if (splitKey.Equals(firstKey))
            {
                while (_contents.GetValue(splitPosition).Equals(splitKey))
                {
                    splitPosition++;
                }
                splitKey = _contents.GetValue(splitPosition);
            }
            else
            {
                while (_contents.GetValue(splitPosition - 1).Equals(splitKey))
                {
                    splitPosition--;
                }
            }
            var block = _contents.Split(splitPosition, -1);
            return new BTreeDirectoryEntry(splitKey, block.Number);
        }
    }
    private bool TryOverflow()
    {
        var first = _contents.GetValue(0);
        var mode = _contents.GetMode();
        if (!_key.Equals(first) || mode < 0)
        {
            return false;
        }
        _contents.Close();
        var block = new BlockId(_fileName, mode);
        _contents = new BTreePage(_transaction, block, _layout);
        _slot = 0;
        return true;
    }
}
