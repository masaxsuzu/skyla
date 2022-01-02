using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;

namespace Skyla.Engine.Metadata;

public class StatisticsMetadataManager
{
    private readonly TableMetadataManager _tables;
    private Dictionary<string, IStatInfo> _stats;
    private int _numberOfCalls;

    public StatisticsMetadataManager(TableMetadataManager tables, ITransaction transaction)
    {
        _tables = tables;
        _stats = new Dictionary<string, IStatInfo>();
        RefreshStatistics(transaction);
    }

    private void RefreshStatistics(ITransaction transaction)
    {
        _stats = new Dictionary<string, IStatInfo>();
        _numberOfCalls = 0;
        var tableCatalogLayout = _tables.GetLayout("table_catalog", transaction);
        var tableCat = new TableScan(transaction, "table_catalog", tableCatalogLayout);
        while (tableCat.Next())
        {
            var tableName = tableCat.GetString("table_name");
            var layout = _tables.GetLayout(tableName, transaction);
            IStatInfo si = CalculateStats(tableName, layout, transaction);
            _stats.Put(tableName, si);
        }
        tableCat.Close();
    }
    public IStatInfo GetStatInfo(string tableName, ILayout layout, ITransaction transaction)
    {
        lock (this)
        {
            _numberOfCalls++;
            if (_numberOfCalls > 100)
            {
                RefreshStatistics(transaction);
            }
            if (_stats.ContainsKey(tableName))
            {
                return _stats[tableName];
            }
            else
            {
                var si = CalculateStats(tableName, layout, transaction);
                _stats.Put(tableName, si);
                return si;
            }
        }
    }
    private IStatInfo CalculateStats(string tableName, ILayout layout, ITransaction transaction)
    {
        int numOfRecords = 0;
        int numOfBlocks = 0;
        var tableCat = new TableScan(transaction, tableName, layout);
        while (tableCat.Next())
        {
            numOfRecords++;
            numOfBlocks = tableCat.Record.BlockNumber + 1;
        }
        tableCat.Close();
        return new StatInfo(numOfBlocks, numOfRecords);
    }
}
