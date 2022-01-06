using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

public class GroupValue : IEquatable<GroupValue>
{
    private Dictionary<string, IConstant> _v = new Dictionary<string, IConstant>();
    public GroupValue(IScan s, List<string> fields)
    {
        foreach (var f in fields)
        {
            _v.Put(f, s.Get(f));
        }
    }

    public bool Equals(GroupValue? other)
    {
        if (other == null) return false;
        foreach (var f in _v.Keys)
        {
            var v1 = Get(f);
            var v2 = other.Get(f);
            if (!v1.Equals(v2))
            {
                return false;
            }
        }
        return true;
    }

    public IConstant Get(string f)
    {
        return _v[f];
    }

}
