using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
namespace Skyla.Engine.Metadata;

public class ViewMetadataManager
{
    private readonly TableMetadataManager _tables;
    public ViewMetadataManager(bool isNew, TableMetadataManager tables, ITransaction transaction)
    {
        _tables = tables;
        if (isNew)
        {
            var schema = new Schema();
            schema.AddField(new StringFieldType("view_name", 16));
            schema.AddField(new StringFieldType("view_def", 100));
            _tables.CreateTable("view_catalog", schema, transaction);
        }
    }
    public void CreateView(string viewName, string viewDefinition, ITransaction transaction)
    {
        var layout = _tables.GetLayout("view_catalog", transaction);
        var tableCat = new TableScan(transaction, "view_catalog", layout);
        tableCat.Insert();
        tableCat.SetString("view_name", viewName);
        tableCat.SetString("view_def", viewDefinition);
        tableCat.Close();
    }

    public string GetViewDefinition(string viewName, ITransaction transaction)
    {
        var viewDefinition = "";
        var layout = _tables.GetLayout("view_catalog", transaction);
        var tableCat = new TableScan(transaction, "view_catalog", layout);
        while (tableCat.Next())
        {
            if (tableCat.GetString("view_name") == viewName)
            {
                viewDefinition = tableCat.GetString("view_def");
                break;
            }
        }
        tableCat.Close();
        return viewDefinition;
    }
}
