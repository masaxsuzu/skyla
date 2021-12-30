using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Records;

public class TableScan : ITableScan
{
    private readonly ITransaction _transaction;
    private readonly ILayout _layout;
    private readonly string _tableFileName;
    private IRecordPage _recordPage;
    private int _currentSlot;
    public TableScan(ITransaction transaction, string tableName, ILayout layout)
    {
        _transaction = transaction;
        _tableFileName = $"{tableName}.tbl";
        _layout = layout;
        if (transaction.Size(_tableFileName) == 0)
        {
            _recordPage = MoveToNewBlock();
        }
        else
        {
            _recordPage = MoveToBlock(0);
        }
    }
    public bool Next()
    {
        _currentSlot = _recordPage.NextAfter(_currentSlot);
        while (_currentSlot < 0)
        {
            if (AtLastBlock())
            {
                return false;
            }
            MoveToBlock(_recordPage.Block.Number + 1);
            _currentSlot = _recordPage.NextAfter(_currentSlot);
        }
        return true;
    }

    public IRecordId Record => new RecordId(_recordPage.Block.Number, _currentSlot);

    public void BeforeFirst()
    {
        MoveToBlock(0);
    }

    public void Close()
    {
        if (_recordPage != null)
        {
            _transaction.UnPin(_recordPage.Block);
        }
    }

    public void Delete()
    {
        _recordPage.Delete(_currentSlot);
    }

    public int GetInt(string fieldName)
    {
        return _recordPage.GetInt(_currentSlot, fieldName);
    }

    public string GetString(string fieldName)
    {
        return _recordPage.GetString(_currentSlot, fieldName);
    }

    public void SetInt(string fieldName, int value)
    {
        _recordPage.SetInt(_currentSlot, fieldName, value);
    }

    public void SetString(string fieldName, string value)
    {
        _recordPage.SetString(_currentSlot, fieldName, value);
    }

    public bool HasField(string name)
    {
        return _layout.Schema.Has(name);
    }

    public void Insert()
    {
        _currentSlot = _recordPage.InsertAfter(_currentSlot);
        while (_currentSlot < 0)
        {
            if (AtLastBlock())
            {
                MoveToNewBlock();
            }
            else
            {
                MoveToBlock(_recordPage.Block.Number + 1);
            }
            _currentSlot = _recordPage.InsertAfter(_currentSlot);
        }
    }

    public void MoveTo(IRecordId r)
    {
        Close();
        var block = new BlockId(_tableFileName, r.BlockNumber);
        _recordPage = new RecordPage(_transaction, block, _layout);
        _currentSlot = r.Slot;
    }

    private IRecordPage MoveToBlock(int number)
    {
        Close();
        var block = new BlockId(_tableFileName, number);
        _recordPage = new RecordPage(_transaction, block, _layout);
        _currentSlot = -1;
        return _recordPage;
    }
    private IRecordPage MoveToNewBlock()
    {
        Close();
        var block = _transaction.Append(_tableFileName);
        _recordPage = new RecordPage(_transaction, block, _layout);
        _recordPage.Format();
        _currentSlot = -1;
        return _recordPage;
    }

    private bool AtLastBlock()
    {
        return _recordPage.Block.Number == _transaction.Size(_tableFileName) - 1;
    }
}
