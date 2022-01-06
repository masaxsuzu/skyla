namespace Skyla.Engine.Interfaces;

public interface IMetadataManager
{
    void CreateTable(string tableName, ISchema schema, ITransaction transaction);
    ILayout GetLayout(string tableName, ITransaction transaction);
    void CreateView(string viewName, string viewDefinition, ITransaction transaction);
    string GetViewDefinition(string viewName, ITransaction transaction);
    void CreateIndex(string indexName, string tableName, string fieldName, ITransaction transaction);
    Dictionary<string, IIndexInfo> GetIndexInfo(string tableName, ITransaction transaction);
    IStatInfo GetStatInfo(string tableName, ILayout layout, ITransaction transaction);

}
