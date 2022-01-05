using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Indexes;

public class BTreePage
{
    private readonly ITransaction _transaction;
    private IBlockId? _block;
    private readonly ILayout _layout;
    public BTreePage(ITransaction transaction, IBlockId block, ILayout layout)
    {
        _transaction = transaction;
        _block = block;
        _layout = layout;
        _transaction.Pin(block);
    }
    public int FindSlotBefore(IConstant key)
    {
        int slot = 0;
        while (slot < NumberOfRecords && GetValue(slot).CompareTo(key) < 0)
        {
            slot++;
        }
        return slot - 1;
    }
    public void Close()
    {
        if (_block != null)
        {
            _transaction.UnPin(_block);
        }
        _block = null;
    }
    public bool IsFull => _transaction.BlockSize <= SlotPosition(NumberOfRecords + 1);

    public IBlockId Split(int splitPosition, int mode)
    {
        var newId = AppendNew(mode);
        var newPage = new BTreePage(_transaction, newId, _layout);
        TransferRecords(splitPosition, newPage);
        newPage.SetMode(mode);
        newPage.Close();
        return newId;
    }

    public IConstant GetValue(int slot)
    {
        return GetValue(slot, "value");
    }
#pragma warning disable CS8604
    public int GetMode() => _transaction.GetInt(_block, 0);
    public void SetMode(int mode) => _transaction.SetInt(_block, 0, mode, true);

    public IBlockId AppendNew(int mode)
    {
#pragma warning disable CS8602
        var block = _transaction.Append(_block.FileName);
        _transaction.Pin(block);
        Format(block, mode);
        return block;
    }

    public void Format(IBlockId block, int mode)
    {
        _transaction.SetInt(block, 0, mode, false);
        _transaction.SetInt(block, 4, 0, false);
        int sizeOfRecords = _layout.SlotSize;
        for (int pos = 2 * 4; pos + sizeOfRecords <= _transaction.BlockSize; pos += sizeOfRecords)
        {
            MakeDefaultRecord(block, pos);
        }
    }
    private void MakeDefaultRecord(IBlockId block, int pos)
    {
        foreach (var field in _layout.Schema.Fields)
        {
            int offset = _layout.Offset(field.Name);
            var type = _layout.Schema.GetType(field.Name).Type;
            if (type == 8)
            {
                _transaction.SetInt(block, pos + offset, 0, false);
            }
            if (type == 22)
            {
                _transaction.SetString(block, pos + offset, "", false);
            }
        }
    }

    internal int ChildNumber(int slot)
    {
        return GetInt(slot, "block");
    }
    internal void InsertDir(int slot, IConstant value, int blockNumber)
    {
        Insert(slot);
        SetValue(slot, "value", value);
        SetInt(slot, "block", blockNumber);
    }
    internal IRecordId Record(int slot) => new RecordId(GetInt(slot, "block"), GetInt(slot, "id"));
    internal void InsertLeaf(int slot, IConstant value, IRecordId record)
    {
        Insert(slot);
        SetValue(slot, "value", value);
        SetInt(slot, "block", record.BlockNumber);
        SetInt(slot, "id", record.Slot);
    }
    internal void Delete(int slot)
    {
        for (int i = slot + 1; i < NumberOfRecords; i++)
        {
            CopyRecord(i, i - 1);
        }
        SetNumberOfRecords(NumberOfRecords - 1);
    }
    internal int NumberOfRecords => _transaction.GetInt(_block, 4);

    private int GetInt(int slot, string fieldName)
    {
        int pos = FieldPosition(slot, fieldName);
        return _transaction.GetInt(_block, pos);
    }
    private string GetString(int slot, string fieldName)
    {
        int pos = FieldPosition(slot, fieldName);
        return _transaction.GetString(_block, pos);
    }

    private IConstant GetValue(int slot, string fieldName)
    {
        var type = _layout.Schema.GetType(fieldName).Type;
        if (type == 8)
        {
            var i = GetInt(slot, fieldName);
            return new IntegerConstant(i);
        }
        var s = GetString(slot, fieldName);
        return new StringConstant(s);
    }
    private void SetInt(int slot, string fieldName, int value)
    {
        int pos = FieldPosition(slot, fieldName);
        _transaction.SetInt(_block, pos, value, true);
    }
    private void SetString(int slot, string fieldName, string value)
    {
        int pos = FieldPosition(slot, fieldName);
        _transaction.SetString(_block, pos, value, true);
    }

    private void SetValue(int slot, string fieldName, IConstant value)
    {
        var type = value.Type;
        if (type == ConstantType.integer)
        {
            var i = value as IntegerConstant;
            SetInt(slot, fieldName, i.Value);
        }
        if (type == ConstantType.varchar)
        {
            var s = value as StringConstant;
            SetString(slot, fieldName, s.Value);
        }
    }

    private void SetNumberOfRecords(int n)
    {
        _transaction.SetInt(_block, 4, n, true);
    }

    private void Insert(int slot)
    {
        for (int i = NumberOfRecords; i > slot; i--)
        {
            CopyRecord(i - 1, i);
        }
        SetNumberOfRecords(NumberOfRecords + 1);
    }
    private void CopyRecord(int from, int to)
    {
        var schema = _layout.Schema;
        foreach (var field in schema.Fields)
        {
            SetValue(to, field.Name, GetValue(from, field.Name));
        }
    }

    private void TransferRecords(int slot, BTreePage destination)
    {
        int destinationSlot = 0;
        while (slot < NumberOfRecords)
        {
            destination.Insert(destinationSlot);
            var schema = _layout.Schema;
            foreach (var field in schema.Fields)
            {
                destination.SetValue(destinationSlot, field.Name, GetValue(slot, field.Name));
            }
            Delete(slot);
            destinationSlot++;
        }
    }
    private int FieldPosition(int slot, string fieldName)
    {
        int offset = _layout.Offset(fieldName);
        return SlotPosition(slot) + offset;
    }
    private int SlotPosition(int slot)
    {
        int slotSize = _layout.SlotSize;
        return 4 + 4 + (slot * slotSize);
    }
}
