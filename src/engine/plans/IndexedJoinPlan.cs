using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Plans;

public class IndexedJoinPlan : IPlan
{
    readonly IPlan _p1;
    readonly TablePlan _p2;
    readonly IIndexInfo _info;
    readonly string _field;
    readonly ISchema _schema;
    public IndexedJoinPlan(IPlan plan1, TablePlan plan2, IIndexInfo info, string joinField)
    {
        _field = joinField;
        _info = info;
        _p1 = plan1;
        _p2 = plan2;
        _schema = new Schema();
        _schema.AddAll(_p1.Schema);
        _schema.AddAll(_p2.Schema);
    }
    public int AccessedBlocks => _p1.AccessedBlocks + (_p1.Records * _info.AccessedBlocks) + Records;

    public int Records => _p1.Records + _info.Records;

    public ISchema Schema => _schema;

    public int DistinctValues(string fieldName)
    {
        if (_p1.Schema.Has(fieldName))
        {
            return _p1.DistinctValues(fieldName);
        }
        else
        {
            return _p2.DistinctValues(fieldName);
        }
    }

    public IScan Open()
    {
        var s = _p1.Open();
        var ts = _p2.ScanTable();
        var index = _info.Open();
        return new IndexedJoinScan(s, index, _field, ts);
    }
}
