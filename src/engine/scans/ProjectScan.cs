using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Scans;

public class ProjectScan : IScan
{
    private readonly IScan _scan;
    private readonly IReadOnlyCollection<string> _fields;
    public ProjectScan(IScan scan, IReadOnlyCollection<string> fields)
    {
        _scan = scan;
        _fields = fields;
    }
    public bool Next()
    {
        return _scan.Next();
    }

    public void BeforeFirst()
    {
        _scan.BeforeFirst();
    }

    public void Close()
    {
        _scan.Close();
    }

    public IConstant Get(string fieldName)
    {
        if (!HasField(fieldName)) throw new Exceptions.EngineException($"{fieldName} is not found");
        return _scan.Get(fieldName);
    }

    public int GetInt(string fieldName)
    {
        if (!HasField(fieldName)) throw new Exceptions.EngineException($"{fieldName} is not found");
        return _scan.GetInt(fieldName);
    }

    public string GetString(string fieldName)
    {
        if (!HasField(fieldName)) throw new Exceptions.EngineException($"{fieldName} is not found");
        return _scan.GetString(fieldName);
    }

    public bool HasField(string name)
    {
        return _fields.Contains(name);
    }
}
