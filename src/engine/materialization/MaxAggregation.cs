using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

public class MaxAggregation : IAggregation
{
    private string _f;
    private IConstant? _v;
    public MaxAggregation(string field)
    {
        _f = field;
    }
    public string FieldName => $"maxOf${_f}";

    public IConstant? Value => _v;

    public void ProcessFirst(IScan s)
    {
        _v = s.Get(_f);
    }

    public void ProcessNext(IScan s)
    {
        var newValue = s.Get(_f);
        if (0 < newValue.CompareTo(_v))
        {
            _v = newValue;
        }
    }
}
