using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
namespace Skyla.Engine.Scans;

public class SelectScan : IUpdateScan
{
    private readonly IScan _scan;
    private readonly IUpdateScan? _scanW;
    private readonly IPredicate _predicate;
    public SelectScan(IScan scan, IPredicate predicate)
    {
        _scan = scan;
        _predicate = predicate;
    }
    public SelectScan(IScan scan, IUpdateScan scanW, IPredicate predicate) : this(scan, predicate)
    {
        _scanW = scanW;
    }
    public bool Next()
    {
        while (_scan.Next())
        {
            if (_predicate.IsSatisfied(_scan))
            {
                return true;
            }
        }
        return false;
    }

#pragma warning disable CS8602
    public IRecordId Record => _scanW.Record;

    public void BeforeFirst()
    {
        _scan.BeforeFirst();
    }

    public void Close()
    {
        _scan.Close();
    }

    public void Delete()
    {
        _scanW.Delete();
    }

    public IConstant Get(string fieldName)
    {
        return _scan.Get(fieldName);
    }

    public int GetInt(string fieldName)
    {
        return _scan.GetInt(fieldName);
    }

    public string GetString(string fieldName)
    {
        return _scan.GetString(fieldName);
    }

    public void Set(string fieldName, IConstant value)
    {
#pragma warning disable CS8602
        _scanW.Set(fieldName, value);
    }

    public void SetInt(string fieldName, int value)
    {
#pragma warning disable CS8602
        _scanW.SetInt(fieldName, value);
    }

    public void SetString(string fieldName, string value)
    {
#pragma warning disable CS8602
        _scanW.SetString(fieldName, value);
    }

    public bool HasField(string name)
    {
        return _scan.HasField(name);
    }

    public void Insert()
    {
#pragma warning disable CS8602
        _scanW.Insert();
    }

    public void MoveTo(IRecordId r)
    {
#pragma warning disable CS8602
        _scanW.MoveTo(r);
    }
}
