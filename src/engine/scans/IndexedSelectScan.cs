using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
namespace Skyla.Engine.Scans;

public class IndexedSelectScan : IScan
{
    readonly TableScan _scan;
    readonly IIndexable _index;
    readonly IConstant _key;
    public IndexedSelectScan(TableScan scan, IIndexable index, IConstant key)
    {
        _scan = scan;
        _index = index;
        _key = key;
        BeforeFirst();
    }
    public void BeforeFirst()
    {
        _index.BeforeFirst(_key);
    }

    public void Close()
    {
        _index.Close();
        _scan.Close();
    }

    public IConstant Get(string fieldName)
    {
        return _scan.Get(fieldName);
    }

    public int GetInt(string fieldName)
    {
        return _scan.GetInt(fieldName);
    }

    public string GetString(string fieldName)
    {
        return _scan.GetString(fieldName);
    }

    public bool HasField(string fieldName)
    {
        return _scan.HasField(fieldName);
    }

    public bool Next()
    {
        var ok = _index.Next();
        if (ok)
        {
            var record = _index.Record;
            _scan.MoveTo(record);
        }
        return ok;
    }
}
