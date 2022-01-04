using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Indexes;

public class HashIndex : IIndexable
{
    readonly ITransaction _transaction;
    readonly string _indexName;
    readonly ILayout _layout;
    TableScan? _scan;
    IConstant? _key;
    public HashIndex(ITransaction transaction, string indexName, ILayout layout)
    {
        _transaction = transaction;
        _indexName = indexName;
        _layout = layout;
    }

#pragma warning disable CS8602
    public IRecordId Record
    {
        get
        {
            int block = _scan.GetInt("block");
            int id = _scan.GetInt("id");
            return new RecordId(block, id);
        }
    }

    public static int SearchCost(int numOfBlocks, int recordsPerBlock)
    {
        return numOfBlocks / 100;
    }

    public void BeforeFirst(IConstant constant)
    {
        _BeforeFirst(constant);
    }

    private TableScan _BeforeFirst(IConstant constant)
    {
        Close();
        _key = constant;
        int bucket = _key.GetHashCode();
        _scan = new TableScan(_transaction, $"{_indexName}{bucket}", _layout);
        return _scan;
    }

    public void Close()
    {
        if (_scan != null)
        {
            _scan.Close();
        }
    }

    public void Delete(IConstant data, IRecordId record)
    {
        _scan = _BeforeFirst(data);
        while (_scan.Next())
        {
            if (Record == record)
            {
                _scan.Delete();
                return;
            }
        }
    }

    public void Insert(IConstant data, IRecordId record)
    {
        _scan = _BeforeFirst(data);
        _scan.Insert();
        _scan.SetInt("block", record.BlockNumber);
        _scan.SetInt("id", record.Slot);
        _scan.Set("value", data);
    }

    public bool Next()
    {
        if (_scan == null) return false;
        while (_scan.Next())
        {
            if (_scan.Get("id") == _key)
            {
                return true;
            }
        }
        return false;
    }
}
