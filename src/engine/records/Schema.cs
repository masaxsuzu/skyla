using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Records;

public class Schema : ISchema
{
    private Dictionary<string, IFieldType> _fields;
    public Schema()
    {
        _fields = new Dictionary<string, IFieldType>();
    }
    public IReadOnlyCollection<IFieldType> Fields => _fields.Values;

    public void AddField(IFieldType type)
    {
        if (!Has(type.Name))
        {
            _fields.Add(type.Name, type);
        }
    }

    public void AddField(string name, ISchema schema)
    {
        if (schema.Has(name))
        {
            AddField(schema.GetType(name));
        }
    }
    public void AddAll(ISchema schema)
    {
        foreach (var field in schema.Fields)
        {
            AddField(field);
        }
    }

    public IFieldType GetType(string name)
    {
        return _fields[name];
    }

    public bool Has(string name)
    {
        return _fields.ContainsKey(name);
    }
}
