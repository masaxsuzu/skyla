using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

public class MergeJoinScan : IScan
{
    IScan _s1;
    SortScan _s2;
    string _f1, _f2;
    IConstant? _v;
    public MergeJoinScan(IScan s1, SortScan s2, string field1, string field2)
    {
        _s1 = s1;
        _s2 = s2;
        _f1 = field1;
        _f2 = field2;
        BeforeFirst();

    }
    public void BeforeFirst()
    {
        _s1.BeforeFirst();
        _s2.BeforeFirst();
    }

    public void Close()
    {
        _s1.Close();
        _s2.Close();
    }

    public IConstant Get(string fieldName)
    {
        return _s1.HasField(fieldName) ? _s1.Get(fieldName) : _s2.Get(fieldName);
    }

    public int GetInt(string fieldName)
    {
        return _s1.HasField(fieldName) ? _s1.GetInt(fieldName) : _s2.GetInt(fieldName);
    }

    public string GetString(string fieldName)
    {
        return _s1.HasField(fieldName) ? _s1.GetString(fieldName) : _s2.GetString(fieldName);
    }

    public bool HasField(string fieldName)
    {
        return _s1.HasField(fieldName) || _s2.HasField(fieldName);
    }

    public bool Next()
    {
        var hasMore2 = _s2.Next();
        if (hasMore2 && _s2.Get(_f2).Equals(_v))
        {
            return true;
        }
        var hasMore1 = _s1.Next();
        if (hasMore1 && _s1.Get(_f1).Equals(_v))
        {
            _s2.RestorePositions();
            return true;
        }

        while (hasMore1 && hasMore2)
        {
            var v1 = _s1.Get(_f1);
            var v2 = _s2.Get(_f2);

            var c = v1.CompareTo(v2);
            if (c < 0)
            {
                hasMore1 = _s1.Next();
            }
            else if (0 < c)
            {
                hasMore2 = _s2.Next();
            }
            else
            {
                _s2.SavePositions();
                _v = _s2.Get(_f2);
                return true;
            }
        }
        return false;
    }
}
