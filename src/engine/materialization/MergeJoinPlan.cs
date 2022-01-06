using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

public class MergeJoinPlan : IPlan
{
    SortPlan _p1, _p2;
    string _f1, _f2;
    ISchema _s;
    public MergeJoinPlan(IPlan p1, IPlan p2, string f1, string f2, ITransaction transaction)
    {
        _p1 = new SortPlan(p1, new List<string>() { f1 }, transaction);
        _p2 = new SortPlan(p2, new List<string>() { f2 }, transaction);
        _f1 = f1;
        _f2 = f2;
        _s = new Schema();
        _s.AddAll(p1.Schema);
        _s.AddAll(p2.Schema);

    }
    public int AccessedBlocks => _p1.AccessedBlocks + _p2.AccessedBlocks;

    public int Records
    {
        get
        {
            var max = Math.Max(_p1.DistinctValues(_f1), _p2.DistinctValues(_f2));
            return (_p1.Records * _p2.Records) / max;
        }
    }

    public ISchema Schema => _s;

    public int DistinctValues(string fieldName)
    {
        throw new NotImplementedException();
    }

    public IScan Open()
    {
        var s1 = _p1.Scan();
        var s2 = _p2.Scan();
        return new MergeJoinScan(s1, s2, _f1, _f2);
    }
}
