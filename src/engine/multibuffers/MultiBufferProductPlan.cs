using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.MultiBuffers;

public class MultiBufferProductPlan : IPlan
{
    ITransaction _t;
    IPlan _l, _r;
    ISchema _s;
    public MultiBufferProductPlan(ITransaction transaction, IPlan left, IPlan right)
    {
        _t = transaction;
        _l = left;
        _r = right;
        _s = new Schema();
        _s.AddAll(left.Schema);
        _s.AddAll(right.Schema);
    }

    public int AccessedBlocks
    {
        get
        {
            var a = _t.AvailableBuffers;
            var size = new Materialization.MaterializedPlan(_t, _r).AccessedBlocks;
            var n = size / a;
            return _r.AccessedBlocks + (_l.AccessedBlocks * n);
        }
    }

    public int Records => _l.Records * _r.Records;

    public ISchema Schema => _s;

    public int DistinctValues(string fieldName)
    {
        if (_l.Schema.Has(fieldName))
        {
            return _l.DistinctValues(fieldName);
        }
        return _r.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var ls = _l.Open();
        var temp = CopyRecordsFrom(_r);
        return new MultiBufferProductScan(_t, ls, temp.TableName, temp.Layout);
    }

    private Materialization.TempTable CopyRecordsFrom(IPlan p)
    {
        var src = p.Open();
        var s = p.Schema;
        var temp = new Materialization.TempTable(_t, s);
        var dest = temp.Update();
        while (src.Next())
        {
            dest.Insert();
            foreach (var f in s.Fields)
            {
                dest.Set(f.Name, src.Get(f.Name));
            }
        }
        src.Close();
        dest.Close();
        return temp;
    }
}
