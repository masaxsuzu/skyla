using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

public class MaterializedPlan : IPlan
{
    private ITransaction _transaction;
    private IPlan _plan;
    
    public MaterializedPlan(ITransaction transaction, IPlan plan)
    {
        _transaction = transaction;
        _plan = plan;
    }
    public int AccessedBlocks {
        get{
            var layout = new Layout(_plan.Schema);
            var rpb = (double) (_transaction.BlockSize / layout.SlotSize);
            return (int) Math.Ceiling(_plan.Records / rpb);
        }
    }

    public int Records => _plan.Records;

    public ISchema Schema => _plan.Schema;

    public int DistinctValues(string fieldName)
    {
        return _plan.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var schema = _plan.Schema;
        var temp = new TempTable(_transaction, schema);
        var src = _plan.Open();
        var dest = temp.Update();
        while (src.Next())
        {
             dest.Insert();
             foreach (var field in schema.Fields)
             {
                 dest.Set(field.Name, src.Get(field.Name));
             }
        }
        src.Close();
        dest.BeforeFirst();
        return dest;
    }
}
