using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
namespace Skyla.Engine.Metadata;

public class TableMetadataManager
{
    private readonly ILayout _tableLayout;
    private readonly ILayout _fieldLayout;
    public TableMetadataManager(bool isNew, ITransaction transaction)
    {
        var tableCatalogs = new Schema();
        tableCatalogs.AddField(new StringFieldType("table_name", 16));
        tableCatalogs.AddField(new IntegerFieldType("slot_size"));
        _tableLayout = new Layout(tableCatalogs);

        var fieldCatalogs = new Schema();
        fieldCatalogs.AddField(new StringFieldType("table_name", 16));
        fieldCatalogs.AddField(new StringFieldType("field_name", 16));
        fieldCatalogs.AddField(new IntegerFieldType("type"));
        fieldCatalogs.AddField(new IntegerFieldType("length"));
        fieldCatalogs.AddField(new IntegerFieldType("offset"));

        _fieldLayout = new Layout(fieldCatalogs);

        if (isNew)
        {
            CreateTable("table_catalog", tableCatalogs, transaction);
            CreateTable("field_catalog", fieldCatalogs, transaction);
        }
    }

    public void CreateTable(string tableName, ISchema schema, ITransaction transaction)
    {
        var layout = new Layout(schema);
        var tableCat = new TableScan(transaction, "table_catalog", _tableLayout);
        tableCat.Insert();
        tableCat.SetString("table_name", tableName);
        tableCat.SetInt("slot_size", layout.SlotSize);
        tableCat.Close();

        var fieldCat = new TableScan(transaction, "field_catalog", _fieldLayout);

        foreach (var field in schema.Fields)
        {
            fieldCat.Insert();
            fieldCat.SetString("table_name", tableName);
            fieldCat.SetString("field_name", field.Name);
            fieldCat.SetInt("type", field.Type);
            fieldCat.SetInt("length", field.ByteSize);
            fieldCat.SetInt("offset", layout.Offset(field.Name));
        }
        fieldCat.Close();
    }

    public ILayout GetLayout(string tableName, ITransaction transaction)
    {
        int size = -1;
        var tableCat = new TableScan(transaction, "table_catalog", _tableLayout);
        while (tableCat.Next())
        {
            if (tableCat.GetString("table_name") == tableName)
            {
                size = tableCat.GetInt("slot_size");
                break;
            }
        }
        tableCat.Close();

        var schema = new Schema();
        var offsets = new Dictionary<string, int>();
        var fieldCat = new TableScan(transaction, "field_catalog", _fieldLayout);
        while (fieldCat.Next())
        {
            if (fieldCat.GetString("table_name") == tableName)
            {
                var name = fieldCat.GetString("field_name");
                var type = fieldCat.GetInt("type");
                var len = fieldCat.GetInt("length");
                var offset = fieldCat.GetInt("offset");
                offsets.Put(name, offset);
                switch (type)
                {
                    case 8:
                        schema.AddField(new IntegerFieldType(name));
                        break;
                    case 22:
                        schema.AddField(new StringFieldType(name, len));
                        break;
                    default:
                        throw new Exceptions.EngineException($"unsupported type {type}");
                }
            }
        }
        fieldCat.Close();
        return new Layout(schema, offsets, size);
    }
}
