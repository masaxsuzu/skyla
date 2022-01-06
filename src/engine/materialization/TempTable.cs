using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

public class TempTable
{
    private static object _lock = new object();
    private static int _nextTableNumber = 0;
    private ILayout _layout;
    private string _tableName;
    private ITransaction _transaction;
    public TempTable(ITransaction transaction, ISchema schema)
    {
        _transaction = transaction;
        _tableName = NextTableNumber();
        _layout = new Layout(schema);
    }
    public IUpdateScan Update()
    {
        return new TableScan(_transaction, _tableName, _layout);
    }
    public string TableName => _tableName;
    public ILayout Layout => _layout;
    private static string NextTableNumber()
    {
        lock (_lock)
        {
            _nextTableNumber++;
            return $"temp{_nextTableNumber}";
        }
    }
}
