using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Metadata;

public class MetadataManager : IMetadataManager
{
    private readonly TableMetadataManager _tables;
    private readonly ViewMetadataManager _views;
    private readonly StatisticsMetadataManager _stats;
    private readonly IndexMetadataManager _indexes;
    public MetadataManager(TableMetadataManager tables, ViewMetadataManager views, StatisticsMetadataManager stats, IndexMetadataManager indexes)
    {
        _tables = tables;
        _views = views;
        _stats = stats;
        _indexes = indexes;
    }

    public void CreateTable(string tableName, ISchema schema, ITransaction transaction)
    {
        _tables.CreateTable(tableName, schema, transaction);
    }

    public ILayout GetLayout(string tableName, ITransaction transaction)
    {
        return _tables.GetLayout(tableName, transaction);
    }

    public void CreateView(string viewName, string viewDefinition, ITransaction transaction)
    {
        _views.CreateView(viewName, viewDefinition, transaction);
    }
    public string GetViewDefinition(string viewName, ITransaction transaction)
    {
        return _views.GetViewDefinition(viewName, transaction);
    }
    public void CreateIndex(string indexName, string tableName, string fieldName, ITransaction transaction)
    {
        _indexes.CreateIndex(indexName, tableName, fieldName, transaction);
    }

    public Dictionary<string, IIndexInfo> GetIndexInfo(string tableName, ITransaction transaction)
    {
        return _indexes.GetIndexInto(tableName, transaction);
    }

    public IStatInfo GetStatInfo(string tableName, ILayout layout, ITransaction transaction)
    {
        return _stats.GetStatInfo(tableName, layout, transaction);
    }


}
