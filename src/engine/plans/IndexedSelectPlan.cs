using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Plans;

public class IndexedSelectPlan : IPlan
{
    readonly TablePlan _plan;
    readonly IIndexInfo _info;
    readonly IConstant _key;
    public IndexedSelectPlan(TablePlan p, IIndexInfo info, IConstant key)
    {
        _plan = p;
        _info = info;
        _key = key;
    }
    public int AccessedBlocks => _info.AccessedBlocks + Records;

    public int Records => _info.Records;

    public ISchema Schema => _plan.Schema;

    public int DistinctValues(string fieldName)
    {
        return _info.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var scan = _plan.ScanTable();
        var index = _info.Open();
        return new IndexedSelectScan(scan, index, _key);
    }
}
