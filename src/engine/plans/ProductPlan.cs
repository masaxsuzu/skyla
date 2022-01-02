using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Plans;

public class ProductPlan : IPlan
{
    private readonly IPlan _plan1;
    private readonly IPlan _plan2;
    private readonly ISchema _schema = new Schema();

    public ProductPlan(IPlan plan1, IPlan plan2)
    {
        _plan1 = plan1;
        _plan2 = plan2;
        _schema.AddAll(plan1.Schema);
        _schema.AddAll(plan2.Schema);
    }
    public int AccessedBlocks => _plan1.AccessedBlocks + (_plan1.Records * _plan2.AccessedBlocks);
    public int Records => _plan1.Records * _plan2.Records;

    public ISchema Schema => _schema;

    public int DistinctValues(string fieldName)
    {
        if (_plan1.Schema.Has(fieldName))
        {
            return _plan1.DistinctValues(fieldName);
        }
        else
        {
            return _plan2.DistinctValues(fieldName);
        }
    }

    public IScan Open()
    {
        var s1 = _plan1.Open();
        var s2 = _plan2.Open();
        return new ProductScan(s1, s2);
    }
}
