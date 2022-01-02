using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Plans;

public class ProjectPlan : IPlan
{
    private readonly IPlan _plan;
    private readonly string[] _fields;

    public ProjectPlan(IPlan plan, string[] fields)
    {
        _plan = plan;
        _fields = fields;
    }
    public int AccessedBlocks => _plan.AccessedBlocks;
    public int Records => _plan.AccessedBlocks;

    public ISchema Schema => _plan.Schema;

    public int DistinctValues(string fieldName)
    {
        return _plan.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        var s = _plan.Open();
        return new ProjectScan(s, _fields);
    }
}
