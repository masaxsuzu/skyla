using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Records;

public class RecordPage : IRecordPage
{
    private const int empty = 0;
    private const int inUse = 1;
    private readonly ITransaction _transaction;
    private readonly IBlockId _block;
    private readonly ILayout _layout;
    public RecordPage(ITransaction transaction, IBlockId block, ILayout layout)
    {
        _transaction = transaction;
        _block = block;
        _layout = layout;

        _transaction.Pin(block);
    }
    public IBlockId Block => _block;
    public void Delete(int slot)
    {
        SetSlotUsage(slot, empty);
    }

    public void Format()
    {
        int slot = 0;
        while (IsValidSlot(slot))
        {
            _transaction.SetInt(_block, Offset(slot), empty, false);
            var schema = _layout.Schema;
            foreach (var field in schema.Fields)
            {
                int fieldPosition = Offset(slot) + _layout.Offset(field.Name);
                if (schema.GetType(field.Name).Ok(0))
                {
                    _transaction.SetInt(_block, fieldPosition, 0, false);
                }
                else if (schema.GetType(field.Name).Ok(""))
                {
                    _transaction.SetString(_block, fieldPosition, "", false);
                }
            }
            slot++;
        }
    }

    public int GetInt(int slot, string fieldName)
    {
        int fieldPosition = Offset(slot) + _layout.Offset(fieldName);
        return _transaction.GetInt(_block, fieldPosition);
    }
    public string GetString(int slot, string fieldName)
    {
        int fieldPosition = Offset(slot) + _layout.Offset(fieldName);
        return _transaction.GetString(_block, fieldPosition);
    }

    public void SetInt(int slot, string fieldName, int value)
    {
        int fieldPosition = Offset(slot) + _layout.Offset(fieldName);
        _transaction.SetInt(_block, fieldPosition, value, true);
    }

    public void SetString(int slot, string fieldName, string value)
    {
        int fieldPosition = Offset(slot) + _layout.Offset(fieldName);
        _transaction.SetString(_block, fieldPosition, value, true);
    }

    public int InsertAfter(int slot)
    {
        int newSlot = SearchAfter(slot, empty);
        if (newSlot >= 0)
        {
            SetSlotUsage(newSlot, inUse);
        }
        return newSlot;
    }

    public int NextAfter(int slot)
    {
        return SearchAfter(slot, inUse);
    }

    private int Offset(int slot)
    {
        return slot * _layout.SlotSize;
    }
    private bool IsValidSlot(int slot)
    {
        return Offset(slot + 1) <= _transaction.BlockSize;
    }
    private void SetSlotUsage(int slot, int usage)
    {
        _transaction.SetInt(_block, Offset(slot), usage, true);
    }
    private int SearchAfter(int slot, int usage)
    {
        slot++;
        while (IsValidSlot(slot))
        {
            if (_transaction.GetInt(_block, Offset(slot)) == usage)
            {
                return slot;
            }
            slot++;
        }
        // not found;
        return -1;
    }
}

