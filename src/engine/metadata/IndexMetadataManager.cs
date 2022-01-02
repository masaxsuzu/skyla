using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Metadata;

public class IndexMetadataManager
{
    private readonly TableMetadataManager _tables;
    private readonly StatisticsMetadataManager _stats;
    private readonly ILayout _layout;

    public IndexMetadataManager(bool isNew, TableMetadataManager tables, StatisticsMetadataManager stats, ITransaction transaction)
    {
        if (isNew)
        {
            var schema = new Schema();
            schema.AddField(new StringFieldType("index_name", 16));
            schema.AddField(new StringFieldType("table_name", 16));
            schema.AddField(new StringFieldType("field_name", 16));
            tables.CreateTable("index_catalog", schema, transaction);
        }
        _tables = tables;
        _stats = stats;
        _layout = tables.GetLayout("index_catalog", transaction);
    }

    public void CreateIndex(string indexName, string tableName, string fieldName, ITransaction transaction)
    {
        var tableCat = new TableScan(transaction, "index_catalog", _layout);
        tableCat.Insert();
        tableCat.SetString("index_name", indexName);
        tableCat.SetString("table_name", tableName);
        tableCat.SetString("field_name", fieldName);

        tableCat.Close();
    }

    public Dictionary<string, IIndexInfo> GetIndexInto(string tableName, ITransaction transaction)
    {
        var ret = new Dictionary<string, IIndexInfo>();
        var tableCat = new TableScan(transaction, "index_catalog", _layout);

        while (tableCat.Next())
        {
            if (tableCat.GetString("table_name") == tableName)
            {
                var indexName = tableCat.GetString("index_name");
                var fieldName = tableCat.GetString("field_name");
                var layout = _tables.GetLayout(tableName, transaction);
                var si = _stats.GetStatInfo(tableName, layout, transaction);
                var index = new IndexInfo(indexName, fieldName, layout.Schema, transaction, si);
                ret.Put(fieldName, index);
            }
        }

        tableCat.Close();
        return ret;
    }
}
