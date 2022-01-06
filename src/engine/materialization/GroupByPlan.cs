using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

public class GroupByPlan : IPlan
{
    private IPlan _plan;
    private List<string> _f;
    private List<IAggregation> _a;

    private ISchema _schema;
    public GroupByPlan(IPlan plan, List<string> fields, List<IAggregation> aggregations, ITransaction transaction)
    {
        _plan = new SortPlan(plan, fields, transaction);
        _f = fields;
        _schema = new Schema();
        foreach (var f in fields)
        {
            _schema.AddField(f, _plan.Schema);
        }
        _a = aggregations;
        foreach (var fn in _a)
        {
            _schema.AddField(new IntegerFieldType(fn.FieldName));
        }
    }

    public int AccessedBlocks => _plan.AccessedBlocks;

    public int Records
    {
        get
        {
            var n = 1;
            foreach (var f in _f)
            {
                n *= _plan.DistinctValues(f);
            }
            return n;
        }
    }

    public ISchema Schema => _schema;

    public int DistinctValues(string fieldName)
    {
        if (_plan.Schema.Has(fieldName))
        {
            return _plan.DistinctValues(fieldName);
        }
        return Records;
    }

    public IScan Open()
    {
        var s = _plan.Open();
        return new GroupByScan(s, _f, _a);
    }
}
