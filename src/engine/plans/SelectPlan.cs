using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Plans;

public class SelectPlan : IWritablePlan
{
    private readonly IPredicate _predicate;
    private readonly IWritablePlan? _planW;
    private readonly IPlan _planR;

    public SelectPlan(IPlan r, IPredicate predicate)
    {
        _predicate = predicate;
        _planR = r;
    }

    public SelectPlan(IPlan r, IWritablePlan w, IPredicate predicate) : this(r, predicate)
    {
        _planW = w;
    }
    public int AccessedBlocks => _planR.AccessedBlocks;
    public int Records => _planR.AccessedBlocks;

    public ISchema Schema => _planR.Schema;

    public int DistinctValues(string fieldName)
    {
        //TODO
        return _planR.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var s = _planR.Open();
        return new SelectScan(s, _predicate);
    }
    public IUpdateScan Update()
    {
#pragma warning disable CS8602
        var s = _planW.Update();
        return new SelectScan(s, s, _predicate);
    }
}
