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

    public int DistinctValues(string fieldName1)
    {
        if (_predicate.EquatesWitConstant(fieldName1) != null)
        {
            return 1;
        }
        var fieldName2 = _predicate.EquatesWithField(fieldName1);
        if (fieldName2 != null)
        {
            return Math.Min(_planR.DistinctValues(fieldName1), _planR.DistinctValues(fieldName2));
        }
        else
        {
            return _planR.DistinctValues(fieldName1);
        }
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
