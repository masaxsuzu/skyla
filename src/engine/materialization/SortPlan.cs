using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

public class SortPlan : IPlan
{
    private IPlan _plan;
    private ISchema _schema;
    private ITransaction _transaction;
    private RecordComparer _comp;
    public SortPlan(IPlan plan, List<string> sortedFields, ITransaction transaction)
    {
        _schema = plan.Schema;
        _plan = plan;
        _transaction = transaction;
        _comp = new RecordComparer(sortedFields);
    }
    public int AccessedBlocks => new MaterializedPlan(_transaction, _plan).AccessedBlocks;

    public int Records => _plan.Records;

    public ISchema Schema => _schema;

    public int DistinctValues(string fieldName)
    {
        return _plan.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var src = _plan.Open();
        var runs = SplitIntoRuns(src);
        src.Close();
        while (2 < runs.Count)
        {
            runs = DoAMergeIteration(runs);
        }
        return new SortScan(runs, _comp);
    }
    private List<TempTable> SplitIntoRuns(IScan src)
    {
        var temps = new List<TempTable>();
        src.BeforeFirst();
        if (!src.Next())
        {
            return temps;
        }
        var current = new TempTable(_transaction, _schema);
        temps.Add(current);
        var scan = current.Update();
        while (Copy(src, scan))
        {
            if (_comp.Compare(src, scan) < 0)
            {
                scan.Close();
                current = new TempTable(_transaction, _schema);
                temps.Add(current);
                scan = current.Update();
            }
        }
        scan.Close();
        return temps;
    }

    private List<TempTable> DoAMergeIteration(List<TempTable> runs)
    {
        var ret = new List<TempTable>();
        while (1 < runs.Count)
        {
            var t1 = runs[0];
            var t2 = runs[1];
            runs.RemoveAt(0);
            runs.RemoveAt(0);
            ret.Add(MergeTwoRuns(t1, t2));
        }
        if (runs.Count == 1)
        {
            ret.Add(runs[0]);
        }
        return ret;
    }
    private TempTable MergeTwoRuns(TempTable t1, TempTable t2)
    {
        var src1 = t1.Update();
        var src2 = t2.Update();

        var ret = new TempTable(_transaction, _schema);
        var dest = ret.Update();

        bool hasMore1 = src1.Next();
        bool hasMore2 = src2.Next();

        while (hasMore1 && hasMore2)
        {
            if (_comp.Compare(src1, src2) < 0)
            {
                hasMore1 = Copy(src1, dest);
            }
            else
            {
                hasMore2 = Copy(src2, dest);
            }
        }
        if (hasMore1)
        {
            while (hasMore1)
            {
                hasMore1 = Copy(src1, dest);
            }
        }
        else
        {
            while (hasMore2)
            {
                hasMore2 = Copy(src2, dest);
            }
        }
        src1.Close();
        src2.Close();
        dest.Close();
        return ret;
    }

    private bool Copy(IScan src, IUpdateScan dest)
    {
        dest.Insert();
        foreach (var field in _schema.Fields)
        {
            dest.Set(field.Name, src.Get(field.Name));
        }
        return src.Next();
    }
}

public class RecordComparer : IComparer<IScan>
{
    private List<string> _sortedFields;
    public RecordComparer(List<string> sortedFields)
    {
        _sortedFields = sortedFields;
    }
    public int Compare(IScan? x, IScan? y)
    {
        if (x == null) return -1;
        if (y == null) return 1;

        foreach (var fields in _sortedFields)
        {
            var v1 = x.Get(fields);
            var v2 = x.Get(fields);

            var v = v1.CompareTo(v2);
            if (v != 0)
            {
                return v;
            }
        }
        return 0;
    }
}
