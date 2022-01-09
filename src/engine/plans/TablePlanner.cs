using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Plans;

public class TablePlanner
{
    private TablePlan _plan;
    private IPredicate _pred;
    private ISchema _s;
    private Dictionary<string, IIndexInfo> _indexes = new Dictionary<string, IIndexInfo>();
    private ITransaction _t;
    public TablePlanner(string name, IPredicate pred, ITransaction transaction, IMetadataManager metadata)
    {
        _pred = pred;
        _t = transaction;
        _plan = new TablePlan(_t, name, metadata);
        _s = _plan.Schema;
        _indexes = metadata.GetIndexInfo(name, _t);
    }

    public IPlan MakeSelectPlan()
    {
        var p = MakeIndexedSelect();
        if (p == null)
        {
            p = _plan;
        }

        return AddSelectPredicate(p);
    }

    public IPlan? MakeJoinPlan(IPlan current)
    {
        var s = current.Schema;
        var jp = _pred.JoinSubPredicate(_s, s);
        if (jp.Terms.Length == 0)
        {
            return null;
        }
        var p = MakeIndexedJoin(current, s);
        if (p == null)
        {
            p = MakeProductJoin(current, s);
        }
        return p;
    }

    public IPlan MakeProductPlan(IPlan current)
    {
        var p = AddSelectPredicate(_plan);
        return new MultiBuffers.MultiBufferProductPlan(_t, current, p);
    }
    private IPlan? MakeIndexedSelect()
    {
        foreach (var f in _indexes.Keys)
        {
            var v = _pred.EquatesWitConstant(f);
            if (v != null)
            {
                var index = _indexes[f];
                return new IndexedSelectPlan(_plan, index, v);
            }
        }
        return null;
    }

    private IPlan? MakeIndexedJoin(IPlan current, ISchema s)
    {
        foreach (var f in _indexes.Keys)
        {
            var outer = _pred.EquatesWithField(f);
            if (outer != null && s.Has(f))
            {
                var index = _indexes[f];
                IPlan p = new IndexedJoinPlan(current, _plan, index, f);
                p = AddSelectPredicate(p);
                return AddJoinPredicate(p, s);
            }
        }
        return null;
    }

    private IPlan MakeProductJoin(IPlan current, ISchema s)
    {
        var p = MakeProductPlan(current);
        return AddJoinPredicate(p, s);
    }

    private IPlan AddSelectPredicate(IPlan p)
    {
        var selected = _pred.SelectSubPredicate(_s);
        if (selected.Terms.Length != 0)
        {
            return new SelectPlan(p, selected);
        }
        return p;
    }
    private IPlan AddJoinPredicate(IPlan p, ISchema s)
    {
        var joined = _pred.JoinSubPredicate(s, _s);
        if (joined.Terms.Length != 0)
        {
            return new SelectPlan(p, joined);
        }
        return p;
    }
}
