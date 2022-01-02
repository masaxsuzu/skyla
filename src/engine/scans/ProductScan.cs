using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Scans;

public class ProductScan : IScan
{
    private readonly IScan _scan1;
    private readonly IScan _scan2;
    public ProductScan(IScan scan1, IScan scan2)
    {
        _scan1 = scan1;
        _scan2 = scan2;
        _scan1.Next();
    }
    public bool Next()
    {
        if (_scan2.Next())
        {
            return true;
        }
        else
        {
            _scan2.BeforeFirst();
            return _scan2.Next() && _scan1.Next();
        }
    }

    public void BeforeFirst()
    {
        _scan1.BeforeFirst();
        _scan1.Next();
        _scan2.BeforeFirst();
    }

    public void Close()
    {
        _scan1.Close();
        _scan2.Close();
    }

    public IConstant Get(string fieldName)
    {
        return _scan1.HasField(fieldName) ? _scan1.Get(fieldName) : _scan2.Get(fieldName);
    }

    public int GetInt(string fieldName)
    {
        return _scan1.HasField(fieldName) ? _scan1.GetInt(fieldName) : _scan2.GetInt(fieldName);
    }

    public string GetString(string fieldName)
    {
        return _scan1.HasField(fieldName) ? _scan1.GetString(fieldName) : _scan2.GetString(fieldName);
    }

    public bool HasField(string name)
    {
        return _scan1.HasField(name) || _scan2.HasField(name);
    }
}
