using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
namespace Skyla.Engine.Scans;

public class IndexedJoinScan : IScan
{
    readonly IScan _left;
    readonly TableScan _right;
    readonly IIndexable _index;
    readonly string _field;
    public IndexedJoinScan(IScan left, IIndexable index, string field, TableScan right)
    {
        _left = left;
        _right = right;
        _index = index;
        _field = field;

    }

    public void BeforeFirst()
    {
        _left.BeforeFirst();
        _left.Next();
        _right.BeforeFirst();
    }

    public void Close()
    {
        _left.Close();
        _index.Close();
        _right.Close();
    }

    public IConstant Get(string fieldName)
    {
        if (_right.HasField(fieldName))
        {
            return _right.Get(fieldName);
        }
        else
        {
            return _left.Get(fieldName);
        }
    }

    public int GetInt(string fieldName)
    {
        if (_right.HasField(fieldName))
        {
            return _right.GetInt(fieldName);
        }
        else
        {
            return _left.GetInt(fieldName);
        }
    }

    public string GetString(string fieldName)
    {
        if (_right.HasField(fieldName))
        {
            return _right.GetString(fieldName);
        }
        else
        {
            return _left.GetString(fieldName);
        }
    }

    public bool HasField(string fieldName)
    {
        return _right.HasField(fieldName) || _left.HasField(fieldName);
    }

    public bool Next()
    {
        while (true)
        {
            if (_index.Next())
            {
                _right.MoveTo(_index.Record);
                return true;
            }
            if (!_left.Next())
            {
                return false;
            }
            ResetIndex();
        }
    }
    private void ResetIndex()
    {
        var key = _left.Get(_field);
        _index.BeforeFirst(key);
    }
}
