using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

#pragma warning disable CS8602
#pragma warning disable CS8603
public class GroupByScan : IScan
{
    IScan _s;
    List<string> _f;
    List<IAggregation> _a;
    GroupValue? _v;
    bool _moreGroup;
    public GroupByScan(IScan s, List<string> fields, List<IAggregation> aggregations)
    {
        _s = s;
        _f = fields;
        _a = aggregations;
        BeforeFirst();
    }
    public void BeforeFirst()
    {
        _s.BeforeFirst();
        _moreGroup = _s.Next();
    }

    public void Close()
    {
        _s.Close();
    }

    public IConstant Get(string fieldName)
    {
        if (_f.Contains(fieldName))
        {
            return _v.Get(fieldName);
        }
        foreach (var fn in _a)
        {
            if (fn.FieldName == fieldName)
            {
                return fn.Value;
            }
        }
        throw new Engine.Exceptions.EngineException($"no field {fieldName} found");
    }

    public int GetInt(string fieldName)
    {
        var v = Get(fieldName);
        if (v.Type == ConstantType.integer)
        {
            return (v as IntegerConstant).Value;
        }
        throw new Engine.Exceptions.EngineException($"unreachable");
    }

    public string GetString(string fieldName)
    {
        var v = Get(fieldName);
        if (v.Type == ConstantType.varchar)
        {
            return (v as StringConstant).Value;
        }
        throw new Engine.Exceptions.EngineException($"unreachable");
    }

    public bool HasField(string fieldName)
    {
        if (_f.Contains(fieldName))
        {
            return true;
        }
        foreach (var fn in _a)
        {
            if (fn.FieldName == fieldName)
            {
                return true;
            }
        }
        return false;
    }

    public bool Next()
    {
        if (!_moreGroup)
        {
            return false;
        }
        foreach (var fn in _a)
        {
            fn.ProcessFirst(_s);
        }
        _v = new GroupValue(_s, _f);
        while (_moreGroup = _s.Next())
        {
            var gv = new GroupValue(_s, _f);
            if (!_v.Equals(gv))
            {
                break;
            }
            foreach (var fn in _a)
            {
                fn.ProcessNext(_s);
            }
        }
        return true;
    }
}
